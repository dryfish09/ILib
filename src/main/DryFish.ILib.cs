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

    // ========== Existing Methods ==========
    
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
            Console.WriteLine($"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
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
            Console.WriteLine($"[{prefix}] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
        }
    }

    /// <summary>
    /// Displays a debug log message in cyan color. Only appears in DEBUG builds.
    /// </summary>
    /// <param name="message">The debug message to display.</param>
    public static void ILogDebug(string message)
    {
#if DEBUG
        lock (_consoleLock)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[DEBUG] {DateTime.Now:HH:mm:ss.fff} - {message}");
            Console.ForegroundColor = originalColor;
        }
#endif
    }

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

    /// <summary>
    /// Exits the current application with the specified exit code.
    /// </summary>
    /// <param name="exitCode">The exit code to return to the operating system.</param>
    public static void IExit(int exitCode)
    {
        Environment.Exit(exitCode);
    }

    // ========== New Methods ==========

    /// <summary>
    /// Sets the console foreground color using a color name (e.g., "red") or hex code (e.g., "#FF0000").
    /// </summary>
    /// <param name="color">Color name (red, green, blue, yellow, cyan, magenta, white, black, gray) or hex code (#RRGGBB).</param>
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
                try
                {
                    var hexColor = ParseHexColor(color);
                    if (hexColor.HasValue)
                    {
                        Console.ForegroundColor = hexColor.Value;
                    }
                    else
                    {
                        IWarn($"Unknown color: {color}. Using default.");
                    }
                }
                catch
                {
                    IWarn($"Invalid color format: {color}");
                }
            }
        }
    }

    /// <summary>
    /// Sets both console foreground and background colors.
    /// </summary>
    /// <param name="foregroundColor">Text color name or hex code.</param>
    /// <param name="backgroundColor">Background color name or hex code.</param>
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
    /// Gets the current time for a specific IANA timezone.
    /// </summary>
    /// <param name="timezoneId">IANA timezone ID (e.g., "Asia/Ho_Chi_Minh", "America/New_York").</param>
    /// <returns>Formatted datetime string in "yyyy-MM-dd HH:mm:ss" format.</returns>
    public static string IGetTimeZone(string timezoneId)
    {
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            return localTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
        catch
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(ConvertToWindowsTimezone(timezoneId));
                var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
                return localTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch
            {
                IWarn($"Unknown timezone: {timezoneId}");
                return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
    }

    // ========== Private Helper Methods ==========

    private static ConsoleColor? ParseColor(string color)
    {
        return color?.ToLower() switch
        {
            "black" => ConsoleColor.Black,
            "darkblue" => ConsoleColor.DarkBlue,
            "darkgreen" => ConsoleColor.DarkGreen,
            "darkcyan" => ConsoleColor.DarkCyan,
            "darkred" => ConsoleColor.DarkRed,
            "darkmagenta" => ConsoleColor.DarkMagenta,
            "darkyellow" => ConsoleColor.DarkYellow,
            "gray" or "grey" => ConsoleColor.Gray,
            "darkgray" or "darkgrey" => ConsoleColor.DarkGray,
            "blue" => ConsoleColor.Blue,
            "green" => ConsoleColor.Green,
            "cyan" => ConsoleColor.Cyan,
            "red" => ConsoleColor.Red,
            "magenta" => ConsoleColor.Magenta,
            "yellow" => ConsoleColor.Yellow,
            "white" => ConsoleColor.White,
            _ => null
        };
    }

    private static ConsoleColor? ParseHexColor(string hex)
    {
        hex = hex.TrimStart('#');
        
        if (hex.Length == 6)
        {
            var r = Convert.ToInt32(hex.Substring(0, 2), 16);
            var g = Convert.ToInt32(hex.Substring(2, 2), 16);
            var b = Convert.ToInt32(hex.Substring(4, 2), 16);
            
            return MapRgbToConsoleColor(r, g, b);
        }
        
        return null;
    }

    private static ConsoleColor MapRgbToConsoleColor(int r, int g, int b)
    {
        if (r > 200 && g < 100 && b < 100) return ConsoleColor.Red;
        if (r > 200 && g > 100 && b < 100) return ConsoleColor.DarkYellow;
        if (r > 200 && g > 200 && b < 100) return ConsoleColor.Yellow;
        if (r < 100 && g > 200 && b < 100) return ConsoleColor.Green;
        if (r < 100 && g > 200 && b > 200) return ConsoleColor.Cyan;
        if (r < 100 && g < 100 && b > 200) return ConsoleColor.Blue;
        if (r > 200 && g < 100 && b > 200) return ConsoleColor.Magenta;
        if (r > 200 && g > 200 && b > 200) return ConsoleColor.White;
        if (r < 80 && g < 80 && b < 80) return ConsoleColor.Black;
        
        return ConsoleColor.Gray;
    }

    private static void SetBackgroundColor(string color)
    {
        // Called from within lock, no need for additional locking
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
        bool isNegative = offset.StartsWith('-');
        var numberPart = offset.TrimStart('+', '-');
        
        int hours;
        int minutes = 0;
        
        if (numberPart.Contains(':'))
        {
            // Format: "7:30" or "07:30"
            var parts = numberPart.Split(':');
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
        
        // Validate minutes (0-59)
        if (minutes < 0 || minutes >= 60)
        {
            IWarn($"Invalid minutes in offset: {minutes}. Using 0.");
            minutes = 0;
        }
        
        // Validate hours (-12 to +14 typical range, but allow all)
        if (hours < -12 || hours > 14)
        {
            IWarn($"Unusual hour offset: {hours}. This may be correct for some timezones.");
        }
        
        var timeSpan = new TimeSpan(hours, minutes, 0);
        return isNegative ? -timeSpan : timeSpan;
    }

    private static string ConvertToWindowsTimezone(string ianaTimeZone)
    {
        return ianaTimeZone switch
        {
            "Asia/Ho_Chi_Minh" or "Asia/Saigon" or "Asia/Bangkok" => "SE Asia Standard Time",
            "America/New_York" => "Eastern Standard Time",
            "America/Los_Angeles" => "Pacific Standard Time",
            "America/Chicago" => "Central Standard Time",
            "America/Denver" => "Mountain Standard Time",
            "Europe/London" => "GMT Standard Time",
            "Europe/Paris" => "Romance Standard Time",
            "Europe/Berlin" => "Central Europe Standard Time",
            "Asia/Tokyo" => "Tokyo Standard Time",
            "Asia/Shanghai" => "China Standard Time",
            "Australia/Sydney" => "AUS Eastern Standard Time",
            _ => ianaTimeZone
        };
    }
}
