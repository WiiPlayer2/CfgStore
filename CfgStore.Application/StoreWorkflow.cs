using CfgStore.Application.Abstractions;
using CfgStore.Application.Abstractions.Extensions;
using LanguageExt.ClassInstances;

namespace CfgStore.Application;

public class StoreWorkflow<RT>
    where RT : struct, HasCancel<RT>
{
    private readonly IGitApi<RT> gitApi;

    private readonly IManifestReader<RT> manifestReader;

    private readonly Seq<IPipelineStepProvider<RT>> stepProviders;

    private readonly ICfgFileStore<RT> store;

    private readonly ISystemDataProvider<RT> systemDataProvider;

    private readonly ITemplateRenderer<RT> templateRenderer;

    public StoreWorkflow(
        ICfgFileStore<RT> store,
        IEnumerable<IPipelineStepProvider<RT>> stepProviders,
        IManifestReader<RT> manifestReader,
        IGitApi<RT> gitApi,
        ITemplateRenderer<RT> templateRenderer,
        ISystemDataProvider<RT> systemDataProvider)
    {
        this.store = store;
        this.stepProviders = stepProviders.ToSeq();
        this.manifestReader = manifestReader;
        this.gitApi = gitApi;
        this.templateRenderer = templateRenderer;
        this.systemDataProvider = systemDataProvider;
    }

    public Aff<RT, Unit> Execute(
        string commitMessageTemplate) =>
        from cfgManifest in manifestReader.Read(store, Constants.CFG_MANIFEST_FILE_NAME)
        let stepMap = PipelineStepMapBuilder<RT>.Build(stepProviders)
        from _1 in cfgManifest.Pipelines
            .Pairs
            .Select(x => ExecuteSingle(store.Scope(x.Key), stepMap, x.Value))
            .TraverseParallel(identity)
        from _2 in CommitPossibleChanges(commitMessageTemplate)
        select unit;

    public static Aff<RT, Unit> ExecuteSingle(
        ICfgFileStore<RT> cfgFileStore,
        Map<OrdStringOrdinalIgnoreCase, string, PipelineStepInfo<RT>> stepMap,
        PipelineSetup setup) =>
        from _0 in unitAff
        from pipelines in setup.Steps
            .Select(x => stepMap.Find(x.Name).ToEff($"Step {x.Name} does not exist"))
            .Traverse(x => x.Store)
        let configs = setup.Steps
            .Select(x => x.Config)
            .ToSeq()
        let pipeline = PipelineBuilder<RT>.Build(pipelines)
        from _1 in pipeline(cfgFileStore, configs)
        select unit;

    private Aff<RT, Unit> CommitPossibleChanges(string commitMessageTemplate) =>
        from isGitRepo in gitApi.IsGitRepository()
        from hasChanges in isGitRepo
            ? gitApi.HasChanges()
            : SuccessAff(false)
        from _ in hasChanges
            ? from message in RenderCommitMessageTemplate(commitMessageTemplate)
              from _0 in gitApi.AddAll()
              from _1 in gitApi.CommitAllChanges(message)
              select unit
            : unitAff
        select unit;

    private Aff<RT, string> RenderCommitMessageTemplate(string commitMessageTemplate) =>
        from hostname in systemDataProvider.Hostname
        from domain in systemDataProvider.Domain
        let fqdn = $"{hostname}.{domain}"
        let data = new CommitMessageTemplateData(
            hostname,
            domain,
            fqdn)
        from message in templateRenderer.Render(commitMessageTemplate, data)
        select message;
}
