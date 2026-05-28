using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

#if NET6_0_OR_GREATER
using System.Runtime.InteropServices;
#elif NETFRAMEWORK
using System.Runtime.InteropServices;
#endif

namespace DryFish.ILib;

/// <summary>
/// A lightweight utility library for console operations, logging, delays, and application control.
/// </summary>
public static class ILib
{
    private static readonly object _consoleLock = new object();
    private static ConsoleColor _originalForegroundColor;
    private static ConsoleColor _originalBackgroundColor;
    private static bool _colorsSaved = false;
    private static bool _debugEnabled = false;
    
    // Security: Patterns for masking sensitive data
    private static readonly string[] _sensitivePatterns = new string[]
    {
        // Connection strings
        @"(connectionstring|connstr|connection)[\s]*=[\s]*[""']?[^;""']+",
        // Passwords
        @"(password|pwd|passwd|pass)[\s]*=[\s]*[""']?[^;""']+",
        // Tokens & Keys
        @"(token|apikey|apisecret|secret|privatekey|accesstoken)[\s]*=[\s]*[""']?[^;""']+",
        // Credit cards
        @"\b(?:\d[ -]*?){13,16}\b",
        // Email addresses
        @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b",
        // IP addresses
        @"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b"
    };
    
    private static readonly string[] _pathPatterns = new string[]
    {
        // Windows paths with usernames
        @"[A-Za-z]:\\Users\\[^\\]+\\",
        @"[A-Za-z]:\\Documents and Settings\\[^\\]+\\",
        // Linux/Unix home paths
        @"/home/[^/]+/",
        // macOS paths
        @"/Users/[^/]+/",
        // Windows System paths
        @"[A-Za-z]:\\Windows\\[^\\]+\\",
        @"[A-Za-z]:\\Program Files(?: \(x86\))?\\[^\\]+\\",
        // Package manager paths
        @"/home/[^/]+/\.nuget/",
        @"/Users/[^/]+/\.nuget/",
        @"C:\\Users\\[^\\]+\\.nuget\\",
        @"/home/[^/]+/\.dotnet/",
        @"/Users/[^/]+/\.dotnet/",
        @"C:\\Users\\[^\\]+\\.dotnet\\",
    };

    // ========== Basic Logging Methods ==========

    /// <summary>
    /// Displays a notice message to the console.
    /// </summary>
    /// <param name="message">The notice message to display.</param>
    /// <example>
    /// <code>
    /// ILib.INotice("Application started successfully");
    /// </code>
    /// </example>
    public static void INotice(string message)
    {
        lock (_consoleLock)
        {
            Console.WriteLine($"[NOTICE] {message}");
        }
    }

    /// <summary>
    /// Displays a warning message in yellow color.
    /// </summary>
    /// <param name="message">The warning message to display.</param>
    /// <example>
    /// <code>
    /// ILib.IWarn("Low disk space detected");
    /// </code>
    /// </example>
    public static void IWarn(string message)
    {
        lock (_consoleLock)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[WARN] {message}");
            Console.ForegroundColor = originalColor;
        }
    }

    /// <summary>
    /// Displays an informational log message with timestamp in green color.
    /// </summary>
    /// <param name="message">The info message to display.</param>
    /// <example>
    /// <code>
    /// ILib.ILogInfo("User logged in: admin");
    /// </code>
    /// </example>
    public static void ILogInfo(string message)
    {
        lock (_consoleLock)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            var timestamp = GetTimestamp();
            var timestampPart = string.IsNullOrEmpty(timestamp) ? "" : $" {timestamp}";
            Console.WriteLine($"[INFO]{timestampPart} - {message}");
            Console.ForegroundColor = originalColor;
        }
    }

    /// <summary>
    /// Displays an error log message in red color.
    /// </summary>
    /// <param name="message">The error message to display.</param>
    /// <remarks>Sensitive data in the message will be automatically masked.</remarks>
    /// <example>
    /// <code>
    /// ILib.ILogError("Failed to connect to database");
    /// </code>
    /// </example>
    public static void ILogError(string message)
    {
        lock (_consoleLock)
        {
            // Mask sensitive data before logging
            var safeMessage = MaskSensitiveData(message);
            
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            var timestamp = GetTimestamp();
            var timestampPart = string.IsNullOrEmpty(timestamp) ? "" : $" {timestamp}";
            Console.Error.WriteLine($"[ERROR]{timestampPart} - {safeMessage}");
            Console.ForegroundColor = originalColor;
        }
    }

    /// <summary>
    /// Displays a completion/success message in green color with a checkmark.
    /// </summary>
    /// <param name="message">The completion message to display.</param>
    /// <example>
    /// <code>
    /// ILib.ILogComplete("Backup completed successfully");
    /// </code>
    /// </example>
    public static void ILogComplete(string message)
    {
        lock (_consoleLock)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            var timestamp = GetTimestamp();
            var timestampPart = string.IsNullOrEmpty(timestamp) ? "" : $" {timestamp}";
            Console.WriteLine($"[COMPLETE]{timestampPart} - ✓ {message}");
            Console.ForegroundColor = originalColor;
        }
    }

    /// <summary>
    /// Displays a custom log message with a specified prefix and timestamp.
    /// </summary>
    /// <param name="prefix">The custom prefix for the log entry.</param>
    /// <param name="message">The log message to display.</param>
    /// <example>
    /// <code>
    /// ILib.ILog("AUTH", "User authentication successful");
    /// </code>
    /// </example>
    public static void ILog(string prefix, string message)
    {
        lock (_consoleLock)
        {
            var timestamp = GetTimestamp();
            var timestampPart = string.IsNullOrEmpty(timestamp) ? "" : $" {timestamp}";
            Console.WriteLine($"[{prefix}]{timestampPart} - {message}");
        }
    }

    /// <summary>
    /// Displays a colored log message with a specified prefix and timestamp.
    /// </summary>
    /// <param name="color">Color name (black, red, green, yellow, blue, magenta, cyan, white, etc.)</param>
    /// <param name="prefix">The custom prefix for the log entry.</param>
    /// <param name="message">The log message to display.</param>
    /// <example>
    /// <code>
    /// ILib.ILogColor("red", "ERROR", "Critical system failure");
    /// ILib.ILogColor("green", "SUCCESS", "Operation completed");
    /// </code>
    /// </example>
    public static void ILogColor(string color, string prefix, string message)
    {
        lock (_consoleLock)
        {
            var originalColor = Console.ForegroundColor;
            var consoleColor = ParseColor(color);
            
            if (consoleColor.HasValue)
            {
                Console.ForegroundColor = consoleColor.Value;
            }
            else
            {
                var originalColorTemp = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[WARN] Unknown color: {color}. Using default color.");
                Console.ForegroundColor = originalColorTemp;
            }
            
            var timestamp = GetTimestamp();
            var timestampPart = string.IsNullOrEmpty(timestamp) ? "" : $" {timestamp}";
            Console.WriteLine($"[{prefix}]{timestampPart} - {message}");
            
            Console.ForegroundColor = originalColor;
        }
    }

    /// <summary>
    /// Displays a colored log message with automatic prefix based on color name.
    /// </summary>
    /// <param name="color">Color name for both text and prefix.</param>
    /// <param name="message">The log message to display.</param>
    /// <example>
    /// <code>
    /// ILib.ILogColor("cyan", "Debug information");
    /// ILib.ILogColor("magenta", "Custom colored message");
    /// </code>
    /// </example>
    public static void ILogColor(string color, string message)
    {
        ILogColor(color, color.ToUpperInvariant(), message);
    }

    /// <summary>
    /// Displays a debug log message in cyan color. Only appears if debug is enabled.
    /// </summary>
    /// <param name="message">The debug message to display.</param>
    /// <remarks>
    /// Debug mode must be enabled using <see cref="ISetDebug(bool)"/> for these messages to appear.
    /// Sensitive data is automatically masked even in debug mode.
    /// </remarks>
    /// <example>
    /// <code>
    /// ILib.ISetDebug(true);
    /// ILib.ILogDebug("Variable x = 42");
    /// </code>
    /// </example>
    public static void ILogDebug(string message)
    {
        if (!_debugEnabled) return;
        
        lock (_consoleLock)
        {
            // Mask sensitive data even in debug mode
            var safeMessage = MaskSensitiveData(message);
            
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[DEBUG] {DateTime.Now:HH:mm:ss.fff} - {safeMessage}");
            Console.ForegroundColor = originalColor;
        }
    }

    /// <summary>
    /// Enables or disables debug logging.
    /// </summary>
    /// <param name="enabled">True to enable debug logging, false to disable.</param>
    /// <remarks>
    /// When debug mode is enabled, additional debug messages and masked stack traces will be logged.
    /// Do NOT enable debug mode in production environments as stack traces may contain sensitive information.
    /// </remarks>
    /// <example>
    /// <code>
    /// ILib.ISetDebug(true);  // Enable debug mode
    /// ILib.ISetDebug(false); // Disable debug mode
    /// </code>
    /// </example>
    public static void ISetDebug(bool enabled)
    {
        lock (_consoleLock)
        {
            _debugEnabled = enabled;
            if (enabled)
            {
                Console.WriteLine("[DEBUG] Debug logging enabled - Stack traces will be masked for security");
            }
        }
    }

    /// <summary>
    /// Writes a line of text to the console.
    /// </summary>
    /// <param name="msg">The message to write.</param>
    /// <example>
    /// <code>
    /// ILib.IWriteLine("Hello, World!");
    /// </code>
    /// </example>
    public static void IWriteLine(string msg)
    {
        lock(_consoleLock)
        {
            Console.WriteLine(msg);
        }
    }

    /// <summary>
    /// Writes text to the console without a newline.
    /// </summary>
    /// <param name="msg">The message to write.</param>
    /// <example>
    /// <code>
    /// ILib.IWrite("Enter your name: ");
    /// string name = ILib.IReadLine();
    /// </code>
    /// </example>
    public static void IWrite(string msg)
    {
        lock(_consoleLock)
        {
            Console.Write(msg);
        }
    }

    // ========== Delay Methods ==========

    /// <summary>
    /// Pauses the current thread for the specified number of milliseconds.
    /// </summary>
    /// <param name="milliseconds">The number of milliseconds to delay. Positive values only.</param>
    /// <example>
    /// <code>
    /// ILib.IDelay(1000); // Wait for 1 second
    /// </code>
    /// </example>
    public static void IDelay(int milliseconds)
    {
        if (milliseconds > 0)
            Thread.Sleep(milliseconds);
    }

    /// <summary>
    /// Asynchronously delays for the specified number of milliseconds.
    /// </summary>
    /// <param name="milliseconds">The number of milliseconds to delay. Positive values only.</param>
    /// <returns>A task that completes after the specified delay.</returns>
    /// <example>
    /// <code>
    /// await ILib.IDelayAsync(1000); // Wait for 1 second asynchronously
    /// </code>
    /// </example>
    public static async Task IDelayAsync(int milliseconds)
    {
        if (milliseconds > 0)
            await Task.Delay(milliseconds);
    }

    // ========== Exit Method ==========

    /// <summary>
    /// Exits the current application with the specified exit code.
    /// </summary>
    /// <param name="exitCode">The exit code to return to the operating system.</param>
    /// <remarks>This method terminates the current process immediately.</remarks>
    /// <example>
    /// <code>
    /// ILib.IExit(0); // Successful exit
    /// ILib.IExit(1); // Error exit
    /// </code>
    /// </example>
    public static void IExit(int exitCode)
    {
        Environment.Exit(exitCode);
    }

    // ========== Input Methods ==========

    /// <summary>
    /// Reads a line of input from the console with null safety.
    /// </summary>
    /// <returns>The input string, or empty string if null.</returns>
    /// <example>
    /// <code>
    /// string input = ILib.IReadLine();
    /// </code>
    /// </example>
    public static string IReadLine()
    {
        lock (_consoleLock)
        {
            var input = Console.ReadLine();
            return input ?? string.Empty;
        }
    }

    /// <summary>
    /// Reads a line of input with a custom prompt.
    /// </summary>
    /// <param name="prompt">The prompt to display before reading input.</param>
    /// <returns>The input string, or empty string if null.</returns>
    /// <example>
    /// <code>
    /// string name = ILib.IReadLine("Enter your name: ");
    /// </code>
    /// </example>
    public static string IReadLine(string prompt)
    {
        lock (_consoleLock)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            return input ?? string.Empty;
        }
    }

    /// <summary>
    /// Reads a key press from the console without displaying it.
    /// </summary>
    /// <returns>The key info, or null if not available.</returns>
    /// <example>
    /// <code>
    /// var key = ILib.IReadKey();
    /// if (key?.Key == ConsoleKey.Y) { }
    /// </code>
    /// </example>
    public static ConsoleKeyInfo? IReadKey()
    {
        lock (_consoleLock)
        {
            try
            {
                return Console.ReadKey(true);
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Reads a key press with optional display.
    /// </summary>
    /// <param name="intercept">Whether to intercept the key (not display it).</param>
    /// <returns>The key info, or null if not available.</returns>
    /// <example>
    /// <code>
    /// var key = ILib.IReadKey(false); // Display the key pressed
    /// </code>
    /// </example>
    public static ConsoleKeyInfo? IReadKey(bool intercept)
    {
        lock (_consoleLock)
        {
            try
            {
                return Console.ReadKey(intercept);
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Reads a key press with a custom prompt.
    /// </summary>
    /// <param name="prompt">The prompt to display.</param>
    /// <returns>The key info, or null if not available.</returns>
    /// <example>
    /// <code>
    /// var key = ILib.IReadKey("Press any key to continue...");
    /// </code>
    /// </example>
    public static ConsoleKeyInfo? IReadKey(string prompt)
    {
        lock (_consoleLock)
        {
            Console.Write(prompt);
            try
            {
                return Console.ReadKey(true);
            }
            catch
            {
                return null;
            }
        }
    }

    // ========== Console Methods ==========

    /// <summary>
    /// Clears the console screen. Handles IOException gracefully when output is redirected.
    /// </summary>
    /// <remarks>
    /// This method is safe to call even when console output is redirected (e.g., in CI/CD pipelines).
    /// </remarks>
    /// <example>
    /// <code>
    /// ILib.IClearConsole();
    /// </code>
    /// </example>
    public static void IClearConsole()
    {
        lock (_consoleLock)
        {
            try
            {
                Console.Clear();
            }
            catch (IOException)
            {
                // Safe to ignore if console output is redirected (e.g., in CI/CD pipelines)
            }
        }
    }

    // ========== Console Color Methods ==========

    /// <summary>
    /// Sets the console foreground color using a color name.
    /// </summary>
    /// <param name="color">Color name (black, darkblue, darkgreen, darkcyan, darkred, darkmagenta, darkyellow, gray, darkgray, blue, green, cyan, red, magenta, yellow, white).</param>
    /// <remarks>
    /// Supported colors: black, darkblue, darkgreen, darkcyan, darkred, darkmagenta, darkyellow, gray, grey, darkgray, darkgrey, blue, green, cyan, red, magenta, yellow, white.
    /// </remarks>
    /// <example>
    /// <code>
    /// ILib.ISetConsoleColor("red");
    /// ILib.INotice("This is red text");
    /// ILib.IResetConsoleColor();
    /// </code>
    /// </example>
    public static void ISetConsoleColor(string color)
    {
        lock (_consoleLock)
        {
            if (!_colorsSaved)
            {
                _originalForegroundColor = Console.ForegroundColor;
                _originalBackgroundColor = Console.BackgroundColor;
                _colorsSaved = true;
            }

            var consoleColor = ParseColor(color);
            if (consoleColor.HasValue)
            {
                Console.ForegroundColor = consoleColor.Value;
            }
            else
            {
                IWarn($"Unknown color: {color}. Using default.");
            }
        }
    }

    /// <summary>
    /// Sets both console foreground and background colors.
    /// </summary>
    /// <param name="foregroundColor">Text color name.</param>
    /// <param name="backgroundColor">Background color name.</param>
    /// <example>
    /// <code>
    /// ILib.ISetConsoleColor("yellow", "blue");
    /// ILib.INotice("Yellow text on blue background");
    /// ILib.IResetConsoleColor();
    /// </code>
    /// </example>
    public static void ISetConsoleColor(string foregroundColor, string backgroundColor)
    {
        lock (_consoleLock)
        {
            ISetConsoleColor(foregroundColor);
            SetBackgroundColor(backgroundColor);
        }
    }

    /// <summary>
    /// Resets console colors to their original/default values.
    /// </summary>
    /// <example>
    /// <code>
    /// ILib.ISetConsoleColor("red");
    /// ILib.INotice("Red text");
    /// ILib.IResetConsoleColor();
    /// ILib.INotice("Default color");
    /// </code>
    /// </example>
    public static void IResetConsoleColor()
    {
        lock (_consoleLock)
        {
            if (_colorsSaved)
            {
                Console.ForegroundColor = _originalForegroundColor;
                Console.BackgroundColor = _originalBackgroundColor;
            }
            else
            {
                Console.ResetColor();
            }
        }
    }

    // ========== Timezone Methods ==========

    /// <summary>
    /// Gets the current UTC time adjusted for the specified timezone offset.
    /// </summary>
    /// <param name="utcOffset">Timezone offset (e.g., "+7", "-5", "+0530", "+7:30").</param>
    /// <returns>Formatted datetime string in "yyyy-MM-dd HH:mm:ss" format.</returns>
    /// <example>
    /// <code>
    /// string vietnamTime = ILib.IGetTimeUtc("+7");
    /// string nyTime = ILib.IGetTimeUtc("-5");
    /// </code>
    /// </example>
    public static string IGetTimeUtc(string utcOffset)
    {
        var offset = ParseTimezoneOffset(utcOffset);
        var utcNow = DateTime.UtcNow;
        var localTime = utcNow.Add(offset);
        
        return localTime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// Gets the current UTC time adjusted for the specified timezone offset with custom format.
    /// </summary>
    /// <param name="utcOffset">Timezone offset (e.g., "+7", "-5").</param>
    /// <param name="format">Custom datetime format string.</param>
    /// <returns>Formatted datetime string.</returns>
    /// <example>
    /// <code>
    /// string time = ILib.IGetTimeUtc("+7", "HH:mm:ss");
    /// </code>
    /// </example>
    public static string IGetTimeUtc(string utcOffset, string format)
    {
        var offset = ParseTimezoneOffset(utcOffset);
        var utcNow = DateTime.UtcNow;
        var localTime = utcNow.Add(offset);
        
        return localTime.ToString(format);
    }

    /// <summary>
    /// Gets the current time for a specific timezone (cross-platform compatible).
    /// </summary>
    /// <param name="timezoneId">IANA timezone ID (e.g., "Asia/Ho_Chi_Minh", "America/New_York").</param>
    /// <returns>Formatted datetime string in "yyyy-MM-dd HH:mm:ss" format.</returns>
    /// <remarks>
    /// Supports both IANA timezone IDs (Linux/macOS) and Windows timezone IDs.
    /// </remarks>
    /// <example>
    /// <code>
    /// string vnTime = ILib.IGetTimeZone("Asia/Ho_Chi_Minh");
    /// string nyTime = ILib.IGetTimeZone("America/New_York");
    /// </code>
    /// </example>
    public static string IGetTimeZone(string timezoneId)
    {
        var tz = GetTimeZoneInfo(timezoneId);
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        return localTime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// Gets the current time for a specific timezone with custom format.
    /// </summary>
    /// <param name="timezoneId">IANA timezone ID.</param>
    /// <param name="format">Custom datetime format.</param>
    /// <returns>Formatted datetime string.</returns>
    /// <example>
    /// <code>
    /// string time = ILib.IGetTimeZone("Asia/Ho_Chi_Minh", "HH:mm dd/MM/yyyy");
    /// </code>
    /// </example>
    public static string IGetTimeZone(string timezoneId, string format)
    {
        var tz = GetTimeZoneInfo(timezoneId);
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        return localTime.ToString(format);
    }

    // ========== Error Handling Methods ==========

    /// <summary>
    /// Handles an exception with optional logging and graceful exit.
    /// </summary>
    /// <param name="ex">The exception to handle.</param>
    /// <param name="exitCode">Optional exit code. If provided, exits application.</param>
    /// <returns>True if handled gracefully.</returns>
    /// <remarks>
    /// Security: Stack traces are masked to hide sensitive information like:
    /// <list type="bullet">
    /// <item><description>File paths with usernames</description></item>
    /// <item><description>Connection strings</description></item>
    /// <item><description>Passwords, tokens, and API keys</description></item>
    /// <item><description>Email addresses and IP addresses</description></item>
    /// <item><description>Credit card numbers</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// try {
    ///     // Some operation
    /// } catch (Exception ex) {
    ///     ILib.IHandleError(ex);
    /// }
    /// 
    /// // With exit
    /// try {
    ///     // Critical operation
    /// } catch (Exception ex) {
    ///     ILib.IHandleError(ex, 1);
    /// }
    /// </code>
    /// </example>
    public static bool IHandleError(Exception ex, int? exitCode = null)
    {
        // Null check to prevent NullReferenceException
        if (ex == null)
        {
            return IHandleError("Null exception encountered.", exitCode);
        }
        
        bool shouldExit = false;
        int exitCodeValue = 0;
        
        lock (_consoleLock)
        {
            // Mask sensitive data in exception message
            var safeMessage = MaskSensitiveData(ex.Message);
            ILogError($"Exception: {safeMessage}");
            
            // Only log stack trace in debug mode, but still mask sensitive data
            if (_debugEnabled && ex.StackTrace != null)
            {
                var safeStackTrace = MaskStackTrace(ex.StackTrace);
                ILogDebug($"Stack trace (masked): {safeStackTrace}");
            }
            
            if (ex.InnerException != null)
            {
                var safeInnerMessage = MaskSensitiveData(ex.InnerException.Message);
                IWarn($"Inner exception: {safeInnerMessage}");
            }
            
            if (exitCode.HasValue)
            {
                shouldExit = true;
                exitCodeValue = exitCode.Value;
                IWarn($"Exiting with code {exitCodeValue}");
            }
        }
        
        if (shouldExit)
        {
            IExit(exitCodeValue);
        }
        
        return true;
    }

    /// <summary>
    /// Handles an error message without exception.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="exitCode">Optional exit code.</param>
    /// <returns>True if handled.</returns>
    /// <remarks>Sensitive data in the error message will be automatically masked.</remarks>
    /// <example>
    /// <code>
    /// ILib.IHandleError("Configuration file not found");
    /// ILib.IHandleError("Critical error", 1);
    /// </code>
    /// </example>
    public static bool IHandleError(string errorMessage, int? exitCode = null)
    {
        bool shouldExit = false;
        int exitCodeValue = 0;
        
        lock (_consoleLock)
        {
            var safeMessage = MaskSensitiveData(errorMessage);
            ILogError(safeMessage);
            
            if (exitCode.HasValue)
            {
                shouldExit = true;
                exitCodeValue = exitCode.Value;
                IWarn($"Exiting with code {exitCodeValue}");
            }
        }
        
        if (shouldExit)
        {
            IExit(exitCodeValue);
        }
        
        return true;
    }

    // ========== Security Helper Methods ==========

    /// <summary>
    /// Masks sensitive data in a message (passwords, tokens, connection strings, etc.)
    /// </summary>
    private static string MaskSensitiveData(string message)
    {
        if (string.IsNullOrEmpty(message)) return message;
        
        var result = message;
        
        // Apply all sensitive patterns
        foreach (var pattern in _sensitivePatterns)
        {
            try
            {
                result = Regex.Replace(result, pattern, m =>
                {
                    // Preserve the key name but mask the value
                    var match = m.Value;
                    var equalsIndex = match.IndexOf('=');
                    if (equalsIndex > 0)
                    {
                        var key = match.Substring(0, equalsIndex + 1);
                        return key + "[MASKED]";
                    }
                    return "[MASKED]";
                }, RegexOptions.IgnoreCase);
            }
            catch { /* Skip invalid patterns */ }
        }
        
        return result;
    }
    
    /// <summary>
    /// Masks sensitive information in stack traces (paths, usernames, etc.)
    /// Cross-platform compatible for Windows, Linux, and macOS.
    /// </summary>
    private static string MaskStackTrace(string stackTrace)
    {
        if (string.IsNullOrEmpty(stackTrace)) return stackTrace;
        
        var result = stackTrace;
        
        // Mask file paths with usernames
        foreach (var pattern in _pathPatterns)
        {
            try
            {
                result = Regex.Replace(result, pattern, "[MASKED_PATH]", RegexOptions.IgnoreCase);
            }
            catch { /* Skip invalid patterns */ }
        }
        
        // Mask Windows drive letters but keep structure
        result = Regex.Replace(result, @"[A-Za-z]:\\", "[DRIVE]:\\");
        
        // Mask method parameters that might contain sensitive data
        result = Regex.Replace(result, @"\([^)]*\)", match =>
        {
            var masked = Regex.Replace(match.Value, 
                @"(password|token|secret|key|credential)=['""]?[^,)]+", 
                "$1=[MASKED]", 
                RegexOptions.IgnoreCase);
            return masked;
        });
        
        return result;
    }

    // ========== Configuration ==========

    /// <summary>
    /// Gets or sets whether timestamps are shown in logs.
    /// </summary>
    /// <value>True to show timestamps (default), false to hide them.</value>
    /// <example>
    /// <code>
    /// ILib.ShowTimestamps = false;
    /// ILib.ILogInfo("No timestamp in this log");
    /// </code>
    /// </example>
    public static bool ShowTimestamps { get; set; } = true;

    /// <summary>
    /// Gets or sets the timestamp format.
    /// </summary>
    /// <value>A custom datetime format string (default: "yyyy-MM-dd HH:mm:ss").</value>
    /// <example>
    /// <code>
    /// ILib.TimestampFormat = "HH:mm:ss";
    /// ILib.ILogInfo("Shows time only");
    /// </code>
    /// </example>
    public static string TimestampFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";

    private static string GetTimestamp()
    {
        return ShowTimestamps ? DateTime.Now.ToString(TimestampFormat) : string.Empty;
    }

    // ========== Private Helper Methods ==========

    private static TimeZoneInfo GetTimeZoneInfo(string timezoneId)
    {
        // Try direct lookup first
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
        }
        catch { /* Continue to next method */ }

#if NET6_0_OR_GREATER
        // On Windows, try IANA to Windows conversion
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var windowsId = ConvertToWindowsTimezone(timezoneId);
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(windowsId);
            }
            catch { /* Fall through to UTC */ }
        }

        // On Linux/Mac, try Windows to IANA conversion
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || 
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var ianaId = ConvertToIANATimezone(timezoneId);
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(ianaId);
            }
            catch { /* Fall through to UTC */ }
        }
#else
        // For .NET Framework, try IANA to Windows conversion
        try
        {
            var windowsId = ConvertToWindowsTimezone(timezoneId);
            return TimeZoneInfo.FindSystemTimeZoneById(windowsId);
        }
        catch { /* Fall through to UTC */ }
#endif

        // Fallback to UTC
        IWarn($"Cannot find timezone '{timezoneId}', using UTC");
        return TimeZoneInfo.Utc;
    }

    private static ConsoleColor? ParseColor(string color)
    {
        if (string.IsNullOrEmpty(color)) return null;
        
        switch (color.ToLower())
        {
            case "black": return ConsoleColor.Black;
            case "darkblue": return ConsoleColor.DarkBlue;
            case "darkgreen": return ConsoleColor.DarkGreen;
            case "darkcyan": return ConsoleColor.DarkCyan;
            case "darkred": return ConsoleColor.DarkRed;
            case "darkmagenta": return ConsoleColor.DarkMagenta;
            case "darkyellow": return ConsoleColor.DarkYellow;
            case "gray":
            case "grey": return ConsoleColor.Gray;
            case "darkgray":
            case "darkgrey": return ConsoleColor.DarkGray;
            case "blue": return ConsoleColor.Blue;
            case "green": return ConsoleColor.Green;
            case "cyan": return ConsoleColor.Cyan;
            case "red": return ConsoleColor.Red;
            case "magenta": return ConsoleColor.Magenta;
            case "yellow": return ConsoleColor.Yellow;
            case "white": return ConsoleColor.White;
            default: return null;
        }
    }

    private static void SetBackgroundColor(string color)
    {
        var consoleColor = ParseColor(color);
        if (consoleColor.HasValue)
        {
            Console.BackgroundColor = consoleColor.Value;
        }
        else
        {
            IWarn($"Unknown background color: {color}");
        }
    }

    private static TimeSpan ParseTimezoneOffset(string offset)
    {
        offset = offset.Trim();
        bool isNegative = offset.StartsWith("-");
        string numberPart = offset;
        if (numberPart.StartsWith("+") || numberPart.StartsWith("-"))
        {
            numberPart = numberPart.Substring(1);
        }
        
        int hours;
        int minutes = 0;
        
        // Check for colon using IndexOf (compatible with all .NET versions)
        int colonIndex = numberPart.IndexOf(':');
        if (colonIndex >= 0)
        {
            string[] parts = numberPart.Split(':');
            hours = int.Parse(parts[0]);
            if (parts.Length > 1)
                minutes = int.Parse(parts[1]);
        }
        else if (numberPart.Length >= 3 && numberPart.Length <= 4)
        {
            // Format: "0730" or "530" (HHMM or HMM)
            hours = int.Parse(numberPart.Substring(0, numberPart.Length - 2));
            minutes = int.Parse(numberPart.Substring(numberPart.Length - 2));
        }
        else
        {
            // Format: "7", "07", "14"
            hours = int.Parse(numberPart);
        }
        
        if (minutes < 0 || minutes >= 60)
        {
            IWarn($"Invalid minutes in offset: {minutes}. Using 0.");
            minutes = 0;
        }
        
        var timeSpan = new TimeSpan(hours, minutes, 0);
        return isNegative ? -timeSpan : timeSpan;
    }

    private static string ConvertToWindowsTimezone(string ianaTimeZone)
    {
        switch (ianaTimeZone)
        {
            case "Asia/Ho_Chi_Minh":
            case "Asia/Saigon":
            case "Asia/Bangkok":
                return "SE Asia Standard Time";
            case "Asia/Jakarta":
                return "SE Asia Standard Time";
            case "Asia/Singapore":
                return "Singapore Standard Time";
            case "Asia/Tokyo":
                return "Tokyo Standard Time";
            case "Asia/Shanghai":
                return "China Standard Time";
            case "Asia/Kolkata":
                return "India Standard Time";
            case "Asia/Dubai":
                return "Arabian Standard Time";
            case "America/New_York":
                return "Eastern Standard Time";
            case "America/Los_Angeles":
                return "Pacific Standard Time";
            case "America/Chicago":
                return "Central Standard Time";
            case "America/Denver":
                return "Mountain Standard Time";
            case "America/Toronto":
                return "Eastern Standard Time";
            case "America/Vancouver":
                return "Pacific Standard Time";
            case "Europe/London":
                return "GMT Standard Time";
            case "Europe/Paris":
                return "Romance Standard Time";
            case "Europe/Berlin":
                return "Central Europe Standard Time";
            case "Europe/Moscow":
                return "Russian Standard Time";
            case "Australia/Sydney":
                return "AUS Eastern Standard Time";
            case "Australia/Perth":
                return "W. Australia Standard Time";
            case "Pacific/Auckland":
                return "New Zealand Standard Time";
            default:
                return ianaTimeZone;
        }
    }

    private static string ConvertToIANATimezone(string windowsTimeZone)
    {
        switch (windowsTimeZone)
        {
            case "SE Asia Standard Time":
                return "Asia/Bangkok";
            case "Singapore Standard Time":
                return "Asia/Singapore";
            case "Tokyo Standard Time":
                return "Asia/Tokyo";
            case "China Standard Time":
                return "Asia/Shanghai";
            case "India Standard Time":
                return "Asia/Kolkata";
            case "Arabian Standard Time":
                return "Asia/Dubai";
            case "Eastern Standard Time":
                return "America/New_York";
            case "Pacific Standard Time":
                return "America/Los_Angeles";
            case "Central Standard Time":
                return "America/Chicago";
            case "Mountain Standard Time":
                return "America/Denver";
            case "GMT Standard Time":
                return "Europe/London";
            case "Romance Standard Time":
                return "Europe/Paris";
            case "Central Europe Standard Time":
                return "Europe/Berlin";
            case "Russian Standard Time":
                return "Europe/Moscow";
            case "AUS Eastern Standard Time":
                return "Australia/Sydney";
            case "W. Australia Standard Time":
                return "Australia/Perth";
            case "New Zealand Standard Time":
                return "Pacific/Auckland";
            default:
                return windowsTimeZone;
        }
    }
}
