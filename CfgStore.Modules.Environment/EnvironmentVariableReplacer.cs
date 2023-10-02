using System.Text.RegularExpressions;

namespace CfgStore.Modules.Environment;

public static class EnvironmentVariableReplacer
{
    private static readonly Regex variableRegex = new(@"\${(?<variable>(\w|_)+)}");

    public static Fin<string> Replace(string template, Map<string, string> values)
    {
        Match match;
        var currentIndex = 0;
        var currentTemplate = template;

        while ((match = variableRegex.Match(currentTemplate, currentIndex)).Success)
        {
            var variable = match.Groups["variable"].Value;
            if (!values.ContainsKey(variable))
            {
                return FinFail<string>($"{variable} not found in values");
            }

            var value = values[variable];

            currentTemplate = $"{currentTemplate[..match.Index]}{value}{currentTemplate[(match.Index + match.Length)..]}";
            currentIndex = match.Index + value.Length;
        }

        return currentTemplate;
    }
}
