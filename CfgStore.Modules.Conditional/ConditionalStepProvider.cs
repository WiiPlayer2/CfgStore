using CfgStore.Application.Abstractions;

namespace CfgStore.Modules.Conditional;

public class ConditionalStepProvider<RT> : IPipelineStepProvider<RT>
    where RT : struct, HasCancel<RT>
{
    public PipelineStep<RT> Load { get; }

    public Seq<string> Names => Seq("conditional", "cond", "if");

    public PipelineStep<RT> Store { get; }
}
