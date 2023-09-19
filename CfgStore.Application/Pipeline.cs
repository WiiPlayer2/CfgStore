namespace CfgStore.Application;

public delegate Aff<RT, Unit> Pipeline<RT>(Seq<PipelineStepConfig> configs) where RT : struct, HasCancel<RT>;
