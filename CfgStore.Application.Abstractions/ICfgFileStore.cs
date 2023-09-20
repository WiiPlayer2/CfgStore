namespace CfgStore.Application.Abstractions;

public interface ICfgFileStore<RT>
    where RT : struct, HasCancel<RT> { }
