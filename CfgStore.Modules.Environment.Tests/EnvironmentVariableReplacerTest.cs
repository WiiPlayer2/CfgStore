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
        result.Should().Be(expected);
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
        result.Should().Be(template);
    }
}
