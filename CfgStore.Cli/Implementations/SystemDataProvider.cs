using System.Net.NetworkInformation;
using CfgStore.Application.Abstractions;

namespace CfgStore.Cli.Implementations;

internal class SystemDataProvider<RT> : ISystemDataProvider<RT>
    where RT : struct, HasCancel<RT>
{
    public Aff<RT, string> Domain =>
        Eff(() => IPGlobalProperties.GetIPGlobalProperties().DomainName);

    public Aff<RT, string> Hostname =>
        Eff(() => Environment.MachineName);
}
