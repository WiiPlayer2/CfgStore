using CfgStore.Application.Abstractions;

namespace CfgStore.Application;

public class PushWorkflow<RT>
    where RT : struct, HasCancel<RT>
{
    private readonly IGitApi<RT> gitApi;

    private readonly StoreWorkflow<RT> storeWorkflow;

    public PushWorkflow(
        StoreWorkflow<RT> storeWorkflow,
        IGitApi<RT> gitApi)
    {
        this.storeWorkflow = storeWorkflow;
        this.gitApi = gitApi;
    }

    public Aff<RT, Unit> Execute(string commitMessageTemplate) =>
        from _0 in storeWorkflow.Execute(commitMessageTemplate)
        from isGitRepo in gitApi.IsGitRepository()
        from _1 in isGitRepo
            ? gitApi.Push()
            : unitAff
        select unit;
}
