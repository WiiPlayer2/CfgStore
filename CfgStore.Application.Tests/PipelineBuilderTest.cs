namespace CfgStore.Application.Tests;

[TestClass]
public class PipelineBuilderTest
{
    [TestMethod]
    public async Task Build_WithCapturingStep_ReturnsCapturingPipeline()
    {
        // Arrange
        var steps = Seq1<PipelineStep<RT>>(_ => unitAff);

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

    [TestMethod]
    public async Task Build_WithSteps_ExecutesStepsInPipeline()
    {
        // Arrange
        var step1 = new Mock<PipelineStep<RT>>();
        var step2 = new Mock<PipelineStep<RT>>();
        var steps = Seq(step1.Object, step2.Object);
        step1.Setup(s => s(It.IsAny<Pipeline<RT>>()))
            .Returns((Pipeline<RT> next) => next());
        step2.Setup(s => s(It.IsAny<Pipeline<RT>>()))
            .Returns(unitAff);

        // Act
        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
        await PipelineBuilder<RT>.Build(steps)
            .Invoke()
            .Run(RT.New());

        // Assert
        step1.Verify(s => s(It.IsAny<Pipeline<RT>>()));
        step2.Verify(s => s(It.IsAny<Pipeline<RT>>()));
    }
}
