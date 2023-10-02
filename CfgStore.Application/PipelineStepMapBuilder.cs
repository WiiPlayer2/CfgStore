using CfgStore.Application.Abstractions;
using LanguageExt.ClassInstances;
using Map = LanguageExt.Map;

namespace CfgStore.Application;

public class PipelineStepMapBuilder<RT>
    where RT : struct, HasCancel<RT>
{
    public static Map<OrdStringOrdinalIgnoreCase, string, PipelineStepInfo<RT>> Build(Seq<IPipelineStepProvider<RT>> pipelineStepProviders) =>
        Map.createRange<OrdStringOrdinalIgnoreCase, string, PipelineStepInfo<RT>>(
            pipelineStepProviders.SelectMany(x => x.Names, (y, x) => (x, new PipelineStepInfo<RT>(y.Store, y.Load))));
}
