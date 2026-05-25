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
    public void ILog_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.ILog("TEST", "Test custom log"));
        Assert.Null(exception);
    }

    [Fact]
    public void ILogDebug_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.ILogDebug("Test debug"));
        Assert.Null(exception);
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
    public void ISetConsoleColor_WithInvalidColor_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.ISetConsoleColor("invalidcolor"));
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
        Assert.NotNull(time);
        Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}", time);
    }

    [Fact]
    public void IGetTimeUtc_WithNegativeOffset_ShouldReturnFormattedString()
    {
        var time = ILib.IGetTimeUtc("-5");
        Assert.NotNull(time);
        Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}", time);
    }

    [Fact]
    public void IGetTimeUtc_WithHourMinuteOffset_ShouldReturnFormattedString()
    {
        var time = ILib.IGetTimeUtc("+730");
        Assert.NotNull(time);
        Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}", time);
    }

    [Fact]
    public void IGetTimeUtc_WithColonOffset_ShouldReturnFormattedString()
    {
        var time = ILib.IGetTimeUtc("+5:30");
        Assert.NotNull(time);
        Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}", time);
    }

    [Fact]
    public void IGetTimeUtc_WithCustomFormat_ShouldReturnFormattedString()
    {
        var time = ILib.IGetTimeUtc("+7", "dd/MM/yyyy HH:mm");
        Assert.NotNull(time);
        Assert.Matches(@"\d{2}/\d{2}/\d{4} \d{2}:\d{2}", time);
    }

    [Fact]
    public void IGetTimeUtc_ZeroOffset_ShouldReturnUtcTime()
    {
        var utcNow = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        var time = ILib.IGetTimeUtc("0");
        
        // Allow small difference due to execution time
        Assert.Equal(utcNow.Substring(0, 10), time.Substring(0, 10));
    }

    [Fact]
    public void IGetTimeZone_WithValidIANATimezone_ShouldReturnFormattedString()
    {
        var time = ILib.IGetTimeZone("Asia/Ho_Chi_Minh");
        Assert.NotNull(time);
        Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}", time);
    }

    [Fact]
    public void IGetTimeZone_WithAnotherValidTimezone_ShouldReturnFormattedString()
    {
        var time = ILib.IGetTimeZone("America/New_York");
        Assert.NotNull(time);
        Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}", time);
    }

    [Fact]
    public void IGetTimeZone_WithInvalidTimezone_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.IGetTimeZone("Invalid/Timezone"));
        Assert.Null(exception);
    }

    // ========== Combination Tests ==========

    [Fact]
    public void ColorAndTime_ShouldWorkTogether()
    {
        var exception = Record.Exception(() =>
        {
            ILib.ISetConsoleColor("green");
            var time = ILib.IGetTimeUtc("+7");
            ILib.ILogInfo($"Current time: {time}");
            ILib.IResetConsoleColor();
        });
        
        Assert.Null(exception);
    }

    [Fact]
    public void MultipleColorChanges_ShouldWork()
    {
        var exception = Record.Exception(() =>
        {
            ILib.ISetConsoleColor("red");
            ILib.INotice("Red text");
            
            ILib.ISetConsoleColor("blue");
            ILib.INotice("Blue text");
            
            ILib.ISetConsoleColor("green");
            ILib.INotice("Green text");
            
            ILib.IResetConsoleColor();
        });
        
        Assert.Null(exception);
    }

    // ========== Performance Tests ==========

    [Fact]
    public void IDelay_Precision_ShouldBeWithinAcceptableRange()
    {
        const int delayMs = 500;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        ILib.IDelay(delayMs);
        
        stopwatch.Stop();
        var actualDelay = stopwatch.ElapsedMilliseconds;
        
        // Allow 100ms margin of error
        Assert.InRange(actualDelay, delayMs - 100, delayMs + 100);
    }

    [Fact]
    public async Task IDelayAsync_Precision_ShouldBeWithinAcceptableRange()
    {
        const int delayMs = 500;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        await ILib.IDelayAsync(delayMs);
        
        stopwatch.Stop();
        var actualDelay = stopwatch.ElapsedMilliseconds;
        
        // Allow 100ms margin of error
        Assert.InRange(actualDelay, delayMs - 100, delayMs + 100);
    }

    // ========== Edge Cases ==========

    [Fact]
    public void IGetTimeUtc_WithVeryLargeOffset_ShouldWork()
    {
        var time = ILib.IGetTimeUtc("+14");
        Assert.NotNull(time);
    }

    [Fact]
    public void IGetTimeUtc_WithVeryNegativeOffset_ShouldWork()
    {
        var time = ILib.IGetTimeUtc("-12");
        Assert.NotNull(time);
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
}
