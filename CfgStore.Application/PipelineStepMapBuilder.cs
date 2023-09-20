using CfgStore.Application.Abstractions;
using LanguageExt.ClassInstances;
using Map = LanguageExt.Map;

namespace CfgStore.Application;

public class PipelineStepMapBuilder<RT>
    where RT : struct, HasCancel<RT>
{
    public static Map<OrdStringOrdinalIgnoreCase, string, PipelineStep<RT>> Build(Seq<IPipelineStepProvider> pipelineStepProviders) =>
        Map.empty<OrdStringOrdinalIgnoreCase, string, PipelineStep<RT>>();
}
