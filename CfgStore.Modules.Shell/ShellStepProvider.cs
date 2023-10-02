using CfgStore.Application.Abstractions;

namespace CfgStore.Modules.Shell;

public class ShellStepProvider<RT> : IPipelineStepProvider<RT>
    where RT : struct, HasCancel<RT>
{
    public PipelineStep<RT> Load => throw new NotImplementedException();

    public Seq<string> Names => Seq("shell", "cmd", "command", "commands");

    public PipelineStep<RT> Store => throw new NotImplementedException();
}
