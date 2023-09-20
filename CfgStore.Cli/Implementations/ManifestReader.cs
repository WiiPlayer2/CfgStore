using CfgStore.Application.Abstractions;

namespace CfgStore.Cli.Implementations;

internal class ManifestReader<RT> : IManifestReader<RT>
    where RT : struct, HasCancel<RT>
{
    private const string FILE_EXTENSION = ".yaml";

    public Aff<RT, CfgManifest> Read(ICfgFileStore<RT> store, string baseFileName) =>
        from _0 in unitAff
        let fileName = $"{baseFileName}{FILE_EXTENSION}"
        from _99 in FailAff<Unit>("TODO")
        select default(CfgManifest)!;
}
