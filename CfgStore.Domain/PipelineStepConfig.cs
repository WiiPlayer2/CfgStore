using LanguageExt.ClassInstances;

namespace CfgStore.Domain;

public record PipelineStepConfig(ConfigEntry Value);

public abstract record ConfigEntry
{
    public virtual Option<string> Get() => None;

    public virtual ConfigEntry Get(int index) => ConfigNone.Instance;

    public virtual ConfigEntry Get(string key) => ConfigNone.Instance;

    public virtual Map<OrdStringOrdinalIgnoreCase, string, ConfigEntry> GetMap() => default;

    public virtual Seq<ConfigEntry> GetSeq() => default;

    public T Match<T>(
        Func<ConfigValue, T> onValue,
        Func<ConfigMap, T> onMap,
        Func<ConfigList, T> onList,
        Func<T>? onNone = default) =>
        this switch
        {
            ConfigValue v => onValue(v),
            ConfigMap v => onMap(v),
            ConfigList v => onList(v),
            ConfigNone => (onNone ?? (() => throw new InvalidOperationException()))(),
            _ => throw new InvalidOperationException(),
        };
}

internal record ConfigNone : ConfigEntry
{
    public static ConfigNone Instance { get; } = new();
}

public record ConfigValue(string Value) : ConfigEntry
{
    public override Option<string> Get() => Value;
}

public record ConfigMap(Map<OrdStringOrdinalIgnoreCase, string, ConfigEntry> Entries) : ConfigEntry
{
    public override ConfigEntry Get(string key) => Entries.Find(key, identity, () => ConfigNone.Instance);

    public override Map<OrdStringOrdinalIgnoreCase, string, ConfigEntry> GetMap() => Entries;
}

public record ConfigList(Seq<ConfigEntry> Entries) : ConfigEntry
{
    public override ConfigEntry Get(int index) => Entries.At(index).IfNone(ConfigNone.Instance);

    public override Seq<ConfigEntry> GetSeq() => Entries;
}
