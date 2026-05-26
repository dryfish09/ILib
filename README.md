
# DryFish.ILib

[![Build and Test](https://github.com/dryfish09/ILib/actions/workflows/build.yml/badge.svg)](https://github.com/dryfish09/ILib/actions/workflows/build.yml)
[![NuGet Version](https://img.shields.io/nuget/v/DryFish.ILib)](https://www.nuget.org/packages/DryFish.ILib)
[![NuGet Downloads](https://img.shields.io/nuget/dt/DryFish.ILib)](https://www.nuget.org/packages/DryFish.ILib)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A simple, lightweight C# library for console operations, logging, delays, and application control.

## ✨ Features

- 📝 **Multiple Log Levels** - Notice, Warning, Info, Error, Debug, Complete
- ⏱️ **Delays** - Both synchronous and asynchronous delay methods
- 🎨 **Colored Console** - Set colors by name (red, green) or hex code (#FF0000)
- ⌨️ **User Input** - ReadLine and ReadKey with null safety
- 🧹 **Console Control** - Clear console screen with exception handling
- 🌍 **Timezone Support** - Cross-platform timezone conversion (IANA & Windows)
- 🛡️ **Error Handling** - Graceful exception handling with optional exit
- 🔧 **Configurable** - Toggle timestamps, custom formats, debug mode
- 📦 **Lightweight** - Zero external dependencies
- 🚪 **Exit** - Controlled application exit with status codes

## 📦 Installation

### .NET CLI
```bash
dotnet add package DryFish.ILib
```

### Package Manager
```bash
NuGet\Install-Package DryFish.ILib
```

### PackageReference
```xml
<PackageReference Include="DryFish.ILib" Version="2026.5.1" />
```

## 🚀 Quick Start

```csharp
using DryFish.ILib;

// Optional configuration
ILib.ISetDebug(true);
ILib.TimestampFormat = "HH:mm:ss";

ILib.INotice("Application starting...");
ILib.ILogInfo("Initializing components");

// Get user input
string name = ILib.IReadLine("Enter your name: ");
ILib.ILogComplete($"Welcome, {name}!");

// Timezone example
string vnTime = ILib.IGetTimeZone("Asia/Ho_Chi_Minh");
ILib.ILogInfo($"Vietnam time: {vnTime}");
```

## 📚 API Reference

### Logging Methods

| Method | Description | Example |
|--------|-------------|---------|
| `INotice(string message)` | Display a notice message | `ILib.INotice("Hello");` |
| `IWarn(string message)` | Display a warning (yellow) | `ILib.IWarn("Warning!");` |
| `ILogInfo(string message)` | Info log with timestamp | `ILib.ILogInfo("Data saved");` |
| `ILogError(string message)` | Error log (red) | `ILib.ILogError("Failed!");` |
| `ILogComplete(string message)` | Success log with checkmark | `ILib.ILogComplete("Done!");` |
| `ILog(string prefix, string message)` | Custom prefixed log | `ILib.ILog("APP", "Message");` |
| `ILogDebug(string message)` | Debug log (requires debug mode) | `ILib.ILogDebug("Value: 42");` |

### Control Methods

| Method | Description | Example |
|--------|-------------|---------|
| `IDelay(int milliseconds)` | Synchronous delay | `ILib.IDelay(500);` |
| `IDelayAsync(int milliseconds)` | Asynchronous delay | `await ILib.IDelayAsync(500);` |
| `IExit(int exitCode)` | Exit application | `ILib.IExit(0);` |

### Input Methods

| Method | Description | Example |
|--------|-------------|---------|
| `IReadLine()` | Read line with null safety | `string input = ILib.IReadLine();` |
| `IReadLine(string prompt)` | Read line with prompt | `string name = ILib.IReadLine("Name: ");` |
| `IReadKey()` | Read single key | `var key = ILib.IReadKey();` |
| `IReadKey(bool intercept)` | Read key with display option | `ILib.IReadKey(true);` |
| `IReadKey(string prompt)` | Read key with prompt | `ILib.IReadKey("Press any key...");` |

### Console Methods

| Method | Description | Example |
|--------|-------------|---------|
| `IClearConsole()` | Clear the console screen | `ILib.IClearConsole();` |

### Console Color Methods

| Method | Description | Example |
|--------|-------------|---------|
| `ISetConsoleColor(string color)` | Set foreground color | `ILib.ISetConsoleColor("red");` |
| `ISetConsoleColor(string fg, string bg)` | Set both colors | `ILib.ISetConsoleColor("yellow", "blue");` |
| `IResetConsoleColor()` | Reset to default | `ILib.IResetConsoleColor();` |

**Supported colors:** `black`, `darkblue`, `darkgreen`, `darkcyan`, `darkred`, `darkmagenta`, `darkyellow`, `gray`, `darkgray`, `blue`, `green`, `cyan`, `red`, `magenta`, `yellow`, `white`

**Hex support:** `#FF0000` (red), `#00FF00` (green), `#0000FF` (blue)

### Timezone Methods

| Method | Description | Example |
|--------|-------------|---------|
| `IGetTimeUtc(string offset)` | UTC + offset | `ILib.IGetTimeUtc("+7");` |
| `IGetTimeUtc(string offset, string format)` | With custom format | `ILib.IGetTimeUtc("+7", "HH:mm");` |
| `IGetTimeZone(string timezoneId)` | IANA timezone | `ILib.IGetTimeZone("Asia/Ho_Chi_Minh");` |
| `IGetTimeZone(string id, string format)` | With custom format | `ILib.IGetTimeZone("America/New_York", "dd/MM/yyyy");` |

**Timezone offset formats:** `"+7"`, `"-5"`, `"+730"`, `"+7:30"`, `"+0530"`

### Error Handling Methods

| Method | Description | Example |
|--------|-------------|---------|
| `IHandleError(Exception ex)` | Handle exception | `ILib.IHandleError(ex);` |
| `IHandleError(Exception ex, int exitCode)` | Handle and exit | `ILib.IHandleError(ex, 1);` |
| `IHandleError(string message)` | Handle error message | `ILib.IHandleError("Failed!");` |
| `IHandleError(string message, int exitCode)` | Handle and exit | `ILib.IHandleError("Failed!", 1);` |

### Configuration

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ShowTimestamps` | `bool` | `true` | Show/hide timestamps in logs |
| `TimestampFormat` | `string` | `"yyyy-MM-dd HH:mm:ss"` | Custom timestamp format |

| Method | Description |
|--------|-------------|
| `ISetDebug(bool enabled)` | Enable/disable debug logging |

## 💡 Examples

### Basic Logging
```csharp
ILib.INotice("Server started");
ILib.ILogInfo("User logged in: admin");
ILib.ILogError("Database connection failed");
ILib.ILogComplete("Backup completed");
```

### Console Colors
```csharp
// By name
ILib.ISetConsoleColor("cyan");
ILib.INotice("Cyan text");

// By hex
ILib.ISetConsoleColor("#FF00FF");
ILib.INotice("Magenta text");

// Foreground and background
ILib.ISetConsoleColor("yellow", "blue");
ILib.INotice("Yellow on blue");

// Reset to default
ILib.IResetConsoleColor();
```

### Clear Console
```csharp
ILib.INotice("Press any key to clear screen...");
ILib.IReadKey();
ILib.IClearConsole();
ILib.INotice("Screen cleared!");
```

### User Input
```csharp
string name = ILib.IReadLine("Enter your name: ");
ILib.ILogInfo($"Hello, {name}!");

ConsoleKeyInfo? key = ILib.IReadKey("Press Y to continue: ");
if (key?.Key == ConsoleKey.Y)
{
    ILib.ILogComplete("Continuing...");
}
```

### Timezone Examples
```csharp
// Using UTC offset
string vietnam = ILib.IGetTimeUtc("+7");
string ny = ILib.IGetTimeUtc("-5");
string india = ILib.IGetTimeUtc("+5:30");

// Using IANA timezone (cross-platform)
string vn = ILib.IGetTimeZone("Asia/Ho_Chi_Minh");
string jp = ILib.IGetTimeZone("Asia/Tokyo");
string uk = ILib.IGetTimeZone("Europe/London");

// Custom format
string time = ILib.IGetTimeZone("Asia/Ho_Chi_Minh", "HH:mm dd/MM/yyyy");
```

### Error Handling
```csharp
try
{
    int.Parse("not a number");
}
catch (Exception ex)
{
    ILib.IHandleError(ex);
}

// With automatic exit
try
{
    // Critical operation
}
catch (Exception ex)
{
    ILib.IHandleError(ex, 1); // Logs error and exits with code 1
}

// Handle null exception safely
ILib.IHandleError(null as Exception); // Handles gracefully
```

### Configuration
```csharp
// Disable timestamps
ILib.ShowTimestamps = false;
ILib.ILogInfo("No timestamp"); // Output: [INFO] - No timestamp

// Custom timestamp format
ILib.TimestampFormat = "HH:mm:ss";
ILib.ILogInfo("With time only"); // Output: [INFO] 14:30:45 - With time only

// Enable debug mode
ILib.ISetDebug(true);
ILib.ILogDebug("This will show"); // Only shows when debug enabled
```

### Async Delay
```csharp
async Task ProcessData()
{
    ILib.ILogInfo("Processing started");
    await ILib.IDelayAsync(2000);
    ILib.ILogComplete("Processing completed");
}
```

### Progress Simulation
```csharp
for (int i = 0; i <= 100; i += 20)
{
    ILib.ILogInfo($"Progress: {i}%");
    ILib.IDelay(500);
}
ILib.ILogComplete("Complete!");
```

## 🧪 Running Tests

```bash
# Clone repository
git clone https://github.com/dryfish09/ILib.git

# Restore dependencies
dotnet restore

# Run tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 🔧 Building from Source

```bash
# Build
dotnet build src/main/DryFish.ILib.csproj --configuration Release

# Pack NuGet
dotnet pack src/main/DryFish.ILib.csproj --configuration Release --output ./artifacts
```

## 📁 Project Structure

```
ILib/
├── src/
│   ├── main/
│   │   ├── DryFish.ILib.csproj
│   │   └── ILib.cs
│   └── test/
│       ├── DryFish.ILib.Test.csproj
│       └── ILibTest.cs
├── .github/
│   ├── workflows/
│   │   └── build.yml
│   └── dependabot.yml
├── global.json
├── README.md
├── LICENSE
└── DryFish.ILib.sln
```

## 📋 Requirements

- .NET 8.0 or later
- Compatible with Windows, Linux, and macOS

## 🗺️ Roadmap

- [x] Basic logging (Notice, Warn, Info)
- [x] Sync/Async delays
- [x] Console colors (names & hex)
- [x] User input methods
- [x] Clear console screen
- [x] Cross-platform timezone
- [x] Error handling
- [x] Debug mode
- [x] Configurable timestamps
- [ ] File logging
- [ ] JSON configuration
- [ ] Structured logging
- [ ] Log rotation

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

## 👤 Author

**DryFish**
- GitHub: [@dryfish09](https://github.com/dryfish09)

## 🙏 Acknowledgments

- Built with .NET 8.0
- Tested with xUnit
- CI/CD with GitHub Actions
- Cross-platform timezone support
- Dependabot for automated dependency updates

## 📞 Support

- Create an [Issue](https://github.com/dryfish09/ILib/issues)
- Star ⭐ the repository if you find it useful!

---

**Made with ❤️ by DryFish**
