namespace CfgStore.Application.Abstractions;

public interface IManifestReader<RT>
    where RT : struct, HasCancel<RT>
{
    Aff<RT, CfgManifest> Read(ICfgFileStore store, string baseFileName);
}
