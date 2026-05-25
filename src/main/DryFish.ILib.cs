namespace DryFish.ILib;

public static class ILib
{
    private static readonly object _consoleLock = new object();
    private static ConsoleColor _originalForegroundColor;
    private static ConsoleColor _originalBackgroundColor;
    private static bool _colorsSaved = false;

    // ========== Existing Methods ==========
    
    public static void INotice(string message)
    {
        lock (_consoleLock)
        {
            Console.WriteLine($"[NOTICE] {message}");
        }
    }

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

    public static void ILog(string prefix, string message)
    {
        lock (_consoleLock)
        {
            Console.WriteLine($"[{prefix}] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
        }
    }

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

    public static void IDelay(int milliseconds)
    {
        if (milliseconds > 0)
            Thread.Sleep(milliseconds);
    }

    public static async Task IDelayAsync(int milliseconds)
    {
        if (milliseconds > 0)
            await Task.Delay(milliseconds);
    }

    public static void IExit(int exitCode)
    {
        Environment.Exit(exitCode);
    }

    // ========== New Methods ==========

    /// <summary>
    /// Set console foreground color using color name or hex code
    /// </summary>
    /// <param name="color">Color name (red, green, blue) or hex (#FF0000)</param>
    public static void ISetConsoleColor(string color)
    {
        lock (_consoleLock)
        {
            // Save original colors first time
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
                // Try to parse as hex
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
    /// Set console foreground and background colors
    /// </summary>
    /// <param name="foregroundColor">Text color name or hex</param>
    /// <param name="backgroundColor">Background color name or hex</param>
    public static void ISetConsoleColor(string foregroundColor, string backgroundColor)
    {
        lock (_consoleLock)
        {
            ISetConsoleColor(foregroundColor);
            SetBackgroundColor(backgroundColor);
        }
    }

    /// <summary>
    /// Reset console colors to original/default values
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
                // Default console colors
                Console.ResetColor();
            }
        }
    }

    /// <summary>
    /// Get UTC time with timezone offset
    /// </summary>
    /// <param name="utcOffset">Timezone offset (e.g., +7, -5, +0530 for half-hour offsets)</param>
    /// <returns>Formatted datetime string</returns>
    public static string IGetTimeUtc(string utcOffset)
    {
        var offset = ParseTimezoneOffset(utcOffset);
        var utcNow = DateTime.UtcNow;
        var localTime = utcNow.Add(offset);
        
        return localTime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// Get UTC time with timezone offset and custom format
    /// </summary>
    /// <param name="utcOffset">Timezone offset (e.g., +7, -5)</param>
    /// <param name="format">Custom datetime format</param>
    /// <returns>Formatted datetime string</returns>
    public static string IGetTimeUtc(string utcOffset, string format)
    {
        var offset = ParseTimezoneOffset(utcOffset);
        var utcNow = DateTime.UtcNow;
        var localTime = utcNow.Add(offset);
        
        return localTime.ToString(format);
    }

    /// <summary>
    /// Get current time for a specific timezone
    /// </summary>
    /// <param name="timezoneId">IANA timezone ID (e.g., "Asia/Ho_Chi_Minh", "America/New_York")</param>
    /// <returns>Formatted datetime string</returns>
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
            // Try Windows timezone format
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
        // Remove # if present
        hex = hex.TrimStart('#');
        
        if (hex.Length == 6)
        {
            // Parse RGB components
            var r = Convert.ToInt32(hex.Substring(0, 2), 16);
            var g = Convert.ToInt32(hex.Substring(2, 2), 16);
            var b = Convert.ToInt32(hex.Substring(4, 2), 16);
            
            // Map RGB to nearest ConsoleColor
            return MapRgbToConsoleColor(r, g, b);
        }
        
        return null;
    }

    private static ConsoleColor MapRgbToConsoleColor(int r, int g, int b)
    {
        // Simple mapping logic
        if (r > 200 && g < 100 && b < 100) return ConsoleColor.Red;
        if (r > 200 && g > 100 && b < 100) return ConsoleColor.DarkYellow;
        if (r > 200 && g > 200 && b < 100) return ConsoleColor.Yellow;
        if (r < 100 && g > 200 && b < 100) return ConsoleColor.Green;
        if (r < 100 && g > 200 && b > 200) return ConsoleColor.Cyan;
        if (r < 100 && g < 100 && b > 200) return ConsoleColor.Blue;
        if (r > 200 && g < 100 && b > 200) return ConsoleColor.Magenta;
        if (r > 200 && g > 200 && b > 200) return ConsoleColor.White;
        if (r < 80 && g < 80 && b < 80) return ConsoleColor.Black;
        
        // Default
        return ConsoleColor.Gray;
    }

    private static void SetBackgroundColor(string color)
    {
        var consoleColor = ParseColor(color);
        if (consoleColor.HasValue)
        {
            Console.BackgroundColor = consoleColor.Value;
        }
    }

    private static TimeSpan ParseTimezoneOffset(string offset)
    {
        offset = offset.Trim();
        bool isNegative = offset.StartsWith('-');
        
        // Remove + or - sign
        var numberPart = offset.TrimStart('+', '-');
        
        int hours;
        int minutes = 0;
        
        if (numberPart.Length >= 3 && numberPart.Length <= 4)
        {
            // Format: +7, +07, +0700, +7:30, +0730
            if (numberPart.Contains(':'))
            {
                var parts = numberPart.Split(':');
                hours = int.Parse(parts[0]);
                if (parts.Length > 1)
                    minutes = int.Parse(parts[1]);
            }
            else if (numberPart.Length >= 3 && numberPart.Length <= 4)
            {
                // Handle formats: 7, 07, 730, 0730
                if (numberPart.Length <= 2)
                {
                    hours = int.Parse(numberPart);
                }
                else
                {
                    hours = int.Parse(numberPart.Substring(0, numberPart.Length - 2));
                    minutes = int.Parse(numberPart.Substring(numberPart.Length - 2));
                }
            }
            else
            {
                hours = int.Parse(numberPart);
            }
        }
        else
        {
            hours = int.Parse(numberPart);
        }
        
        var timeSpan = new TimeSpan(hours, minutes, 0);
        
        return isNegative ? -timeSpan : timeSpan;
    }

    private static string ConvertToWindowsTimezone(string ianaTimeZone)
    {
        // Common IANA to Windows timezone mappings
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
