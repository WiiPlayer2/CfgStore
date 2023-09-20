using CfgStore.Application.Abstractions;

namespace CfgStore.Application.Tests;

[TestClass]
public class PipelineStepMapBuilderTest
{
    [TestMethod]
    public void Build_WithoutSteps_ReturnsEmptyMap()
    {
        // Arrange
        var stepProviders = Seq<IPipelineStepProvider>();

        // Act
        var result = PipelineStepMapBuilder<RT>.Build(stepProviders);

        // Assert
        ((IEnumerable<(string, PipelineStep<RT>)>) result).Should().BeEmpty();
    }
}
