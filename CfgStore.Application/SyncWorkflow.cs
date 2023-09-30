using CfgStore.Application.Abstractions;

namespace CfgStore.Application;

public class SyncWorkflow<RT>
    where RT : struct, HasCancel<RT>
{
    private readonly IGitApi<RT> gitApi;

    private readonly LoadWorkflow<RT> loadWorkflow;

    private readonly StoreWorkflow<RT> storeWorkflow;

    public SyncWorkflow(
        StoreWorkflow<RT> storeWorkflow,
        LoadWorkflow<RT> loadWorkflow,
        IGitApi<RT> gitApi)
    {
        this.storeWorkflow = storeWorkflow;
        this.loadWorkflow = loadWorkflow;
        this.gitApi = gitApi;
    }

    public Aff<RT, Unit> Execute(string commitMessageTemplate) =>
        from _0 in storeWorkflow.Execute(commitMessageTemplate)
        from _1 in gitApi.Pull()
        from _3 in loadWorkflow.Execute()
        from _2 in gitApi.Push()
        select unit;
}
