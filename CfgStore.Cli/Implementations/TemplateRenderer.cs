using CfgStore.Application.Abstractions;
using Stubble.Core.Builders;

namespace CfgStore.Cli.Implementations;

internal class TemplateRenderer<RT> : ITemplateRenderer<RT>
    where RT : struct, HasCancel<RT>
{
    public Aff<RT, string> Render(string template, object data) =>
        Aff(() => new StubbleBuilder()
            .Build()
            .RenderAsync(template, data));
}
