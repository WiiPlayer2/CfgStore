using CfgStore.Application.Abstractions;
using CliWrap.Buffered;

namespace CfgStore.Cli.Implementations;

internal class GitApi<RT> : IGitApi<RT>
    where RT : struct, HasCancel<RT>
{
    public Aff<RT, Unit> AddAll() =>
        Aff(async (RT rt) => await CliWrap.Cli.Wrap("git")
                .WithArguments("add -A")
                .ExecuteBufferedAsync(rt.CancellationToken))
            .Map(_ => unit);

    public Aff<RT, Unit> CommitAllChanges(string message) =>
        Aff(async (RT rt) => await CliWrap.Cli.Wrap("git")
                .WithArguments(b => b
                    .Add("commit -a -m", false)
                    .Add(message))
                .ExecuteBufferedAsync(rt.CancellationToken))
            .Map(x => unit);

    public Aff<RT, bool> HasChanges() =>
        Aff(async (RT rt) => await CliWrap.Cli.Wrap("git")
                .WithArguments("status --porcelain")
                .ExecuteBufferedAsync(rt.CancellationToken))
            .Map(x => !string.IsNullOrWhiteSpace(x.StandardOutput));

    public Aff<RT, bool> IsGitRepository() =>
        Eff(() => Directory.Exists(".git"));

    public Aff<RT, Unit> Pull() =>
        Aff(async (RT rt) => await CliWrap.Cli.Wrap("git")
                .WithArguments("pull --rebase")
                .ExecuteBufferedAsync(rt.CancellationToken))
            .Map(_ => unit);

    public Aff<RT, Unit> Push() =>
        Aff(async (RT rt) => await CliWrap.Cli.Wrap("git")
                .WithArguments("push")
                .ExecuteBufferedAsync(rt.CancellationToken))
            .Map(_ => unit);
}
