using CfgStore.Application.Abstractions;

namespace CfgStore.Application;

public delegate Aff<RT, Unit> Pipeline<RT>(ICfgFileStore cfgFileStore, Seq<PipelineStepConfig> configs) where RT : struct, HasCancel<RT>;
