using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

#if NET6_0_OR_GREATER
using System.Runtime.InteropServices;
#endif

namespace DryFish.ILib;

/// <summary>
/// A lightweight utility library for console operations, logging, delays, and application control.
/// </summary>
public static class ILib
{
    private static readonly object _consoleLock = new object();
    private static readonly ConcurrentQueue<LogEntry> _logQueue = new ConcurrentQueue<LogEntry>();
    private static readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private static Task? _backgroundLogger;
    private static bool _loggerRunning = false;
    
    private static readonly Stack<ColorState> _colorStack = new Stack<ColorState>();
    private static bool _debugEnabled = false;
    private static LogLevel _minimumLogLevel = LogLevel.Info;
    private static ILogWriter? _customWriter = null;
    
    // Color mapping cache for performance
    private static readonly Dictionary<string, ConsoleColor?> _colorNameCache = new Dictionary<string, ConsoleColor?>(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<int, ConsoleColor> _rgbCache = new Dictionary<int, ConsoleColor>();
    
    // Security: Mask sensitive data patterns
    private static readonly HashSet<string> _sensitivePatterns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "password", "token", "secret", "key", "apikey", "connectionstring"
    };

    static ILib()
    {
        InitializeColorCache();
        StartBackgroundLogger();
        AppDomain.CurrentDomain.ProcessExit += (s, e) => ShutdownLogger();
        Console.CancelKeyPress += (s, e) => ShutdownLogger();
    }

    #region Initialization & Shutdown

    private static void InitializeColorCache()
    {
        var colorMappings = new (string name, ConsoleColor color)[]
        {
            ("black", ConsoleColor.Black),
            ("darkblue", ConsoleColor.DarkBlue),
            ("darkgreen", ConsoleColor.DarkGreen),
            ("darkcyan", ConsoleColor.DarkCyan),
            ("darkred", ConsoleColor.DarkRed),
            ("darkmagenta", ConsoleColor.DarkMagenta),
            ("darkyellow", ConsoleColor.DarkYellow),
            ("gray", ConsoleColor.Gray),
            ("grey", ConsoleColor.Gray),
            ("darkgray", ConsoleColor.DarkGray),
            ("darkgrey", ConsoleColor.DarkGray),
            ("blue", ConsoleColor.Blue),
            ("green", ConsoleColor.Green),
            ("cyan", ConsoleColor.Cyan),
            ("red", ConsoleColor.Red),
            ("magenta", ConsoleColor.Magenta),
            ("yellow", ConsoleColor.Yellow),
            ("white", ConsoleColor.White)
        };

        foreach (var mapping in colorMappings)
        {
            _colorNameCache[mapping.name] = mapping.color;
        }
    }

    private static void StartBackgroundLogger()
    {
        if (_loggerRunning) return;
        
        _loggerRunning = true;
        _backgroundLogger = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    while (_logQueue.TryDequeue(out var entry))
                    {
                        await WriteLogEntryAsync(entry);
                    }
                    await Task.Delay(10, _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // Silent fail for background logger
                    System.Diagnostics.Debug.WriteLine($"Background logger error: {ex.Message}");
                }
            }
            
            // Flush remaining logs
            while (_logQueue.TryDequeue(out var entry))
            {
                WriteLogEntrySync(entry);
            }
        }, _cts.Token);
    }

    private static void ShutdownLogger()
    {
        if (!_loggerRunning) return;
        
        _cts.Cancel();
        try
        {
            _backgroundLogger?.Wait(TimeSpan.FromSeconds(2));
        }
        catch (AggregateException)
        {
            // Ignore shutdown exceptions
        }
        finally
        {
            _cts.Dispose();
            _loggerRunning = false;
        }
    }

    #endregion

    #region Logging Methods with Performance & Security

    /// <summary>
    /// Sets minimum log level for filtering.
    /// </summary>
    public static void ISetMinimumLogLevel(LogLevel level)
    {
        lock (_consoleLock)
        {
            _minimumLogLevel = level;
        }
    }

    /// <summary>
    /// Sets custom log writer (e.g., for file logging).
    /// </summary>
    public static void ISetLogWriter(ILogWriter writer)
    {
        lock (_consoleLock)
        {
            _customWriter = writer;
        }
    }

    /// <summary>
    /// Displays a notice message (always shown).
    /// </summary>
    public static void INotice(string message)
    {
        if (ShouldLog(LogLevel.Notice))
            QueueLog(LogLevel.Notice, message, null, false);
    }

    /// <summary>
    /// Displays a warning message.
    /// </summary>
    public static void IWarn(string message, [CallerMemberName] string caller = "")
    {
        if (ShouldLog(LogLevel.Warning))
            QueueLog(LogLevel.Warning, message, ConsoleColor.Yellow, false, caller);
    }

    /// <summary>
    /// Displays an informational log message.
    /// </summary>
    public static void ILogInfo(string message, [CallerMemberName] string caller = "")
    {
        if (ShouldLog(LogLevel.Info))
            QueueLog(LogLevel.Info, message, ConsoleColor.Green, true, caller);
    }

    /// <summary>
    /// Displays an error log message.
    /// </summary>
    public static void ILogError(string message, [CallerMemberName] string caller = "")
    {
        if (ShouldLog(LogLevel.Error))
            QueueLog(LogLevel.Error, message, ConsoleColor.Red, true, caller, true);
    }

    /// <summary>
    /// Displays a completion/success message.
    /// </summary>
    public static void ILogComplete(string message, [CallerMemberName] string caller = "")
    {
        if (ShouldLog(LogLevel.Info))
            QueueLog(LogLevel.Info, $"✓ {message}", ConsoleColor.Green, true, caller);
    }

    /// <summary>
    /// Displays a debug log message. Only appears if debug is enabled.
    /// </summary>
    public static void ILogDebug(string message, [CallerMemberName] string caller = "")
    {
        if (_debugEnabled && ShouldLog(LogLevel.Debug))
            QueueLog(LogLevel.Debug, message, ConsoleColor.Cyan, true, caller);
    }

    /// <summary>
    /// Enables or disables debug logging.
    /// </summary>
    public static void ISetDebug(bool enabled)
    {
        lock (_consoleLock)
        {
            _debugEnabled = enabled;
            if (enabled)
            {
                ILogDebug("Debug logging enabled");
            }
        }
    }

    private static bool ShouldLog(LogLevel level)
    {
        return level >= _minimumLogLevel;
    }

    private static void QueueLog(LogLevel level, string message, ConsoleColor? color, bool useTimestamp, 
        string caller = "", bool isError = false)
    {
        // SECURITY: Mask sensitive data
        message = MaskSensitiveData(message);
        
        var entry = new LogEntry
        {
            Level = level,
            Message = message,
            Color = color,
            UseTimestamp = useTimestamp,
            Caller = string.IsNullOrEmpty(caller) ? null : caller,
            IsError = isError,
            Timestamp = DateTime.UtcNow
        };
        
        _logQueue.Enqueue(entry);
    }

    private static async Task WriteLogEntryAsync(LogEntry entry)
    {
        if (_customWriter != null)
        {
            await _customWriter.WriteAsync(entry);
        }
        
        WriteLogEntrySync(entry);
    }

    private static void WriteLogEntrySync(LogEntry entry)
    {
        lock (_consoleLock)
        {
            var originalColor = Console.ForegroundColor;
            
            try
            {
                if (entry.Color.HasValue)
                    Console.ForegroundColor = entry.Color.Value;
                
                var timestamp = entry.UseTimestamp ? GetTimestamp() : "";
                var timestampPart = string.IsNullOrEmpty(timestamp) ? "" : $" {timestamp}";
                var callerPart = !string.IsNullOrEmpty(entry.Caller) ? $" [{entry.Caller}]" : "";
                
                var logLine = $"[{entry.Level}]{timestampPart}{callerPart} - {entry.Message}";
                
                if (entry.IsError)
                    Console.Error.WriteLine(logLine);
                else
                    Console.WriteLine(logLine);
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }
    }

    private static string MaskSensitiveData(string message)
    {
        if (string.IsNullOrEmpty(message)) return message;
        
        var result = message;
        foreach (var pattern in _sensitivePatterns)
        {
            // Simple masking - could be enhanced with regex
            if (result.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result = result.Replace(pattern, "***MASKED***", StringComparison.OrdinalIgnoreCase);
            }
        }
        return result;
    }

    #endregion

    #region Improved Color Management

    /// <summary>
    /// Pushes current console colors to stack and sets new foreground color.
    /// </summary>
    public static void IPushConsoleColor(string color)
    {
        lock (_consoleLock)
        {
            _colorStack.Push(new ColorState
            {
                Foreground = Console.ForegroundColor,
                Background = Console.BackgroundColor
            });
            
            var consoleColor = ParseColorFast(color);
            if (consoleColor.HasValue)
                Console.ForegroundColor = consoleColor.Value;
        }
    }

    /// <summary>
    /// Pops and restores previous console colors from stack.
    /// </summary>
    public static void IPopConsoleColor()
    {
        lock (_consoleLock)
        {
            if (_colorStack.Count > 0)
            {
                var previous = _colorStack.Pop();
                Console.ForegroundColor = previous.Foreground;
                Console.BackgroundColor = previous.Background;
            }
            else
            {
                Console.ResetColor();
            }
        }
    }

    /// <summary>
    /// Sets console color with stack preservation (automatically resets after using block).
    /// </summary>
    public static IDisposable IUseConsoleColor(string color)
    {
        return new ColorScope(color);
    }

    /// <summary>
    /// Resets console colors to system default.
    /// </summary>
    public static void IResetConsoleColor()
    {
        lock (_consoleLock)
        {
            _colorStack.Clear();
            Console.ResetColor();
        }
    }

    private static ConsoleColor? ParseColorFast(string color)
    {
        if (string.IsNullOrEmpty(color)) return null;
        
        // Check cache first for O(1) lookup
        if (_colorNameCache.TryGetValue(color, out var cachedColor))
            return cachedColor;
        
        // Try hex color
        var hexColor = ParseHexColorFast(color);
        if (hexColor.HasValue)
            return hexColor.Value;
        
        IWarn($"Unknown color: {color}. Using default.");
        return null;
    }

    private static ConsoleColor? ParseHexColorFast(string hex)
    {
        if (string.IsNullOrEmpty(hex)) return null;
        
        hex = hex.TrimStart('#');
        if (hex.Length != 6) return null;
        
        // Use cache key for RGB combinations
        if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int rgbValue))
        {
            if (_rgbCache.TryGetValue(rgbValue, out var cached))
                return cached;
            
            var r = (rgbValue >> 16) & 0xFF;
            var g = (rgbValue >> 8) & 0xFF;
            var b = rgbValue & 0xFF;
            
            var result = MapRgbToConsoleColorFast(r, g, b);
            _rgbCache[rgbValue] = result;
            return result;
        }
        
        return null;
    }

    private static ConsoleColor MapRgbToConsoleColorFast(int r, int g, int b)
    {
        // Optimized mapping with pre-calculated thresholds
        int brightness = (r + g + b) / 3;
        
        if (brightness < 80) return ConsoleColor.Black;
        if (brightness > 200) return ConsoleColor.White;
        
        // Find closest color using color distance
        var colors = new (ConsoleColor color, int r, int g, int b)[]
        {
            (ConsoleColor.Black, 0, 0, 0),
            (ConsoleColor.DarkBlue, 0, 0, 128),
            (ConsoleColor.DarkGreen, 0, 128, 0),
            (ConsoleColor.DarkCyan, 0, 128, 128),
            (ConsoleColor.DarkRed, 128, 0, 0),
            (ConsoleColor.DarkMagenta, 128, 0, 128),
            (ConsoleColor.DarkYellow, 128, 128, 0),
            (ConsoleColor.Gray, 192, 192, 192),
            (ConsoleColor.Blue, 0, 0, 255),
            (ConsoleColor.Green, 0, 255, 0),
            (ConsoleColor.Cyan, 0, 255, 255),
            (ConsoleColor.Red, 255, 0, 0),
            (ConsoleColor.Magenta, 255, 0, 255),
            (ConsoleColor.Yellow, 255, 255, 0),
            (ConsoleColor.White, 255, 255, 255)
        };
        
        var closest = ConsoleColor.Gray;
        int minDistance = int.MaxValue;
        
        foreach (var color in colors)
        {
            int dr = r - color.r;
            int dg = g - color.g;
            int db = b - color.b;
            int distance = dr * dr + dg * dg + db * db;
            
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = color.color;
            }
        }
        
        return closest;
    }

    #endregion

    #region Optimized Delay Methods

    private static readonly System.Diagnostics.Stopwatch _delayStopwatch = new System.Diagnostics.Stopwatch();

    /// <summary>
    /// High-precision delay using spin wait for small delays.
    /// </summary>
    public static void IDelay(int milliseconds)
    {
        if (milliseconds <= 0) return;
        
        if (milliseconds < 50)
        {
            // Use spin wait for short delays (more precise)
            var start = Environment.TickCount;
            while (Environment.TickCount - start < milliseconds)
            {
                Thread.SpinWait(10);
            }
        }
        else
        {
            Thread.Sleep(milliseconds);
        }
    }

    /// <summary>
    /// Asynchronously delays with cancellation support.
    /// </summary>
    public static async Task IDelayAsync(int milliseconds, CancellationToken cancellationToken = default)
    {
        if (milliseconds > 0)
            await Task.Delay(milliseconds, cancellationToken);
    }

    #endregion

    #region Secure Input Methods

    /// <summary>
    /// Reads a line securely, optionally masking input.
    /// </summary>
    public static string IReadLineSecure(string? prompt = null, char maskChar = '*')
    {
        lock (_consoleLock)
        {
            if (!string.IsNullOrEmpty(prompt))
                Console.Write(prompt);
            
            var password = new StringBuilder();
            ConsoleKeyInfo key;
            
            do
            {
                key = Console.ReadKey(true);
                
                if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password.Length--;
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    password.Append(key.KeyChar);
                    Console.Write(maskChar);
                }
            }
            while (key.Key != ConsoleKey.Enter);
            
            Console.WriteLine();
            return password.ToString();
        }
    }

    /// <summary>
    /// Reads a line with timeout.
    /// </summary>
    public static string IReadLineWithTimeout(string prompt, int timeoutMilliseconds)
    {
        lock (_consoleLock)
        {
            Console.Write(prompt);
            var task = Task.Run(() => Console.ReadLine());
            
            if (task.Wait(timeoutMilliseconds))
                return task.Result ?? string.Empty;
            
            Console.WriteLine("\nInput timeout.");
            return string.Empty;
        }
    }

    #endregion

    #region Helper Classes

    /// <summary>
    /// Represents a log entry for structured logging.
    /// </summary>
    public class LogEntry
    {
        public LogLevel Level { get; set; }
        public string Message { get; set; } = string.Empty;
        public ConsoleColor? Color { get; set; }
        public bool UseTimestamp { get; set; }
        public string? Caller { get; set; }
        public bool IsError { get; set; }
        public DateTime Timestamp { get; set; }
    }

    private struct ColorState
    {
        public ConsoleColor Foreground;
        public ConsoleColor Background;
    }

    private class ColorScope : IDisposable
    {
        private bool _disposed;

        public ColorScope(string color)
        {
            IPushConsoleColor(color);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                IPopConsoleColor();
                _disposed = true;
            }
        }
    }

    #endregion

    #region Existing Methods (Simplified)

    private static string GetTimestamp()
    {
        return ShowTimestamps ? DateTime.Now.ToString(TimestampFormat) : string.Empty;
    }

    public static bool ShowTimestamps { get; set; } = true;
    public static string TimestampFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";
    
    public static void IClearConsole()
    {
        lock (_consoleLock)
        {
            try { Console.Clear(); }
            catch (IOException) { }
        }
    }

    public static string IReadLine(string? prompt = null)
    {
        lock (_consoleLock)
        {
            if (!string.IsNullOrEmpty(prompt))
                Console.Write(prompt);
            return Console.ReadLine() ?? string.Empty;
        }
    }

    public static void IExit(int exitCode)
    {
        ShutdownLogger();
        Environment.Exit(exitCode);
    }

    #endregion
}

/// <summary>
/// Log level enumeration for filtering.
/// </summary>
public enum LogLevel
{
    Debug = 0,
    Info = 1,
    Notice = 2,
    Warning = 3,
    Error = 4
}

/// <summary>
/// Interface for custom log writers.
/// </summary>
public interface ILogWriter
{
    Task WriteAsync(ILib.LogEntry entry);
}

/// <summary>
/// Extension methods for ILib.
/// </summary>
public static class ILibExtensions
{
    /// <summary>
    /// Logs an exception with optional context.
    /// </summary>
    public static void ILogError(this Exception ex, string? context = null)
    {
        var message = !string.IsNullOrEmpty(context) ? $"{context}: {ex.Message}" : ex.Message;
        ILib.ILogError(message);
        
        if (ex.StackTrace != null)
            ILib.ILogDebug($"Stack trace: {ex.StackTrace}");
    }
}
