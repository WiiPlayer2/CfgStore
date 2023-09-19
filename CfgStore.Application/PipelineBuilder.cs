namespace CfgStore.Application;

internal class PipelineBuilder<RT>
    where RT : struct, HasCancel<RT>
{
    private static readonly Pipeline<RT> FALLTHROUGH_PIPELINE = () => FailAff<Unit>(Errors.PipelineFallthrough);

    public static Pipeline<RT> Build(Seq<PipelineStep<RT>> steps) =>
        steps.HeadOrNone()
            .Match(
                step => () => step(Build(steps.Tail)),
                () => FALLTHROUGH_PIPELINE
            );
}
