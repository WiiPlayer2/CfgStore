namespace CfgStore.Application.Tests;

[TestClass]
public class PipelineBuilderTest
{
    [TestMethod]
    public async Task Build_WithCapturingStep_ReturnsCapturingPipeline()
    {
        // Arrange
        var steps = Seq1<PipelineStep<RT>>((_, _, _) => unitAff);

        // Act
        var result = await PipelineBuilder<RT>.Build(steps)
            .Invoke(Seq1(new PipelineStepConfig()))
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
            .Invoke(default)
            .Run(RT.New());

        // Assert
        result.Case.Should().Be(Errors.PipelineFallthrough);
    }

    [TestMethod]
    public async Task Build_WithStepAndConfig_PassesConfigToStep()
    {
        // Arrange
        var config1 = new PipelineStepConfig();
        var config2 = new PipelineStepConfig();
        var configs = Seq(config1, config2);
        var step1 = new Mock<PipelineStep<RT>>();
        var step2 = new Mock<PipelineStep<RT>>();
        var steps = Seq(step1.Object, step2.Object);
        step1.Setup(s => s(config1, It.IsAny<Seq<PipelineStepConfig>>(), It.IsAny<Pipeline<RT>>()))
            .Returns((PipelineStepConfig _, Seq<PipelineStepConfig> nextConfigs, Pipeline<RT> next) => next(nextConfigs));
        step2.Setup(s => s(config2, It.IsAny<Seq<PipelineStepConfig>>(), It.IsAny<Pipeline<RT>>()))
            .Returns(unitAff);

        // Act
        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
        await PipelineBuilder<RT>.Build(steps)
            .Invoke(configs)
            .Run(RT.New());

        // Assert
        step1.Verify(s => s(config1, It.IsAny<Seq<PipelineStepConfig>>(), It.IsAny<Pipeline<RT>>()));
        step2.Verify(s => s(config2, It.IsAny<Seq<PipelineStepConfig>>(), It.IsAny<Pipeline<RT>>()));
    }

    [TestMethod]
    public async Task Build_WithSteps_ExecutesStepsInPipeline()
    {
        // Arrange
        var step1 = new Mock<PipelineStep<RT>>();
        var step2 = new Mock<PipelineStep<RT>>();
        var steps = Seq(step1.Object, step2.Object);
        step1.Setup(s => s(It.IsAny<PipelineStepConfig>(), It.IsAny<Seq<PipelineStepConfig>>(), It.IsAny<Pipeline<RT>>()))
            .Returns((PipelineStepConfig _, Seq<PipelineStepConfig> nextConfigs, Pipeline<RT> next) => next(nextConfigs));
        step2.Setup(s => s(It.IsAny<PipelineStepConfig>(), It.IsAny<Seq<PipelineStepConfig>>(), It.IsAny<Pipeline<RT>>()))
            .Returns(unitAff);

        // Act
        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
        await PipelineBuilder<RT>.Build(steps)
            .Invoke(Seq(new PipelineStepConfig(), new PipelineStepConfig()))
            .Run(RT.New());

        // Assert
        step1.Verify(s => s(It.IsAny<PipelineStepConfig>(), It.IsAny<Seq<PipelineStepConfig>>(), It.IsAny<Pipeline<RT>>()));
        step2.Verify(s => s(It.IsAny<PipelineStepConfig>(), It.IsAny<Seq<PipelineStepConfig>>(), It.IsAny<Pipeline<RT>>()));
    }
}
