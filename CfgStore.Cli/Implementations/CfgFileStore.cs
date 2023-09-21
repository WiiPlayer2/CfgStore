using CfgStore.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace CfgStore.Cli.Implementations;

internal class CfgFileStore<RT> : ICfgFileStore<RT>
    where RT : struct, HasCancel<RT>
{
    private readonly ILogger<CfgFileStore<RT>> logger;

    public CfgFileStore(
        ILogger<CfgFileStore<RT>> logger)
    {
        this.logger = logger;
    }

    public Aff<RT, Seq<string>> List(string? path = default) =>
        Eff(() => Directory.EnumerateFiles(path ?? string.Empty, "*", SearchOption.AllDirectories)
            .ToSeq());

    public Aff<RT, string> ReadText(string path) => Aff((RT rt) =>
    {
        logger.LogInformation("Reading {path} from store...", path);
        return File.ReadAllTextAsync(path, rt.CancellationToken).ToValue();
    });

    public Aff<RT, Unit> WriteText(string path, string content) =>
        from _0 in EnsureDirectoryExists(Path.GetDirectoryName(path))
        from _1 in Aff((RT rt) =>
        {
            logger.LogInformation("Writing {path} to store...", path);
            return File.WriteAllTextAsync(path, content, rt.CancellationToken).ToUnit().ToValue();
        })
        select unit;

    private Eff<Unit> EnsureDirectoryExists(string? path) =>
        string.IsNullOrEmpty(path)
            ? unitEff
            : from _0 in unitEff
              let parentDirectory = Path.GetDirectoryName(path)
              from _1 in EnsureDirectoryExists(parentDirectory)
              let doesExist = Directory.Exists(path)
              from _2 in doesExist
                  ? unitEff
                  : Eff(() => Directory.CreateDirectory(path)).Map(_ => unit)
              select unit;
}
