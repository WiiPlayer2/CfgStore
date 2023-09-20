using CfgStore.Application.Abstractions;

namespace CfgStore.Application;

public record PipelineStepInfo<RT>(
    PipelineStep<RT> Store,
    PipelineStep<RT> Load)
    where RT : struct, HasCancel<RT>;
