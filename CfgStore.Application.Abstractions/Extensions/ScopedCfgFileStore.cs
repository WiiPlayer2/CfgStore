﻿namespace CfgStore.Application.Abstractions.Extensions;

internal class ScopedCfgFileStore<RT> : ICfgFileStore<RT>
    where RT : struct, HasCancel<RT>
{
    private readonly ICfgFileStore<RT> baseStore;

    private readonly string subPath;

    public ScopedCfgFileStore(ICfgFileStore<RT> baseStore, string subPath)
    {
        this.baseStore = baseStore;
        this.subPath = subPath;
    }

    public Aff<RT, string> ReadText(string path) => baseStore.ReadText(GetPath(path));

    private string GetPath(string path) => Path.Combine(subPath, path);
}
