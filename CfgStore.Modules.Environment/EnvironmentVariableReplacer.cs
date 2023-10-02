using System.Text.RegularExpressions;

namespace CfgStore.Modules.Environment;

public static class EnvironmentVariableReplacer
{
    private static readonly Regex variableRegex = new(@"^\${(?<variable>(\w|_)+)}$");

    public static string Replace(string template, Map<string, string> values) =>
        variableRegex.Match(template)
            .Apply(x => x.Success
                ? values[x.Groups["variable"].Value]
                : template);
}
