namespace CfgStore.Application.Abstractions;

public delegate Aff<RT, Unit> PipelineStep<RT>(ICfgFileStore cfgFileStore, PipelineStepConfig config, Seq<PipelineStepConfig> nextConfigs, Pipeline<RT> next) where RT : struct, HasCancel<RT>;
