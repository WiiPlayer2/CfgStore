namespace CfgStore.Application;

public static class Errors
{
    public static readonly Error PipelineFallthrough = Error.New("Pipeline should not fall completely through.");
}
