using CfgStore.Application.Abstractions;
using LanguageExt.ClassInstances;
using Map = LanguageExt.Map;

namespace CfgStore.Modules.Environment;

public class EnvironmentStepProvider<RT> : IPipelineStepProvider<RT>
    where RT : struct, HasCancel<RT>
{
    public PipelineStep<RT> Load { get; } = (store, config, configs, next) =>
        from _0 in unitEff
        from cfg in ParseConfig(config)
        from values in GetValues(cfg.Variables)
        from mappedConfigs in configs
            .Select(x => MapConfigEntry(x.Value, values))
            .Traverse(x => new PipelineStepConfig(x))
        from _99 in next(store, mappedConfigs)
        select unit;

    public string Name => "env";

    public PipelineStep<RT> Store => Load;

    private static Eff<Map<string, string>> GetValues(Seq<string> variables) =>
        variables
            .Select(x => Optional(System.Environment.GetEnvironmentVariable(x))
                .ToEff($"Environment variable ${{{x}}} is not set.")
                .Map(y => (x, y)))
            .Traverse(identity)
            .Map(x => Map.createRange(x));

    private static Eff<ConfigEntry> MapConfigEntry(ConfigEntry entry, Map<string, string> values) =>
        entry.Match(
            v => MapConfigValue(v, values).Map(x => (ConfigEntry) x),
            v => MapConfigMap(v, values).Map(x => (ConfigEntry) x),
            v => MapConfigList(v, values).Map(x => (ConfigEntry) x));

    private static Eff<ConfigList> MapConfigList(ConfigList list, Map<string, string> values) =>
        from mappedEntries in list.Entries
            .Select(x => MapConfigEntry(x, values))
            .Traverse(identity)
        select new ConfigList(mappedEntries);

    private static Eff<ConfigMap> MapConfigMap(ConfigMap map, Map<string, string> values) =>
        from mappedEntries in map.Entries.ValueTuples
            .Select(kv => MapConfigEntry(kv.Value, values).Map(x => (kv.Key, x)))
            .Traverse(identity)
        select new ConfigMap(Map.createRange<OrdStringOrdinalIgnoreCase, string, ConfigEntry>(mappedEntries));

    private static Eff<ConfigValue> MapConfigValue(ConfigValue value, Map<string, string> values) =>
        from mappedValue in EffMaybe(() => EnvironmentVariableReplacer.Replace(value.Value, values))
        select new ConfigValue(mappedValue);

    private static Eff<Config> ParseConfig(PipelineStepConfig config) =>
        from variables in config.Value.Get("variables").GetSeq()
            .Select(x => x.Get())
            .Traverse(identity)
            .ToEff("Failed to parse variables from config.")
        select new Config(
            variables);

    private record Config(
        Seq<string> Variables);
}
