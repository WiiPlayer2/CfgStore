using CfgStore.Application.Abstractions;

namespace CfgStore.Cli.Implementations;

internal class CfgFileStore<RT> : ICfgFileStore<RT>
    where RT : struct, HasCancel<RT>
{
    public Aff<RT, string> ReadText(string path) => Aff((RT rt) => File.ReadAllTextAsync(path, rt.CancellationToken).ToValue());
}
