﻿using CfgStore.Application.Abstractions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Map = LanguageExt.Map;

namespace CfgStore.Cli.Implementations;

internal class ManifestReader<RT> : IManifestReader<RT>
    where RT : struct, HasCancel<RT>
{
    private const string FILE_EXTENSION = ".yaml";

    public Aff<RT, CfgManifest> Read(ICfgFileStore<RT> store, string baseFileName) =>
        from _0 in unitAff
        let fileName = $"{baseFileName}{FILE_EXTENSION}"
        from manifestFileContent in store.ReadText(fileName)
        from manifestDto in Eff(() => BuildDeserializer().Deserialize<ManifestDto>(manifestFileContent))
        from manifest in MapManifest(manifestDto)
        select manifest;

    private IDeserializer BuildDeserializer() => new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    private Eff<CfgManifest> MapManifest(ManifestDto dto) =>
        from mappedPipelines in dto.Pipelines
            .Select(x => MapPipelineSetup(x.Value).Map(y => (x.Key, y)))
            .Traverse(identity)
            .Map(Map.createRange)
        select new CfgManifest(mappedPipelines);

    private Eff<PipelineSetup> MapPipelineSetup(List<Dictionary<string, object>> dto) =>
        from steps in dto
            .Select(MapPipelineStep)
            .Traverse(identity)
            .Map(x => x.ToSeq())
        select new PipelineSetup(steps);

    private Eff<(string Name, PipelineStepConfig Config)> MapPipelineStep(Dictionary<string, object> dto) =>
        from _0 in unitEff
        from _1 in guard(dto.Count == 1, Error.New("Expected a single pipeline step configuration."))
        let name = dto.Keys.Single()
        let dtoValues = dto[name]
        select (name, new PipelineStepConfig());

    private class ManifestDto
    {
        public Dictionary<string, List<Dictionary<string, object>>> Pipelines { get; } = default!;
    }
}
