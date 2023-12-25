using CfgStore.Application.Abstractions;
using CliWrap;

namespace CfgStore.Modules.Conditional;

public class ConditionalStepProvider<RT> : IPipelineStepProvider<RT>
    where RT : struct, HasCancel<RT>
{
    private PipelineStep<RT> Step { get; } =
        (store, config, configs, next) =>
            from cfg in ParseConfig(config.Value)
            let os = Environment.OSVersion
            from cmd in GetCommand(cfg, os)
            from shell in GetPlatformSpecificShell(os)
            from result in ExecCommand(shell, cmd)
            from _ in RunConditionalNext(result != cfg.Invert, next, store, configs)
            select unit;

    public PipelineStep<RT> Load => Step;

    public Seq<string> Names => Seq("conditional", "cond", "if");

    public PipelineStep<RT> Store => Step;

    private static Aff<RT, bool> ExecCommand(string shell, string command) =>
        Aff(async (RT rt) => await Cli.Wrap(shell)
                .WithArguments("-c -")
                .WithStandardInputPipe(PipeSource.FromString(command))
                .WithStandardOutputPipe(PipeTarget.ToStream(Console.OpenStandardOutput()))
                .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync(rt.CancellationToken))
            .Map(x => x.ExitCode == 0);

    private static Eff<string> GetCommand(Config cfg, OperatingSystem os) =>
        GetPlatformSpecificCommand(cfg, os).ToEff()
            .Catch(_ => cfg.Command.ToEff(Error.New("No command applicable.")));

    private static Option<string> GetPlatformSpecificCommand(Config cfg, OperatingSystem os) =>
        os.Platform switch
        {
            PlatformID.Win32NT => cfg.WindowsCommand,
            PlatformID.Unix => cfg.UnixCommand,
            _ => Option<string>.None,
        };

    private static Eff<string> GetPlatformSpecificShell(OperatingSystem os) =>
        os.Platform switch
        {
            PlatformID.Win32NT => SuccessEff("pwsh"),
            PlatformID.Unix => SuccessEff("bash"),
            _ => FailEff<string>($"{os.VersionString} is currently not supported."),
        };

    private static Eff<Config> ParseConfig(ConfigEntry configEntry) =>
        from invert in Eff(() => configEntry.Get("invert").Get().IfNone("false").Apply(bool.Parse))
        select new Config(
            invert,
            configEntry.Get("cmd").Get(),
            configEntry.Get("win").Get(),
            configEntry.Get("unix").Get()
        );

    private static Aff<RT, Unit> RunConditionalNext(bool shouldRun, Pipeline<RT> next, ICfgFileStore<RT> store, Seq<PipelineStepConfig> configs) =>
        shouldRun ? next(store, configs) : unitAff;

    private record Config(
        bool Invert,
        Option<string> Command,
        Option<string> WindowsCommand,
        Option<string> UnixCommand
    );
}
