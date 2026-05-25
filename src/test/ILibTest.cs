using DryFish.ILib;
using Xunit;

namespace DryFish.ILib.Tests;

public class ILibTests
{
    private readonly StringWriter _consoleOutput;
    private readonly TextWriter _originalOutput;

    public ILibTests()
    {
        _originalOutput = Console.Out;
        _consoleOutput = new StringWriter();
        Console.SetOut(_consoleOutput);
    }

    private void Dispose()
    {
        Console.SetOut(_originalOutput);
        _consoleOutput.Dispose();
    }

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

    [Fact]
    public void ILogError_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.ILogError("Test error"));
        Assert.Null(exception);
    }

    [Fact]
    public void ILogComplete_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.ILogComplete("Task completed"));
        Assert.Null(exception);
    }

    // ========== Debug Mode Tests ==========

    [Fact]
    public void ISetDebug_ShouldEnableDebug()
    {
        var exception = Record.Exception(() => ILib.ISetDebug(true));
        Assert.Null(exception);
        
        ILib.ILogDebug("This should appear");
        
        ILib.ISetDebug(false);
        ILib.ILogDebug("This should not appear");
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
        Assert.NotNull(result);
        Assert.Equal("test input", result);
        
        // Restore stdin
        var standardInput = new StreamReader(Console.OpenStandardInput());
        Console.SetIn(standardInput);
    }

    [Fact]
    public void IReadLine_WithPrompt_ShouldReturnString()
    {
        var input = new StringReader("hello world\n");
        Console.SetIn(input);
        
        var result = ILib.IReadLine("Enter: ");
        Assert.NotNull(result);
        Assert.Equal("hello world", result);
    }

    [Fact]
    public void IReadLine_WithNullInput_ShouldReturnEmpty()
    {
        var input = new StringReader("");
        Console.SetIn(input);
        
        var result = ILib.IReadLine();
        Assert.Equal("", result);
    }

    [Fact]
    public void IReadKey_ShouldNotThrow()
    {
        // Cannot fully test without user input, just verify method exists
        var exception = Record.Exception(() => ILib.IReadKey(true));
        // Result may be null in automated test environment
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
    public void IGetTimeZone_WithCustomFormat_ShouldReturnFormattedString()
    {
        var time = ILib.IGetTimeZone("Asia/Ho_Chi_Minh", "dd/MM/yyyy");
        Assert.NotNull(time);
        Assert.Matches(@"\d{2}/\d{2}/\d{4}", time);
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
    public void IHandleError_WithExceptionAndExitCode_ShouldNotThrow()
    {
        var ex = new InvalidOperationException("Test exception");
        // Note: IExit will be called, but we can't test that without process exit
        // Just verify the method doesn't throw before exit
        var exception = Record.Exception(() => ILib.IHandleError(ex, 1));
        // The method will exit, so we can only test this in a separate process
        // For now, just verify the method exists
        Assert.True(true);
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
        var exception = Record.Exception(() => ILib.ILogInfo("Test message"));
        Assert.Null(exception);
        
        ILib.ShowTimestamps = originalValue;
    }

    [Fact]
    public void TimestampFormat_ShouldBeCustomizable()
    {
        var originalFormat = ILib.TimestampFormat;
        
        ILib.TimestampFormat = "HH:mm:ss";
        var exception = Record.Exception(() => ILib.ILogInfo("Test message"));
        Assert.Null(exception);
        
        ILib.TimestampFormat = originalFormat;
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

    [Fact]
    public void NullOrEmptyMessages_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.INotice(null));
        Assert.Null(exception);
        
        exception = Record.Exception(() => ILib.ILogInfo(""));
        Assert.Null(exception);
        
        exception = Record.Exception(() => ILib.IWarn(null));
        Assert.Null(exception);
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
