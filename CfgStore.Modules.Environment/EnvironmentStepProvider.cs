using CfgStore.Application.Abstractions;
using Map = LanguageExt.Map;

namespace CfgStore.Modules.Environment;

public class EnvironmentStepProvider<RT> : IPipelineStepProvider<RT>
    where RT : struct, HasCancel<RT>
{
    public PipelineStep<RT> Load { get; } = (store, config, configs, next) =>
        from _0 in unitEff
        from cfg in ParseConfig(config)
        from values in GetValues(cfg.Variables)
        from _99 in next(store, configs)
        select unit;

    public string Name => "env";

    public PipelineStep<RT> Store => Load;

    private static Eff<Map<string, string>> GetValues(Seq<string> variables) =>
        variables
            .Select(x => Optional(System.Environment.GetEnvironmentVariable(x))
                .ToEff($"Environment variable ${{{x}}} is not set.")
                .Map(y => (x, y)))
            .Traverse(identity)
            .Map(x => Map.createRange(x));

    private static Eff<Config> ParseConfig(PipelineStepConfig config) =>
        from variables in config.Value.Get("variables").GetSeq()
            .Select(x => x.Get())
            .Traverse(identity)
            .ToEff("Failed to parse variables from config.")
        select new Config(
            variables);

    private record Config(
        Seq<string> Variables);
}
