namespace CfgStore.Application.Abstractions;

public interface IGitApi<RT>
    where RT : struct, HasCancel<RT>
{
    Aff<RT, Unit> AddAll();

    Aff<RT, Unit> CommitAllChanges(string message);

    Aff<RT, bool> HasChanges();

    Aff<RT, bool> IsGitRepository();

    Aff<RT, Unit> Pull();

    Aff<RT, Unit> Push();
}
