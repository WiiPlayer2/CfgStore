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

const string cfgStoreGlobalDirectoryVariableName = "CFG_STORE_GLOBAL_DIRECTORY";

var globalDirectory = Optional(Environment.GetEnvironmentVariable(cfgStoreGlobalDirectoryVariableName))
    .IfNone(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cfg-store"));

await BuildCommandLine()
    .UseDefaults()
    .UseHost(builder => builder
        .ConfigureDefaults(args)
        .ConfigureServices((_, services) =>
        {
            services.AddSingleton<ICfgFileStore<RT>, CfgFileStore<RT>>();
            services.AddSingleton<IManifestReader<RT>, ManifestReader<RT>>();
            services.AddSingleton<IGitApi<RT>, GitApi<RT>>();
            services.AddSingleton<ITemplateRenderer<RT>, TemplateRenderer<RT>>();
            services.AddSingleton<ISystemDataProvider<RT>, SystemDataProvider<RT>>();

            services.AddSingleton<IPipelineStepProvider<RT>, FilesStepProvider<RT>>();
            services.AddSingleton<IPipelineStepProvider<RT>, EnvironmentStepProvider<RT>>();

            services.AddSingleton<StoreWorkflow<RT>>();
            services.AddSingleton<LoadWorkflow<RT>>();
            services.AddSingleton<PushWorkflow<RT>>();
            services.AddSingleton<PullWorkflow<RT>>();
        }))
    .Build()
    .InvokeAsync(args);

CommandLineBuilder BuildCommandLine()
{
    var messageTemplateOption = new System.CommandLine.Option<string>(
        new[] {"--message-template", "-m"},
        () => "Update configuration from {{ Hostname }}",
        "The commit message template used for commiting after storing if directory is also a git repository.");
    var storeCommand = new Command("store", "Stores configuration defined in the manifest in the local folder.")
    {
        messageTemplateOption,
    };
    storeCommand.Handler = CommandHandler.Create(InvokeStoreWorkflow);

    var loadCommand = new Command("load", "Loads configuration defined in the manifest in the local folder.");
    loadCommand.Handler = CommandHandler.Create(InvokeLoadWorkflow);

    var pushCommand = new Command("push", "Stores and pushes configuration defined in the manifest local folder.")
    {
        messageTemplateOption,
    };
    pushCommand.Handler = CommandHandler.Create(InvokePushWorkflow);

    var pullCommand = new Command("pull", "Pulls and loads configurations defined in the manifest in the local folder.");
    pullCommand.Handler = CommandHandler.Create(InvokePullWorkflow);

    var rootCommand = new RootCommand("Tool to store and load different types of configuration into a folder defined by pipelines inside a manifest.")
    {
        new System.CommandLine.Option<DirectoryInfo?>(
            new[] {"--directory", "-C"},
            () => default,
            "Change to this directory before performing any action."),
        new System.CommandLine.Option<bool>(
            new[] {"--global", "-g"},
            () => false,
            $"Use the global directory ({globalDirectory}) for performing actions.\nCan be changed using the environment variable {cfgStoreGlobalDirectoryVariableName}.\nOverrides --directory."),
        storeCommand,
        loadCommand,
        pushCommand,
        pullCommand,
    };

    return new CommandLineBuilder(rootCommand);
}

(
    ILogger<Program> Logger,
    TWorkflow Workflow
    ) Resolve<TWorkflow>(IHost host)
    where TWorkflow : notnull =>
(
    host.Services.GetRequiredService<ILogger<Program>>(),
    host.Services.GetRequiredService<TWorkflow>()
);

async Task InvokeWorkflow<TWorkflow>(Func<TWorkflow, Aff<RT, Unit>> invocation, GlobalArgs args, IHost host, CancellationToken cancellationToken)
{
    var (logger, workflow) = Resolve<TWorkflow>(host);
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    var runtime = RT.New(cts);

    var result = await (
        from _0 in Eff(fun(() =>
        {
            if (args.Global)
            {
                logger.LogInformation("Changing to global directory");
                if (!Directory.Exists(globalDirectory))
                {
                    Directory.CreateDirectory(globalDirectory);
                }

                Environment.CurrentDirectory = globalDirectory;
            }
            else if (args.Directory is not null)
            {
                logger.LogInformation("Changing directory to {directory}", args.Directory);
                Environment.CurrentDirectory = args.Directory.FullName;
            }

            logger.LogInformation("Using directory {directory}", Environment.CurrentDirectory);
        }))
        from _1 in invocation(workflow)
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

Task InvokeStoreWorkflow(GlobalArgs args, StoreArgs storeArgs, IHost host, CancellationToken cancellationToken) =>
    InvokeWorkflow<StoreWorkflow<RT>>(w => w.Execute(storeArgs.MessageTemplate), args, host, cancellationToken);

Task InvokeLoadWorkflow(GlobalArgs args, IHost host, CancellationToken cancellationToken) =>
    InvokeWorkflow<LoadWorkflow<RT>>(w => w.Execute(), args, host, cancellationToken);

Task InvokePushWorkflow(GlobalArgs args, PushArgs pushArgs, IHost host, CancellationToken cancellationToken) =>
    InvokeWorkflow<PushWorkflow<RT>>(w => w.Execute(pushArgs.MessageTemplate), args, host, cancellationToken);

Task InvokePullWorkflow(GlobalArgs args, IHost host, CancellationToken cancellationToken) =>
    InvokeWorkflow<PullWorkflow<RT>>(w => w.Execute(), args, host, cancellationToken);

internal record GlobalArgs(
    DirectoryInfo? Directory,
    bool Global);

internal record StoreArgs(
    string MessageTemplate);

internal record PushArgs(
    string MessageTemplate);
