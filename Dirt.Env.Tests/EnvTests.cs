namespace Dirt.Tests;

public class EnvTests
{
    [Fact]
    public void NewEnvCreatesSystemEnvironment()
    {
        var env = new Env();
        Assert.NotEmpty(env);
    }

    [Fact]
    public void NewEnvWithCustomEnvironment()
    {
        var env = new Env(new Dictionary<string, string>
        {
            ["key"] = "value"
        });
        Assert.Single(env);
        Assert.True(env.ContainsKey("key"));
        Assert.Equal("value", env["key"]);
    }

    [Fact]
    public void EnvCanGetAnyPathSeparatedValue()
    {
        var builder = new EnvBuilder()
            .AppendPath("Foo", "Bar")
            .AppendPath("Foo", "Baz");
        var env = builder.Build();
        var foo = env.GetPathSeparatedValue("Foo");
        Assert.NotEmpty(foo);
        Assert.Equal(2, foo.Count);
        Assert.Equal("Bar", foo[0]);
        Assert.Equal("Baz", foo[1]);
    }

    [Fact]
    public void EnvKeysAreCaseSensitive()
    {
        var builder = new EnvBuilder()
            .SetVariable("Foo", "Bar")
            .AppendPath("FoO", "Baz");
        var env = builder.Build();
        Assert.Equal(2, env.Count);
        Assert.Contains("Foo", env.Keys);
        Assert.Contains("FoO", env.Keys);
    }

    [Fact]
    public void PathWillReturnEmptyEvenIfUnderlyingVariableIsNotPresent()
    {
        var builder = new EnvBuilder();
        var env = builder.Build();
        Assert.Empty(env);
        Assert.NotNull(env.Path);
        Assert.Empty(env.Path);
    }
}