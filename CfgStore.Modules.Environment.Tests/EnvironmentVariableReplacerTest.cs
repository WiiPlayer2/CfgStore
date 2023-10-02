using FluentAssertions;

namespace CfgStore.Modules.Environment.Tests;

[TestClass]
public class EnvironmentVariableReplacerTest
{
    [TestMethod]
    public void Replace_OnlyPlaceholder_ReturnsValue()
    {
        // Arrange
        var template = "${test}";
        var env = Map(
            ("test", "value"));
        var expected = "value";

        // Act
        var result = EnvironmentVariableReplacer.Replace(template, env);

        // Assert
        result.Case.Should().Be(expected);
    }

    [TestMethod]
    public void Replace_WithMultiplePlaceholdersInTemplate_ReturnsTemplatedValue()
    {
        // Arrange
        var template = "__${test}//${test2}..";
        var env = Map(
            ("test", "value"),
            ("test2", "value2"));
        var expected = "__value//value2..";

        // Act
        var result = EnvironmentVariableReplacer.Replace(template, env);

        // Assert
        result.Case.Should().Be(expected);
    }

    [TestMethod]
    public void Replace_WithoutPlaceholders_ReturnsTemplate()
    {
        // Arrange
        var template = "test";
        var env = Map(
            ("test", "asdfg"));

        // Act
        var result = EnvironmentVariableReplacer.Replace(template, env);

        // Assert
        result.Case.Should().Be(template);
    }

    [TestMethod]
    public void Replace_WithPlaceholderInTemplate_ReturnsTemplatedValue()
    {
        // Arrange
        var template = "__${test}//";
        var env = Map(
            ("test", "value"));
        var expected = "__value//";

        // Act
        var result = EnvironmentVariableReplacer.Replace(template, env);

        // Assert
        result.Case.Should().Be(expected);
    }

    [TestMethod]
    public void Replace_WithPlaceholderWithoutMatchingValue_ReturnsFail()
    {
        // Arrange
        var template = "${not_me}";
        var env = Map(
            ("test", "asdfg"));

        // Act
        var result = EnvironmentVariableReplacer.Replace(template, env);

        // Assert
        result.IsFail.Should().BeTrue();
    }
}
