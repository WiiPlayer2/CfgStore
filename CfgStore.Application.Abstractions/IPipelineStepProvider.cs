namespace CfgStore.Application.Abstractions;

public interface IPipelineStepProvider<RT>
    where RT : struct, HasCancel<RT>
{
    string Name { get; }

    PipelineStep<RT> Step { get; }
}
