namespace CfgStore.Application.Abstractions;

public interface ICfgFileStore<RT>
    where RT : struct, HasCancel<RT>
{
    Aff<RT, string> ReadText(string path);
}
