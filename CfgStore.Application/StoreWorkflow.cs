﻿using CfgStore.Application.Abstractions;
using CfgStore.Application.Abstractions.Extensions;
using LanguageExt.ClassInstances;

namespace CfgStore.Application;

public class StoreWorkflow<RT>
    where RT : struct, HasCancel<RT>
{
    private const string CFG_MANIFEST_FILE_NAME = "cfg.manifest";

    public static Aff<RT, Unit> Execute(
        ICfgFileStore<RT> store,
        Seq<IPipelineStepProvider<RT>> stepProviders,
        IManifestReader<RT> manifestReader) =>
        from _0 in unitAff
        from cfgManifest in manifestReader.Read(store, CFG_MANIFEST_FILE_NAME)
        let stepMap = PipelineStepMapBuilder<RT>.Build(stepProviders)
        from _1 in cfgManifest.Pipelines
            .Pairs
            .Select(x => ExecuteSingle(store.Scope(x.Key), stepMap, x.Value))
            .TraverseParallel(identity)
        select unit;

    public static Aff<RT, Unit> ExecuteSingle(
        ICfgFileStore<RT> cfgFileStore,
        Map<OrdStringOrdinalIgnoreCase, string, PipelineStep<RT>> stepMap,
        PipelineSetup setup) =>
        from _0 in unitAff
        from pipelines in setup.Steps
            .Select(x => stepMap.Find(x.Name).ToEff($"Step {x.Name} does not exist"))
            .Traverse(identity)
        let configs = setup.Steps
            .Select(x => x.Config)
            .ToSeq()
        let pipeline = PipelineBuilder<RT>.Build(pipelines)
        from _1 in pipeline(cfgFileStore, configs)
        select unit;
}
