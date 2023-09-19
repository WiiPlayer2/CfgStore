namespace CfgStore.Domain;

public delegate Aff<RT, Unit> PipelineStep<RT>() where RT : struct, HasCancel<RT>;

public delegate Aff<RT, Unit> Pipeline<RT>() where RT : struct, HasCancel<RT>;
