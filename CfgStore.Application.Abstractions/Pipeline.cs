namespace CfgStore.Application.Abstractions;

public delegate Aff<RT, Unit> Pipeline<RT>(ICfgFileStore cfgFileStore, Seq<PipelineStepConfig> configs) where RT : struct, HasCancel<RT>;
