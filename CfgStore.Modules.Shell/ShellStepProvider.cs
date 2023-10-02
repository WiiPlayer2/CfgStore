using CfgStore.Application.Abstractions;
using LanguageExt.ClassInstances;

namespace CfgStore.Modules.Shell;

public class ShellStepProvider<RT> : IPipelineStepProvider<RT>
    where RT : struct, HasCancel<RT>
{
    public PipelineStep<RT> Load => Step(x => x.Load);

    public Seq<string> Names => Seq("shell", "cmd", "command", "commands");

    public PipelineStep<RT> Store => Step(x => x.Store);

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

    private Aff<RT, Unit> RunStage(StagesConfig stagesConfig, Func<StagesConfig, StageConfig> getStageConfig, Func<StageConfig, Seq<string>> getSteps) =>
        from steps in Eff(() =>
            stagesConfig.All.Steps
                .Concat(getSteps(stagesConfig.All))
                .Concat(getStageConfig(stagesConfig).Steps)
                .Concat(getSteps(getStageConfig(stagesConfig))))
        from _ in RunSteps(steps)
        select unit;

    private Aff<RT, Unit> RunStep(string step) => throw new NotImplementedException();

    private Aff<RT, Unit> RunSteps(Seq<string> steps) =>
        steps
            .Select(RunStep)
            .TraverseSerial(identity)
            .Map(_ => unit);

    private PipelineStep<RT> Step(Func<StagesConfig, StageConfig> getStageConfig) =>
        (store, config, configs, next) =>
            from cfg in ParseConfig(config.Value)
            from _0 in RunStage(cfg.Stages, getStageConfig, x => x.Before)
            from _1 in cfg.Passthrough
                ? from _0 in next(store, configs)
                  from _1 in RunStage(cfg.Stages, getStageConfig, x => x.After)
                  select unit
                : unitAff
            select unit;

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
