using Dirt.Environment;

namespace Dirt.Tests;

public class EnvBuilderTests
{
    [Fact]
    public void NewEnvBuilderCreatesEmptyEnvironment()
    {
        var builder = new EnvBuilder();
        var env = builder.Build();
        Assert.Empty(env);
    }

    [Fact]
    public void EnvBuilderAddsVariable()
    {
        var builder = new EnvBuilder()
            .SetVariable("key", "value");
        var env = builder.Build();
        Assert.Equal("value", env["key"]);
    }

    [Fact]
    public void EnvBuilderOverwritesExistingVariablesByDefault()
    {
        var builder = new EnvBuilder()
            .SetVariable("key", "value1")
            .SetVariable("key", "value2");
        var env = builder.Build();
        Assert.Equal("value2", env["key"]);
    }

    [Fact]
    public void EnvBuilderDoesNotOverwriteExistingVariablesWhenSkipDuplicate()
    {
        var builder = new EnvBuilder
            {
                DefaultDuplicateKeyStrategy = IfDuplicate.Skip
            }
            .SetVariable("key", "value1")
            .SetVariable("key", "value2");
        var env = builder.Build();
        Assert.Equal("value1", env["key"]);
    }

    [Fact]
    public void EnvBuilderClearsVariables()
    {
        var builder = new EnvBuilder()
            .SetVariable("key", "value1")
            .Clear();
        var env = builder.Build();
        Assert.Empty(env);
    }

    [Fact]
    public void EnvBuilderCanUnsetPreviouslySetVariables()
    {
        var builder = new EnvBuilder()
            .SetVariable("key", "value")
            .SetVariable("key2", "value2")
            .UnsetVariable("key");

        var env = builder.Build();
        Assert.False(env.ContainsKey("key"));
        Assert.True(env.ContainsKey("key2"));
    }

    private static readonly string[] TwoValues = ["value1", "value2"];

    [Fact]
    public void EnvBuilderAutomaticallyHandlesSettingPathVariable()
    {
        var builder = new EnvBuilder();
        builder
            .SetVariable(builder.PathVariable, TwoValues[0])
            .AppendPath(TwoValues[1]);
        var env = builder.Build();
        Assert.Equal(TwoValues, env.Path);
    }

    [Fact]
    public void CanAppendPathEvenWhenPathVariableIsNotSet()
    {
        var builder = new EnvBuilder()
            .AppendPath(TwoValues[0]);
        var env = builder.Build();
        Assert.Equal(TwoValues[0], env.Path[0]);
    }

    [Fact]
    public void CanPrependPath()
    {
        var builder = new EnvBuilder()
            .AppendPath(TwoValues[1])
            .PrependPath(TwoValues[0]);
        var env = builder.Build();
        Assert.Equal(TwoValues, env.Path);
    }

    [Fact]
    public void CanPrependPathEventWhenPathVariableIsNotSet()
    {
        var builder = new EnvBuilder()
            .PrependPath(TwoValues[0]);
        var env = builder.Build();
        Assert.Equal(TwoValues[0], env.Path[0]);
    }

    [Fact]
    public void SkippingWhenAppendingPathPreservesOriginal()
    {
        var builder = new EnvBuilder
            {
                DefaultDuplicatePathStrategy = IfDuplicatePath.Skip
            }
            .AppendPath(TwoValues[0])
            .AppendPath(TwoValues[1])
            .AppendPath(TwoValues[0]);
        var env = builder.Build();
        Assert.Equal(TwoValues, env.Path);
    }

    [Fact]
    public void SupersedingWhenAppendingPathRemovesOriginal()
    {
        var builder = new EnvBuilder
            {
                DefaultDuplicatePathStrategy = IfDuplicatePath.Supersede
            }
            .AppendPath(TwoValues[0])
            .AppendPath(TwoValues[1])
            .AppendPath(TwoValues[0]);
        var env = builder.Build();
        Assert.Equal(TwoValues[1], env.Path[0]);
        Assert.Equal(TwoValues[0], env.Path[1]);
    }

    [Fact]
    public void IgnoringWhenAppendingPathPreservesOriginal()
    {
        var builder = new EnvBuilder
            {
                DefaultDuplicatePathStrategy = IfDuplicatePath.Ignore
            }
            .AppendPath(TwoValues[0])
            .AppendPath(TwoValues[1])
            .AppendPath(TwoValues[0]);
        var env = builder.Build();
        Assert.Equal(TwoValues.Append(TwoValues[0]), env.Path);
    }

    [Fact]
    public void UsingWindowsPathSetsPathAsExpected()
    {
        var builder = new EnvBuilder()
            .UsingWindowsPath()
            .AppendPath(TwoValues[0])
            .AppendPath(TwoValues[1]);

        var env = builder.Build();
        Assert.True(env.ContainsKey("Path"));
        Assert.Equal($"{TwoValues[0]};{TwoValues[1]}", env["Path"]);
    }

    [Fact]
    public void UsingNixPathSetsPathAsExpected()
    {
        var builder = new EnvBuilder()
            .UsingNixPath()
            .AppendPath(TwoValues[0])
            .AppendPath(TwoValues[1]);

        var env = builder.Build();
        Assert.True(env.ContainsKey("PATH"));
        Assert.Equal($"{TwoValues[0]}:{TwoValues[1]}", env["PATH"]);
    }

    [Fact]
    public void CanInstantiateUsingSystemEnvironment()
    {
        var builder = new EnvBuilder()
            .WithSystemEnvironment();
        var env = builder.Build();
        Assert.NotEmpty(env);
    }

    [Fact]
    public void CanBuildWithCustomEnvironment()
    {
        var builder = new EnvBuilder();
            builder
            .WithEnvironment(new Dictionary<string, string>
            {
                ["key"] = "value",
                [builder.PathVariable] = TwoValues[0]
            })
            .AppendPath(TwoValues[1]);
        var env = builder.Build();
        Assert.Equal("value", env["key"]);
        Assert.Equal(TwoValues, env.Path);
    }

    [Fact]
    public void TryGetValueReturnsFalseWhenKeyIsNotPresent()
    {
        var builder = new EnvBuilder();
        var env = builder.Build();
        Assert.False(env.TryGetValue("key", out _));
    }

    [Fact]
    public void TryGetValueReturnsTrueWhenKeyIsPresent()
    {
        var builder = new EnvBuilder()
            .SetVariable("key", "value");
        var env = builder.Build();
        Assert.True(env.TryGetValue("key", out var value));
        Assert.Equal("value", value);
    }

    [Fact]
    public void ValueContainsExpectedValues()
    {
        var builder = new EnvBuilder()
            .SetVariable("key", "value");
        var env = builder.Build();
        Assert.Contains("key", env.Keys);
        Assert.Contains("value", env.Values);
        Assert.Single(env.Values);
    }
}