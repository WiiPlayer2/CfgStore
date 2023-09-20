namespace CfgStore.Domain;

public record CfgManifest(
    Map<string, PipelineSetup> Pipelines);

public record PipelineSetup(
    Seq<(string Name, PipelineStepConfig Config)> Steps);
