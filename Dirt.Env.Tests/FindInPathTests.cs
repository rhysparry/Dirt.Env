using Dirt.Environment;

namespace Dirt.Tests;

public class FindInPathTests
{
    [Fact]
    public void FindInPathCanFindDotNet()
    {
        var env = new Env();
        var notepad = env.FindExecutableInPath("dotnet");
        Assert.NotNull(notepad);
        Assert.True(File.Exists(notepad));
    }

    [Fact]
    [Trait(Traits.PlatformTrait, Traits.Windows)]
    public void FindInPathCanFindDotnetExeOnWindows()
    {
        var env = new Env();
        var dotnet = env.FindExecutableInPath("dotnet.exe");
        Assert.NotNull(dotnet);
        Assert.True(File.Exists(dotnet));
    }
}