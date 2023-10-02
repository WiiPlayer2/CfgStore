using CfgStore.Application.Abstractions;
using LanguageExt.ClassInstances;

namespace CfgStore.Modules.Shell;

public class ShellStepProvider<RT> : IPipelineStepProvider<RT>
    where RT : struct, HasCancel<RT>
{
    public PipelineStep<RT> Load => (store, config, configs, next) =>
        from cfg in ParseConfig(config.Value)
        from _99 in FailEff<Unit>("TODO")
        select unit;

    public Seq<string> Names => Seq("shell", "cmd", "command", "commands");

    public PipelineStep<RT> Store => (store, config, configs, next) =>
        from cfg in ParseConfig(config.Value)
        from _99 in FailEff<Unit>("TODO")
        select unit;

    private Eff<Config> ParseConfig(ConfigEntry configEntry) =>
        from passthrough in configEntry.Get("passthrough").Get()
            .IfNone("false")
            .Apply(x => Eff(() => bool.Parse(x)))
        from stagesConfig in ParseStagesConfig(configEntry.Get("stages").GetMap())
        select new Config(
            passthrough,
            stagesConfig);

    private Eff<StageConfig> ParseStageConfig(Option<ConfigEntry> optionConfigEntry, string stage) =>
        from steps in ParseStepsConfig(optionConfigEntry.Map(x => x.Get("steps")), "steps", stage)
        from before in ParseStepsConfig(optionConfigEntry.Map(x => x.Get("before")), "before steps", stage)
        from after in ParseStepsConfig(optionConfigEntry.Map(x => x.Get("after")), "after steps", stage)
        select new StageConfig(
            steps,
            before,
            after);

    private Eff<StagesConfig> ParseStagesConfig(Map<OrdStringOrdinalIgnoreCase, string, ConfigEntry> configEntry) =>
        from allConfig in ParseStageConfig(configEntry.Find("all"), "all")
        from loadConfig in ParseStageConfig(configEntry.Find("load"), "load")
        from storeConfig in ParseStageConfig(configEntry.Find("store"), "store")
        select new StagesConfig(
            allConfig,
            loadConfig,
            storeConfig);

    private Eff<Seq<string>> ParseStepsConfig(Option<ConfigEntry> optionConfigEntry, string steps, string stage) =>
        optionConfigEntry
            .Map(x => x.GetSeq().Select(x => x.Get()).Traverse(identity))
            .IfNone(() => Some(Seq<string>()))
            .ToEff($"Failed to parse {steps} for stage {stage}.");

    private record Config(
        bool Passthrough,
        StagesConfig Stages);

    private record StageConfig(
        Seq<string> Steps,
        Seq<string> Before,
        Seq<string> After);

    private record StagesConfig(
        StageConfig All,
        StageConfig Load,
        StageConfig Store);
}
