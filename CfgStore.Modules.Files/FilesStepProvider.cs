using System.IO.Abstractions;
using CfgStore.Application.Abstractions;
using Ganss.IO;

namespace CfgStore.Modules.Files;

public class FilesStepProvider<RT> : IPipelineStepProvider<RT> where RT : struct, HasCancel<RT>
{
    public PipelineStep<RT> Load { get; } = (store, config, _, _) => throw new NotImplementedException();

    public string Name => "files";

    public PipelineStep<RT> Store { get; } = (store, config, _, _) =>
        from cfg in ParseConfig(config)
        let filters = cfg.Filters.Count == 0
            ? Seq1("**/*")
            : cfg.Filters
        let files = filters
            .Select(x => $"{cfg.Directory.FullName}/{x}")
            .SelectMany(x => Glob.Expand(x))
            .DistinctBy(x => x.FullName)
            .OfType<IFileInfo>()
            .ToSeq()
        from _ in files.Select(x =>
                from content in Aff((RT rt) => File.ReadAllTextAsync(x.FullName, rt.CancellationToken).ToValue())
                from relativePath in Eff(() => new Uri(cfg.Directory.FullName, UriKind.Absolute).MakeRelativeUri(new Uri(x.FullName, UriKind.Absolute)).ToString())
                from _ in store.WriteText(relativePath, content)
                select unit
            )
            .TraverseParallel(identity)
        select unit;

    private static Eff<Config> ParseConfig(PipelineStepConfig config) =>
        from directory in ParseDirectory(config)
        from filters in ParseFilters(config)
        select new Config(
            directory,
            filters);

    private static Eff<DirectoryInfo> ParseDirectory(PipelineStepConfig config) =>
        from directoryPath in config.Value.Get("directory").Get().ToEff()
        from directoryInfo in Eff(() => new DirectoryInfo(directoryPath))
        select directoryInfo;

    private static Eff<Seq<string>> ParseFilters(PipelineStepConfig config) =>
        config.Value.Get("filters").GetSeq()
            .Select(x => x.Get())
            .Traverse(identity)
            .ToEff();

    private record Config(
        DirectoryInfo Directory,
        Seq<string> Filters);
}
