using CfgStore.Application.Abstractions;

namespace CfgStore.Application.Tests;

[TestClass]
public class PipelineBuilderTest
{
    [TestMethod]
    public async Task Build_WithCapturingStep_ReturnsCapturingPipeline()
    {
        // Arrange
        var steps = Seq1<PipelineStep<RT>>((_, _, _, _) => unitAff);

        // Act
        var result = await PipelineBuilder<RT>.Build(steps)
            .Invoke(Mock.Of<ICfgFileStore>(), Seq1(new PipelineStepConfig()))
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
            .Invoke(Mock.Of<ICfgFileStore>(), default)
            .Run(RT.New());

        // Assert
        result.Case.Should().Be(Errors.PipelineFallthrough);
    }

    [TestMethod]
    public async Task Build_WithStep_PassesCfgFileStoreToStep()
    {
        // Arrange
        var cfgFileStore = Mock.Of<ICfgFileStore>();
        var config = new PipelineStepConfig();
        var configs = Seq1(config);
        var step = new Mock<PipelineStep<RT>>();
        var steps = Seq1(step.Object);
        step.Setup(s => s(cfgFileStore, config, It.IsAny<Seq<PipelineStepConfig>>(), It.IsAny<Pipeline<RT>>()))
            .Returns(unitAff);

        // Act
        await PipelineBuilder<RT>.Build(steps)
            .Invoke(cfgFileStore, configs)
            .Run(RT.New());

        // Assert
        step.Verify(s => s(cfgFileStore, config, It.IsAny<Seq<PipelineStepConfig>>(), It.IsAny<Pipeline<RT>>()));
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
        step1.Setup(s => s(It.IsAny<ICfgFileStore>(), config1, It.IsAny<Seq<PipelineStepConfig>>(), It.IsAny<Pipeline<RT>>()))
            .Returns((ICfgFileStore cfgFileStore, PipelineStepConfig _, Seq<PipelineStepConfig> nextConfigs, Pipeline<RT> next) => next(cfgFileStore, nextConfigs));
        step2.Setup(s => s(It.IsAny<ICfgFileStore>(), config2, It.IsAny<Seq<PipelineStepConfig>>(), It.IsAny<Pipeline<RT>>()))
            .Returns(unitAff);

        // Act
        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
        await PipelineBuilder<RT>.Build(steps)
            .Invoke(Mock.Of<ICfgFileStore>(), configs)
            .Run(RT.New());

        // Assert
        step1.Verify(s => s(It.IsAny<ICfgFileStore>(), config1, It.IsAny<Seq<PipelineStepConfig>>(), It.IsAny<Pipeline<RT>>()));
        step2.Verify(s => s(It.IsAny<ICfgFileStore>(), config2, It.IsAny<Seq<PipelineStepConfig>>(), It.IsAny<Pipeline<RT>>()));
    }

    [TestMethod]
    public async Task Build_WithSteps_ExecutesStepsInPipeline()
    {
        // Arrange
        var step1 = new Mock<PipelineStep<RT>>();
        var step2 = new Mock<PipelineStep<RT>>();
        var steps = Seq(step1.Object, step2.Object);
        step1.Setup(s => s(It.IsAny<ICfgFileStore>(), It.IsAny<PipelineStepConfig>(), It.IsAny<Seq<PipelineStepConfig>>(), It.IsAny<Pipeline<RT>>()))
            .Returns((ICfgFileStore cfgFileStore, PipelineStepConfig _, Seq<PipelineStepConfig> nextConfigs, Pipeline<RT> next) => next(cfgFileStore, nextConfigs));
        step2.Setup(s => s(It.IsAny<ICfgFileStore>(), It.IsAny<PipelineStepConfig>(), It.IsAny<Seq<PipelineStepConfig>>(), It.IsAny<Pipeline<RT>>()))
            .Returns(unitAff);

        // Act
        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
        await PipelineBuilder<RT>.Build(steps)
            .Invoke(Mock.Of<ICfgFileStore>(), Seq(new PipelineStepConfig(), new PipelineStepConfig()))
            .Run(RT.New());

        // Assert
        step1.Verify(s => s(It.IsAny<ICfgFileStore>(), It.IsAny<PipelineStepConfig>(), It.IsAny<Seq<PipelineStepConfig>>(), It.IsAny<Pipeline<RT>>()));
        step2.Verify(s => s(It.IsAny<ICfgFileStore>(), It.IsAny<PipelineStepConfig>(), It.IsAny<Seq<PipelineStepConfig>>(), It.IsAny<Pipeline<RT>>()));
    }
}
