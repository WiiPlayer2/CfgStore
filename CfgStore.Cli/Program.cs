using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using CfgStore.Application;
using CfgStore.Application.Abstractions;
using CfgStore.Cli.Implementations;
using CfgStore.Modules.Files;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RT = LanguageExt.Sys.Live.Runtime;

await BuildCommandLine()
    .UseDefaults()
    .UseHost(builder => builder
        .ConfigureDefaults(args)
        .ConfigureServices((_, services) =>
        {
            services.AddSingleton<ICfgFileStore<RT>, CfgFileStore<RT>>();
            services.AddSingleton<IManifestReader<RT>, ManifestReader<RT>>();

            services.AddSingleton<IPipelineStepProvider<RT>, FilesStepProvider<RT>>();
        }))
    .Build()
    .InvokeAsync(args);

CommandLineBuilder BuildCommandLine()
{
    var storeCommand = new Command("store", "Stores configuration defined in the manifest in the local folder.");
    storeCommand.Handler = CommandHandler.Create(InvokeStoreWorkflow);

    var loadCommand = new Command("load", "Loads configuration defined in the manifest in the local folder.");
    loadCommand.Handler = CommandHandler.Create(InvokeLoadWorkflow);

    var rootCommand = new RootCommand("Tool to store and load different types of configuration into a folder defined by pipelines inside a manifest.")
    {
        storeCommand,
        loadCommand,
    };

    return new CommandLineBuilder(rootCommand);
}

(
    ICfgFileStore<RT> Store,
    IManifestReader<RT> ManifestReader,
    Seq<IPipelineStepProvider<RT>> StepProviders
    ) Resolve(IHost host) =>
(
    host.Services.GetRequiredService<ICfgFileStore<RT>>(),
    host.Services.GetRequiredService<IManifestReader<RT>>(),
    host.Services.GetRequiredService<IEnumerable<IPipelineStepProvider<RT>>>().ToSeq()
);

async Task InvokeStoreWorkflow(IHost host, CancellationToken cancellationToken)
{
    var (store, manifestReader, stepProviders) = Resolve(host);
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    var runtime = RT.New(cts);
    await StoreWorkflow<RT>.Execute(store, stepProviders, manifestReader).RunUnit(runtime);
}

async Task InvokeLoadWorkflow(IHost host, CancellationToken cancellationToken)
{
    var (store, manifestReader, stepProviders) = Resolve(host);
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    var runtime = RT.New(cts);
    await LoadWorkflow<RT>.Execute(store, stepProviders, manifestReader).RunUnit(runtime);
}
