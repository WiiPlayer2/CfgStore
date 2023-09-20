using CfgStore.Application.Abstractions;

namespace CfgStore.Application.Tests;

[TestClass]
public class PipelineStepMapBuilderTest
{
    [TestMethod]
    public void Build_WithoutSteps_ReturnsEmptyMap()
    {
        // Arrange
        var stepProviders = Seq<IPipelineStepProvider<RT>>();

        // Act
        var result = PipelineStepMapBuilder<RT>.Build(stepProviders);

        // Assert
        ((IEnumerable<(string, PipelineStep<RT>)>) result).Should().BeEmpty();
    }

    [TestMethod]
    public void BuildWithSteps_ReturnsMap()
    {
        // Arrange
        var step1 = Mock.Of<IPipelineStepProvider<RT>>(x => x.Name == "step1");
        var step2 = Mock.Of<IPipelineStepProvider<RT>>(x => x.Name == "step2");
        var stepProviders = Seq(step1, step2);

        // Act
        var result = PipelineStepMapBuilder<RT>.Build(stepProviders);

        // Assert
        ((IEnumerable<(string, PipelineStep<RT>)>) result).Should().BeEquivalentTo(new[]
        {
            ("step1", step1.Step),
            ("step2", step2.Step),
        });
    }
}
