namespace CfgStore.Application.Abstractions;

public interface ITemplateRenderer<RT>
    where RT : struct, HasCancel<RT>
{
    Aff<RT, string> Render(string template, object data);
}
