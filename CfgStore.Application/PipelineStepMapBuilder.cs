using CfgStore.Application.Abstractions;
using LanguageExt.ClassInstances;
using Map = LanguageExt.Map;

namespace CfgStore.Application;

public class PipelineStepMapBuilder<RT>
    where RT : struct, HasCancel<RT>
{
    public static Map<OrdStringOrdinalIgnoreCase, string, PipelineStepInfo<RT>> Build(Seq<IPipelineStepProvider<RT>> pipelineStepProviders) =>
        Map.createRange<OrdStringOrdinalIgnoreCase, string, PipelineStepInfo<RT>>(
            pipelineStepProviders.Select(x => (x.Name, new PipelineStepInfo<RT>(x.Store, x.Load))));
}
