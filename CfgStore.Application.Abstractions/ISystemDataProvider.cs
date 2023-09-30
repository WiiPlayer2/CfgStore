namespace CfgStore.Application.Abstractions;

public interface ISystemDataProvider<RT>
    where RT : struct, HasCancel<RT>
{
    Aff<RT, string> Domain { get; }

    Aff<RT, string> Hostname { get; }
}
