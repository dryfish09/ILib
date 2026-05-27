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
        // Linux/Unix home paths
        @"/home/[^/]+/",
        // Windows System paths
        @"[A-Za-z]:\\Windows\\[^\\]+\\",
        @"[A-Za-z]:\\Program Files(?: \(x86\))?\\[^\\]+\\"
    };

    // ========== Basic Logging Methods ==========

    /// <summary>
    /// Displays a notice message to the console.
    /// </summary>
    /// <param name="message">The notice message to display.</param>
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
    /// Displays a completion/success message in green color.
    /// </summary>
    /// <param name="message">The completion message to display.</param>
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
                IWarn($"Unknown color: {color}. Using default color.");
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
    public static void ILogColor(string color, string message)
    {
        ILogColor(color, color.ToUpperInvariant(), message);
    }

    /// <summary>
    /// Displays a debug log message in cyan color. Only appears if debug is enabled.
    /// </summary>
    /// <param name="message">The debug message to display.</param>
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

    // ========== Delay Methods ==========

    /// <summary>
    /// Pauses the current thread for the specified number of milliseconds.
    /// </summary>
    /// <param name="milliseconds">The number of milliseconds to delay. Positive values only.</param>
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
    public static void IExit(int exitCode)
    {
        Environment.Exit(exitCode);
    }

    // ========== Input Methods ==========

    /// <summary>
    /// Reads a line of input from the console with null safety.
    /// </summary>
    /// <returns>The input string, or empty string if null.</returns>
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
    /// Reads a key press from the console.
    /// </summary>
    /// <returns>The key info, or null if not available.</returns>
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
    /// - File paths with usernames
    /// - Connection strings
    /// - Passwords, tokens, and API keys
    /// - Email addresses and IP addresses
    /// - Credit card numbers
    /// </remarks>
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
                result = Regex.Replace(result, pattern, "[MASKED_PATH]");
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
    public static bool ShowTimestamps { get; set; } = true;

    /// <summary>
    /// Gets or sets the timestamp format.
    /// </summary>
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
