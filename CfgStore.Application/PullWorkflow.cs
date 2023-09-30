using CfgStore.Application.Abstractions;

namespace CfgStore.Application;

public class PullWorkflow<RT>
    where RT : struct, HasCancel<RT>
{
    private readonly IGitApi<RT> gitApi;

    private readonly LoadWorkflow<RT> loadWorkflow;

    public PullWorkflow(
        LoadWorkflow<RT> loadWorkflow,
        IGitApi<RT> gitApi)
    {
        this.loadWorkflow = loadWorkflow;
        this.gitApi = gitApi;
    }

    public Aff<RT, Unit> Execute() =>
        from isGitRepo in gitApi.IsGitRepository()
        from _0 in isGitRepo
            ? gitApi.Pull()
            : unitAff
        from _1 in loadWorkflow.Execute()
        select unit;
}
