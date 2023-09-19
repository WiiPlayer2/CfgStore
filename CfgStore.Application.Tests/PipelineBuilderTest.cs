namespace CfgStore.Application.Tests;

[TestClass]
public class PipelineBuilderTest
{
    [TestMethod]
    public async Task Build_WithCapturingStep_ReturnsCapturingPipeline()
    {
        // Arrange
        var steps = Seq1<PipelineStep<RT>>(() => unitAff);

        // Act
        var result = await PipelineBuilder<RT>.Build(steps)
            .Invoke()
            .Run(RT.New());

        // Assert
        result.Case.Should().Be(unit);
    }

    [TestMethod]
    public async Task Build_WithoutSteps_ReturnsFallingThroughPipeline()
    {
        // Arrange
        var steps = Seq<PipelineStep<RT>>();

        // Act
        var result = await PipelineBuilder<RT>.Build(steps)
            .Invoke()
            .Run(RT.New());

        // Assert
        result.Case.Should().Be(Errors.PipelineFallthrough);
    }
}
