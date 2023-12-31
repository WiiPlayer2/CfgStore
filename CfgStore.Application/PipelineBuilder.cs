using CfgStore.Application.Abstractions;

namespace CfgStore.Application;

internal class PipelineBuilder<RT>
    where RT : struct, HasCancel<RT>
{
    private static readonly Pipeline<RT> FALLTHROUGH_PIPELINE = (_, _) => FailAff<Unit>(Errors.PipelineFallthrough);

    public static Pipeline<RT> Build(Seq<PipelineStep<RT>> steps) =>
        steps.HeadOrNone()
            .Match(
                step => (cfgFileStore, configs) =>
                    from config in configs.HeadOrNone().ToEff()
                    from _ in step(cfgFileStore, config, configs.Tail, Build(steps.Tail))
                    select unit,
                () => FALLTHROUGH_PIPELINE
            );
}
