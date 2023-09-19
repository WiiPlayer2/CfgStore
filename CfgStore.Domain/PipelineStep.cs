namespace CfgStore.Domain;

public delegate Aff<RT, Unit> PipelineStep<RT>(Pipeline<RT> next) where RT : struct, HasCancel<RT>;

public delegate Aff<RT, Unit> Pipeline<RT>() where RT : struct, HasCancel<RT>;
