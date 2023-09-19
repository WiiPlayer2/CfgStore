namespace CfgStore.Domain;

public delegate Aff<RT, Unit> PipelineStep<RT>(PipelineStepConfig config, Seq<PipelineStepConfig> nextConfigs, Pipeline<RT> next) where RT : struct, HasCancel<RT>;

public delegate Aff<RT, Unit> Pipeline<RT>(Seq<PipelineStepConfig> configs) where RT : struct, HasCancel<RT>;

public record PipelineStepConfig;
