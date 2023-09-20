using CfgStore.Application.Abstractions;
using LanguageExt.ClassInstances;
using Map = LanguageExt.Map;

namespace CfgStore.Application;

public class PipelineStepMapBuilder<RT>
    where RT : struct, HasCancel<RT>
{
    public static Map<OrdStringOrdinalIgnoreCase, string, PipelineStep<RT>> Build(Seq<IPipelineStepProvider<RT>> pipelineStepProviders) =>
        Map.createRange<OrdStringOrdinalIgnoreCase, string, PipelineStep<RT>>(pipelineStepProviders.Select(x => (x.Name, x.Step)));
}
