namespace CfgStore.Application.Abstractions;

public interface IPipelineStepProvider<RT>
    where RT : struct, HasCancel<RT>
{
    PipelineStep<RT> Load { get; }

    string Name { get; }

    PipelineStep<RT> Store { get; }
}
