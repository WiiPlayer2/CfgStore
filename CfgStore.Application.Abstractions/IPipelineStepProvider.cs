namespace CfgStore.Application.Abstractions;

public interface IPipelineStepProvider<RT>
    where RT : struct, HasCancel<RT>
{
    PipelineStep<RT> Load { get; }

    Seq<string> Names { get; }

    PipelineStep<RT> Store { get; }
}
