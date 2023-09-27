using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using CfgStore.Application;
using CfgStore.Application.Abstractions;
using CfgStore.Cli.Implementations;
using CfgStore.Modules.Environment;
using CfgStore.Modules.Files;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            services.AddSingleton<IPipelineStepProvider<RT>, EnvironmentStepProvider<RT>>();
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
        new System.CommandLine.Option<DirectoryInfo?>(
            new[] {"--directory", "-C",},
            () => default,
            "Change to this directory before performing any action."),
        storeCommand,
        loadCommand,
    };

    return new CommandLineBuilder(rootCommand);
}

(
    ICfgFileStore<RT> Store,
    IManifestReader<RT> ManifestReader,
    Seq<IPipelineStepProvider<RT>> StepProviders,
    ILogger<Program> Logger
    ) Resolve(IHost host) =>
(
    host.Services.GetRequiredService<ICfgFileStore<RT>>(),
    host.Services.GetRequiredService<IManifestReader<RT>>(),
    host.Services.GetRequiredService<IEnumerable<IPipelineStepProvider<RT>>>().ToSeq(),
    host.Services.GetRequiredService<ILogger<Program>>()
);

async Task InvokeWorkflow(Func<ICfgFileStore<RT>, Seq<IPipelineStepProvider<RT>>, IManifestReader<RT>, Aff<RT, Unit>> invocation, DirectoryInfo? directory, IHost host, CancellationToken cancellationToken)
{
    var (store, manifestReader, stepProviders, logger) = Resolve(host);
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    var runtime = RT.New(cts);

    var result = await (
        from _0 in Eff(fun(() =>
        {
            if (directory is not null)
            {
                logger.LogInformation("Changing directory to {directory}", directory);
                Environment.CurrentDirectory = directory.FullName;
            }

            logger.LogInformation("Using directory {directory}", Environment.CurrentDirectory);
        }))
        from _1 in invocation(store, stepProviders, manifestReader)
        select unit
    ).Run(runtime);

    result.IfFail(LogError);

    void LogError(Error error)
    {
        switch (error)
        {
            case Exceptional exceptional:
                logger.LogError(exceptional.ToException(), exceptional.Message);
                break;

            case ManyErrors manyErrors:
                manyErrors.Errors.Iter(LogError);
                break;

            default:
                logger.LogError(error.Exception.IfNoneUnsafe(default(Exception?)), error.Message);
                error.Inner.Do(LogError);
                break;
        }
    }
}

Task InvokeStoreWorkflow(DirectoryInfo? directory, IHost host, CancellationToken cancellationToken) =>
    InvokeWorkflow(StoreWorkflow<RT>.Execute, directory, host, cancellationToken);

Task InvokeLoadWorkflow(DirectoryInfo? directory, IHost host, CancellationToken cancellationToken) =>
    InvokeWorkflow(LoadWorkflow<RT>.Execute, directory, host, cancellationToken);
