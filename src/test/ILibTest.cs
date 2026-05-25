using DryFish.ILib;
using Xunit;

namespace DryFish.ILib.Tests;

public class ILibTests
{
    // ========== Basic Logging Tests ==========

    [Fact]
    public void INotice_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.INotice("Test notice"));
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
    public void ILogError_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.ILogError("Test error"));
        Assert.Null(exception);
    }

    [Fact]
    public void ILogComplete_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.ILogComplete("Test complete"));
        Assert.Null(exception);
    }

    [Fact]
    public void ILog_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.ILog("TEST", "Custom log"));
        Assert.Null(exception);
    }

    [Fact]
    public void ILogDebug_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.ILogDebug("Test debug"));
        Assert.Null(exception);
    }

    [Fact]
    public void ISetDebug_ShouldEnableDebug()
    {
        var exception = Record.Exception(() => ILib.ISetDebug(true));
        Assert.Null(exception);
        
        ILib.ISetDebug(false);
    }

    // ========== Delay Tests ==========

    [Fact]
    public void IDelay_WithPositiveMilliseconds_ShouldWait()
    {
        var start = DateTime.Now;
        ILib.IDelay(100);
        var elapsed = (DateTime.Now - start).TotalMilliseconds;
        
        Assert.InRange(elapsed, 90, 250);
    }

    [Fact]
    public void IDelay_WithZeroMilliseconds_ShouldNotWait()
    {
        var start = DateTime.Now;
        ILib.IDelay(0);
        var elapsed = (DateTime.Now - start).TotalMilliseconds;
        
        Assert.InRange(elapsed, 0, 50);
    }

    [Fact]
    public void IDelay_WithNegativeMilliseconds_ShouldNotWait()
    {
        var start = DateTime.Now;
        ILib.IDelay(-100);
        var elapsed = (DateTime.Now - start).TotalMilliseconds;
        
        Assert.InRange(elapsed, 0, 50);
    }

    [Fact]
    public async Task IDelayAsync_ShouldWait()
    {
        var start = DateTime.Now;
        await ILib.IDelayAsync(100);
        var elapsed = (DateTime.Now - start).TotalMilliseconds;
        
        Assert.InRange(elapsed, 90, 250);
    }

    // ========== Input Tests ==========

    [Fact]
    public void IReadLine_ShouldReturnString()
    {
        var input = new StringReader("test input\n");
        Console.SetIn(input);
        
        var result = ILib.IReadLine();
        Assert.Equal("test input", result);
        
        var standardInput = new StreamReader(Console.OpenStandardInput());
        Console.SetIn(standardInput);
    }

    [Fact]
    public void IReadLine_WithPrompt_ShouldReturnString()
    {
        var input = new StringReader("hello\n");
        Console.SetIn(input);
        
        var result = ILib.IReadLine("Enter: ");
        Assert.Equal("hello", result);
    }

    [Fact]
    public void IReadLine_EmptyInput_ReturnsEmptyString()
    {
        var input = new StringReader("\n");
        Console.SetIn(input);
        
        var result = ILib.IReadLine();
        Assert.Equal("", result);
    }

    [Fact]
    public void IReadKey_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.IReadKey());
        Assert.Null(exception);
    }

    [Fact]
    public void IReadKey_WithIntercept_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.IReadKey(true));
        Assert.Null(exception);
    }

    [Fact]
    public void IReadKey_WithPrompt_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.IReadKey("Press key: "));
        Assert.Null(exception);
    }

    // ========== Console Tests ==========

    [Fact]
    public void IClearConsole_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.IClearConsole());
        Assert.Null(exception);
    }

    // ========== Console Color Tests ==========

    [Fact]
    public void ISetConsoleColor_WithValidColorName_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.ISetConsoleColor("red"));
        Assert.Null(exception);
        ILib.IResetConsoleColor();
    }

    [Fact]
    public void ISetConsoleColor_WithValidHex_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.ISetConsoleColor("#FF0000"));
        Assert.Null(exception);
        ILib.IResetConsoleColor();
    }

    [Fact]
    public void ISetConsoleColor_WithForegroundAndBackground_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.ISetConsoleColor("yellow", "blue"));
        Assert.Null(exception);
        ILib.IResetConsoleColor();
    }

    [Fact]
    public void IResetConsoleColor_ShouldNotThrow()
    {
        ILib.ISetConsoleColor("red");
        var exception = Record.Exception(() => ILib.IResetConsoleColor());
        Assert.Null(exception);
    }

    [Fact]
    public void AllColorNames_ShouldWork()
    {
        var colors = new[] { 
            "black", "darkblue", "darkgreen", "darkcyan", "darkred", 
            "darkmagenta", "darkyellow", "gray", "darkgray", "blue", 
            "green", "cyan", "red", "magenta", "yellow", "white" 
        };
        
        foreach (var color in colors)
        {
            var exception = Record.Exception(() => ILib.ISetConsoleColor(color));
            Assert.Null(exception);
        }
        
        ILib.IResetConsoleColor();
    }

    // ========== Timezone Tests ==========

    [Fact]
    public void IGetTimeUtc_WithPositiveOffset_ShouldReturnFormattedString()
    {
        var time = ILib.IGetTimeUtc("+7");
        Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}", time);
    }

    [Fact]
    public void IGetTimeUtc_WithNegativeOffset_ShouldReturnFormattedString()
    {
        var time = ILib.IGetTimeUtc("-5");
        Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}", time);
    }

    [Fact]
    public void IGetTimeUtc_WithCustomFormat_ShouldReturnFormattedString()
    {
        var time = ILib.IGetTimeUtc("+7", "dd/MM/yyyy HH:mm");
        Assert.Matches(@"\d{2}/\d{2}/\d{4} \d{2}:\d{2}", time);
    }

    [Fact]
    public void IGetTimeZone_WithValidTimezone_ShouldReturnFormattedString()
    {
        var time = ILib.IGetTimeZone("Asia/Ho_Chi_Minh");
        Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}", time);
    }

    [Fact]
    public void IGetTimeZone_WithCustomFormat_ShouldReturnFormattedString()
    {
        var time = ILib.IGetTimeZone("Asia/Ho_Chi_Minh", "HH:mm");
        Assert.Matches(@"\d{2}:\d{2}", time);
    }

    [Fact]
    public void IGetTimeZone_WithInvalidTimezone_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.IGetTimeZone("Invalid/Timezone"));
        Assert.Null(exception);
    }

    // ========== Error Handling Tests ==========

    [Fact]
    public void IHandleError_WithException_ShouldNotThrow()
    {
        var ex = new InvalidOperationException("Test exception");
        var result = ILib.IHandleError(ex);
        Assert.True(result);
    }

    [Fact]
    public void IHandleError_WithMessage_ShouldNotThrow()
    {
        var result = ILib.IHandleError("Test error message");
        Assert.True(result);
    }

    // ========== Configuration Tests ==========

    [Fact]
    public void ShowTimestamps_WhenFalse_ShouldHideTimestamp()
    {
        var originalValue = ILib.ShowTimestamps;
        
        ILib.ShowTimestamps = false;
        var exception = Record.Exception(() => ILib.ILogInfo("Test"));
        Assert.Null(exception);
        
        ILib.ShowTimestamps = originalValue;
    }

    [Fact]
    public void TimestampFormat_ShouldBeCustomizable()
    {
        var originalFormat = ILib.TimestampFormat;
        
        ILib.TimestampFormat = "HH:mm:ss";
        var exception = Record.Exception(() => ILib.ILogInfo("Test"));
        Assert.Null(exception);
        
        ILib.TimestampFormat = originalFormat;
    }

    // ========== Combined Tests ==========

    [Fact]
    public void ColorAndTime_ShouldWorkTogether()
    {
        var exception = Record.Exception(() =>
        {
            ILib.ISetConsoleColor("green");
            var time = ILib.IGetTimeUtc("+7");
            ILib.ILogInfo($"Time: {time}");
            ILib.IResetConsoleColor();
        });
        
        Assert.Null(exception);
    }

    [Fact]
    public void AllLogLevels_ShouldWorkTogether()
    {
        var exception = Record.Exception(() =>
        {
            ILib.INotice("Notice");
            ILib.IWarn("Warning");
            ILib.ILogInfo("Info");
            ILib.ILogError("Error");
            ILib.ILogComplete("Complete");
            ILib.ILog("CUSTOM", "Custom");
        });
        
        Assert.Null(exception);
    }

    // ========== Edge Cases ==========

    [Fact]
    public void NullOrEmptyMessages_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.INotice(null));
        Assert.Null(exception);
        
        exception = Record.Exception(() => ILib.ILogInfo(""));
        Assert.Null(exception);
    }

    [Fact]
    public void ISetConsoleColor_AfterReset_ShouldWorkAgain()
    {
        ILib.ISetConsoleColor("red");
        ILib.IResetConsoleColor();
        
        var exception = Record.Exception(() => ILib.ISetConsoleColor("blue"));
        Assert.Null(exception);
        
        ILib.IResetConsoleColor();
    }

    // ========== Thread Safety Test ==========

    [Fact]
    public void ConcurrentLogging_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
        {
            Parallel.For(0, 10, i =>
            {
                ILib.ILogInfo($"Message {i}");
                ILib.IDelay(10);
            });
        });
        
        Assert.Null(exception);
    }
}
