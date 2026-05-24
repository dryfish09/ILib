using DryFish.ILib;
using Xunit;

namespace DryFish.ILib.Tests;

public class ILibTests
{
    [Fact]
    public void INotice_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.INotice("Test message"));
        Assert.Null(exception);
    }

    [Fact]
    public void IWarn_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.IWarn("Test warning"));
        Assert.Null(exception);
    }

    [Fact]
    public void ILogInfo_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.ILogInfo("Test info"));
        Assert.Null(exception);
    }

    [Fact]
    public void ILog_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.ILog("TEST", "Custom message"));
        Assert.Null(exception);
    }

    [Fact]
    public void ILogDebug_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.ILogDebug("Debug message"));
        Assert.Null(exception);
    }

    [Fact]
    public void IDelay_ShouldWork()
    {
        var exception = Record.Exception(() => ILib.IDelay(10));
        Assert.Null(exception);
    }

    [Fact]
    public async Task IDelayAsync_ShouldWork()
    {
        var exception = await Record.ExceptionAsync(async () => await ILib.IDelayAsync(10));
        Assert.Null(exception);
    }

    // KHÔNG test IExit() vì nó sẽ kill process
    // [Fact] public void IExit_Test() - KHÔNG BAO GIỜ VIẾT TEST NÀY
}
