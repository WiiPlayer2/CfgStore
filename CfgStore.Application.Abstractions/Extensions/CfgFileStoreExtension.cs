namespace CfgStore.Application.Abstractions.Extensions;

public static class CfgFileStoreExtension
{
    public static ICfgFileStore<RT> Scope<RT>(this ICfgFileStore<RT> store, string subPath)
        where RT : struct, HasCancel<RT>
        => throw new NotImplementedException();
}
