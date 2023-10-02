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
