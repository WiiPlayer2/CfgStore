using CfgStore.Application.Abstractions;

namespace CfgStore.Cli.Implementations;

internal class CfgFileStore<RT> : ICfgFileStore<RT>
    where RT : struct, HasCancel<RT> { }
