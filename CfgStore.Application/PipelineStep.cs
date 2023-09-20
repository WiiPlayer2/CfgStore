using CfgStore.Application.Abstractions;

namespace CfgStore.Application;

public delegate Aff<RT, Unit> PipelineStep<RT>(ICfgFileStore cfgFileStore, PipelineStepConfig config, Seq<PipelineStepConfig> nextConfigs, Pipeline<RT> next) where RT : struct, HasCancel<RT>;
