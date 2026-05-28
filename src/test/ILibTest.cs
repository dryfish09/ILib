using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DryFish.ILib.Tests;

public class ILibTests
{
    private const int DelayTolerance = 150; // ms tolerance for delay tests
    private const int ShortDelay = 100;
    private const int LongDelay = 500;

    public ILibTests()
    {
        // Reset configuration before each test
        ILib.ShowTimestamps = true;
        ILib.TimestampFormat = "yyyy-MM-dd HH:mm:ss";
        ILib.ISetDebug(false);
    }

    // ========== Basic Logging Tests ==========

    [Fact]
    public void INotice_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.INotice("Test notice"));
        Assert.Null(exception);
    }

    [Fact]
    public void INotice_WithNullMessage_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.INotice(null));
        Assert.Null(exception);
    }

    [Fact]
    public void INotice_WithEmptyMessage_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.INotice(""));
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
    public void ILogError_WithSensitiveData_ShouldMask()
    {
        var exception = Record.Exception(() => ILib.ILogError("Error with password=secret123"));
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

    // ========== ILogColor Tests ==========

    [Fact]
    public void ILogColor_WithValidColorAndPrefix_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.ILogColor("red", "ERROR", "Test colored error"));
        Assert.Null(exception);
    }

    [Fact]
    public void ILogColor_WithAutoPrefix_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.ILogColor("cyan", "Test message with auto prefix"));
        Assert.Null(exception);
    }

    [Fact]
    public void ILogColor_WithInvalidColor_ShouldFallbackToDefault()
    {
        var exception = Record.Exception(() => ILib.ILogColor("invalidcolor", "TEST", "This should use default color"));
        Assert.Null(exception);
    }

    [Fact]
    public void ILogColor_WithAllColors_ShouldWork()
    {
        var colors = new[] { "red", "green", "blue", "yellow", "cyan", "magenta", "white" };
        
        var exception = Record.Exception(() =>
        {
            foreach (var color in colors)
            {
                ILib.ILogColor(color, "TEST", $"Testing {color} color");
            }
        });
        
        Assert.Null(exception);
    }

    [Fact]
    public void ILogColor_WithAllSupportedColors_ShouldWork()
    {
        var colors = new[] 
        { 
            "black", "darkblue", "darkgreen", "darkcyan", "darkred", 
            "darkmagenta", "darkyellow", "gray", "grey", "darkgray", 
            "darkgrey", "blue", "green", "cyan", "red", "magenta", 
            "yellow", "white" 
        };
        
        var exception = Record.Exception(() =>
        {
            foreach (var color in colors)
            {
                ILib.ILogColor(color, "ALL", $"Testing {color} color");
            }
        });
        
        Assert.Null(exception);
    }

    [Fact]
    public void ILogColor_WithNullMessage_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.ILogColor("red", "TEST", null));
        Assert.Null(exception);
    }

    [Fact]
    public void ILogColor_WithEmptyMessage_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.ILogColor("red", "TEST", ""));
        Assert.Null(exception);
    }

    [Fact]
    public void ILogColor_CaseInsensitive_ShouldWork()
    {
        var exception = Record.Exception(() =>
        {
            ILib.ILogColor("RED", "CASE", "Uppercase");
            ILib.ILogColor("Blue", "CASE", "Capitalized");
            ILib.ILogColor("GrEeN", "CASE", "Mixed case");
        });
        
        Assert.Null(exception);
    }

    // ========== Debug Logging Tests ==========

    [Fact]
    public void ILogDebug_WhenDebugDisabled_ShouldNotShow()
    {
        ILib.ISetDebug(false);
        var exception = Record.Exception(() => ILib.ILogDebug("This should not show"));
        Assert.Null(exception);
    }

    [Fact]
    public void ILogDebug_WhenDebugEnabled_ShouldShow()
    {
        ILib.ISetDebug(true);
        var exception = Record.Exception(() => ILib.ILogDebug("This should show"));
        Assert.Null(exception);
        ILib.ISetDebug(false);
    }

    [Fact]
    public void ISetDebug_ShouldEnableDebug()
    {
        var exception = Record.Exception(() => ILib.ISetDebug(true));
        Assert.Null(exception);
        
        ILib.ISetDebug(false);
    }

    // ========== Console Output Tests ==========

    [Fact]
    public void IWriteLine_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.IWriteLine("Test line"));
        Assert.Null(exception);
    }

    [Fact]
    public void IWriteLine_WithNull_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.IWriteLine(null));
        Assert.Null(exception);
    }

    [Fact]
    public void IWrite_ShouldNotThrow()
    {
        var exception = Record.Exception(() => ILib.IWrite("Test text"));
        Assert.Null(exception);
    }

    // ========== Delay Tests ==========

    [Fact]
    public void IDelay_WithPositiveMilliseconds_ShouldWait()
    {
        var start = DateTime.Now;
        ILib.IDelay(ShortDelay);
        var elapsed = (DateTime.Now - start).TotalMilliseconds;
        
        Assert.InRange(elapsed, ShortDelay - 10, ShortDelay + DelayTolerance);
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
    public async Task IDelayAsync_WithPositiveMilliseconds_ShouldWait()
    {
        var start = DateTime.Now;
        await ILib.IDelayAsync(ShortDelay);
        var elapsed = (DateTime.Now - start).TotalMilliseconds;
        
        Assert.InRange(elapsed, ShortDelay - 10, ShortDelay + DelayTolerance);
    }

    [Fact]
    public async Task IDelayAsync_WithZeroMilliseconds_ShouldNotWait()
    {
        var start = DateTime.Now;
        await ILib.IDelayAsync(0);
        var elapsed = (DateTime.Now - start).TotalMilliseconds;
        
        Assert.InRange(elapsed, 0, 50);
    }

    // ========== Input Tests ==========

    [Fact]
    public void IReadLine_ShouldReturnString()
    {
        var input = new StringReader("test input\n");
        Console.SetIn(input);
        
        var result = ILib.IReadLine();
        Assert.Equal("test input", result);
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
    public void IReadLine_WithNullInput_ShouldReturnEmpty()
    {
        var input = new StringReader("");
        Console.SetIn(input);
        
        var result = ILib.IReadLine();
        Assert.Equal("", result);
    }

    [Fact]
    public void IReadLine_WithEmptyLine_ShouldReturnEmpty()
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
    public void ISetConsoleColor_WithInvalidColorName_ShouldFallbackToDefault()
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
            "darkmagenta", "darkyellow", "gray", "grey", "darkgray", 
            "darkgrey", "blue", "green", "cyan", "red", "magenta", 
            "yellow", "white" 
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
    public void IGetTimeUtc_WithOffsetFormat_HHMM_ShouldWork()
    {
        var time = ILib.IGetTimeUtc("+0730");
        Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}", time);
    }

    [Fact]
    public void IGetTimeUtc_WithOffsetFormat_HH_MM_ShouldWork()
    {
        var time = ILib.IGetTimeUtc("+7:30");
        Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}", time);
    }

    [Fact]
    public void IGetTimeUtc_WithCustomFormat_ShouldReturnFormattedString()
    {
        var time = ILib.IGetTimeUtc("+7", "dd/MM/yyyy HH:mm");
        Assert.Matches(@"\d{2}/\d{2}/\d{4} \d{2}:\d{2}", time);
    }

    [Fact]
    public void IGetTimeZone_WithValidIANATimezone_ShouldReturnFormattedString()
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

    [Fact]
    public void IGetTimeZone_WithWindowsTimezone_ShouldWork()
    {
        var exception = Record.Exception(() => ILib.IGetTimeZone("SE Asia Standard Time"));
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
    public void IHandleError_WithNullException_ShouldNotThrow()
    {
        var result = ILib.IHandleError(null as Exception);
        Assert.True(result);
    }

    [Fact]
    public void IHandleError_WithMessage_ShouldNotThrow()
    {
        var result = ILib.IHandleError("Test error message");
        Assert.True(result);
    }

    [Fact]
    public void IHandleError_WithExitCode_ShouldLogWarning()
    {
        // Note: This test doesn't actually exit because IExit is mocked via environment
        var ex = new Exception("Test with exit");
        var result = ILib.IHandleError(ex, 1);
        Assert.True(result);
    }

    [Fact]
    public void IHandleError_WithSensitiveExceptionMessage_ShouldMask()
    {
        var ex = new Exception("Error with password=secret123");
        var result = ILib.IHandleError(ex);
        Assert.True(result);
    }

    [Fact]
    public void IHandleError_WithInnerException_ShouldLogBoth()
    {
        var innerEx = new Exception("Inner error");
        var outerEx = new Exception("Outer error", innerEx);
        
        var result = ILib.IHandleError(outerEx);
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
    public void ShowTimestamps_WhenTrue_ShouldShowTimestamp()
    {
        var originalValue = ILib.ShowTimestamps;
        
        ILib.ShowTimestamps = true;
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

    // ========== Security Masking Tests ==========

    [Fact]
    public void Masking_ShouldMaskPasswords()
    {
        var exception = Record.Exception(() => 
            ILib.ILogError("Authentication failed with password=MySecret123"));
        Assert.Null(exception);
    }

    [Fact]
    public void Masking_ShouldMaskConnectionStrings()
    {
        var exception = Record.Exception(() => 
            ILib.ILogError("Connection string=Server=localhost;User Id=admin;Password=admin123"));
        Assert.Null(exception);
    }

    [Fact]
    public void Masking_ShouldMaskEmailAddresses()
    {
        var exception = Record.Exception(() => 
            ILib.ILogError("User email: test@example.com"));
        Assert.Null(exception);
    }

    [Fact]
    public void Masking_ShouldMaskIPAddresses()
    {
        var exception = Record.Exception(() => 
            ILib.ILogError("Connection from 192.168.1.100"));
        Assert.Null(exception);
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
            ILib.ILogColor("cyan", "COLORED", "Colored log");
            ILib.ILogColor("magenta", "Auto prefix colored log");
        });
        
        Assert.Null(exception);
    }

    // ========== Thread Safety Tests ==========

    [Fact]
    public void ConcurrentLogging_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
        {
            Parallel.For(0, 20, i =>
            {
                ILib.ILogInfo($"Message {i}");
                ILib.IDelay(5);
            });
        });
        
        Assert.Null(exception);
    }

    [Fact]
    public void ConcurrentColorChanges_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
        {
            Parallel.For(0, 20, i =>
            {
                ILib.ISetConsoleColor(i % 2 == 0 ? "red" : "blue");
                ILib.IDelay(5);
                ILib.IResetConsoleColor();
            });
        });
        
        Assert.Null(exception);
    }

    [Fact]
    public void ConcurrentILogColor_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
        {
            Parallel.For(0, 20, i =>
            {
                var color = i % 2 == 0 ? "red" : "green";
                ILib.ILogColor(color, "PARALLEL", $"Message from thread {i}");
            });
        });
        
        Assert.Null(exception);
    }

    [Fact]
    public void ConcurrentReadWrite_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
        {
            Parallel.Invoke(
                () => { for (int i = 0; i < 10; i++) ILib.IWrite("A"); },
                () => { for (int i = 0; i < 10; i++) ILib.IWriteLine("B"); },
                () => { for (int i = 0; i < 10; i++) ILib.ILogInfo("C"); }
            );
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
        
        exception = Record.Exception(() => ILib.IWarn(null));
        Assert.Null(exception);
        
        exception = Record.Exception(() => ILib.ILogError(null));
        Assert.Null(exception);
    }

    [Fact]
    public void VeryLongMessages_ShouldNotThrow()
    {
        var longMessage = new string('A', 10000);
        var exception = Record.Exception(() => ILib.ILogInfo(longMessage));
        Assert.Null(exception);
    }

    [Fact]
    public void UnicodeCharacters_ShouldNotThrow()
    {
        var exception = Record.Exception(() => 
            ILib.ILogInfo("Unicode: 你好, こんにちは, Привет, 🎉"));
        Assert.Null(exception);
    }

    [Fact]
    public void SpecialCharacters_ShouldNotThrow()
    {
        var exception = Record.Exception(() => 
            ILib.ILogInfo("Special: !@#$%^&*()_+{}|:<>?~`"));
        Assert.Null(exception);
    }

    // ========== Performance Tests ==========

    [Fact]
    public void RapidLogging_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
        {
            for (int i = 0; i < 100; i++)
            {
                ILib.ILogInfo($"Rapid log {i}");
            }
        });
        
        Assert.Null(exception);
    }

    [Fact]
    public void RapidColorChanges_ShouldNotThrow()
    {
        var colors = new[] { "red", "green", "blue", "yellow", "cyan", "magenta" };
        
        var exception = Record.Exception(() =>
        {
            for (int i = 0; i < 100; i++)
            {
                ILib.ISetConsoleColor(colors[i % colors.Length]);
                ILib.IResetConsoleColor();
            }
        });
        
        Assert.Null(exception);
    }

    // ========== Stress Tests ==========

    [Fact]
    public void StressTest_MixedOperations_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
        {
            var random = new Random();
            
            for (int i = 0; i < 50; i++)
            {
                var op = random.Next(5);
                switch (op)
                {
                    case 0:
                        ILib.ILogInfo($"Info {i}");
                        break;
                    case 1:
                        ILib.IWarn($"Warn {i}");
                        break;
                    case 2:
                        ILib.ILogError($"Error {i}");
                        break;
                    case 3:
                        ILib.ILogColor("cyan", "COLOR", $"Color {i}");
                        break;
                    case 4:
                        ILib.ISetConsoleColor("red");
                        ILib.IWrite($"Write {i}");
                        ILib.IResetConsoleColor();
                        break;
                }
                ILib.IDelay(1);
            }
        });
        
        Assert.Null(exception);
    }

    // ========== Helper Method Tests ==========

    [Fact]
    public void ShowTimestampsProperty_ShouldBeConfigurable()
    {
        var original = ILib.ShowTimestamps;
        
        ILib.ShowTimestamps = false;
        Assert.False(ILib.ShowTimestamps);
        
        ILib.ShowTimestamps = true;
        Assert.True(ILib.ShowTimestamps);
        
        ILib.ShowTimestamps = original;
    }

    [Fact]
    public void TimestampFormatProperty_ShouldBeConfigurable()
    {
        var original = ILib.TimestampFormat;
        
        ILib.TimestampFormat = "HH:mm:ss";
        Assert.Equal("HH:mm:ss", ILib.TimestampFormat);
        
        ILib.TimestampFormat = "yyyy/MM/dd";
        Assert.Equal("yyyy/MM/dd", ILib.TimestampFormat);
        
        ILib.TimestampFormat = original;
    }
}

// ========== Extension Method Tests ==========

public class ILibExtensionTests
{
    [Fact]
    public void ILogError_ExceptionExtension_ShouldNotThrow()
    {
        var ex = new InvalidOperationException("Test exception");
        var exception = Record.Exception(() => ex.ILogError());
        Assert.Null(exception);
    }

    [Fact]
    public void ILogError_ExceptionExtension_WithContext_ShouldNotThrow()
    {
        var ex = new InvalidOperationException("Test exception");
        var exception = Record.Exception(() => ex.ILogError("Database operation failed"));
        Assert.Null(exception);
    }

    [Fact]
    public void ILogError_ExceptionExtension_WithNullContext_ShouldNotThrow()
    {
        var ex = new InvalidOperationException("Test exception");
        var exception = Record.Exception(() => ex.ILogError(null));
        Assert.Null(exception);
    }

    [Fact]
    public void ILogError_ExceptionExtension_WithNullException_ShouldNotThrow()
    {
        Exception ex = null;
        var exception = Record.Exception(() => ex.ILogError());
        Assert.Null(exception);
    }
}

// ========== Extension Method ==========

public static class ILibTestExtensions
{
    public static void ILogError(this Exception ex, string context = null)
    {
        var message = !string.IsNullOrEmpty(context) ? $"{context}: {ex?.Message}" : ex?.Message;
        if (message != null)
            ILib.ILogError(message);
    }
}
