
# DryFish.ILib

[![Build and Test](https://github.com/dryfish09/ILib/actions/workflows/build.yml/badge.svg)](https://github.com/dryfish09/ILib/actions/workflows/build.yml)
[![NuGet Version](https://img.shields.io/nuget/v/DryFish.ILib)](https://www.nuget.org/packages/DryFish.ILib)
[![NuGet Downloads](https://img.shields.io/nuget/dt/DryFish.ILib)](https://www.nuget.org/packages/DryFish.ILib)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A simple, lightweight C# library for console operations, logging, delays, and application control.

## ✨ Features

- 📝 **Multiple Log Levels** - Notice, Warning, Info, Error, Debug, Complete
- 🎨 **Colored Console** - Set foreground and background colors by name
- ⏱️ **Delays** - Both synchronous and asynchronous delay methods
- ⌨️ **User Input** - ReadLine and ReadKey with null safety
- 🧹 **Console Control** - Clear console screen with exception handling
- 🌍 **Timezone Support** - Cross-platform timezone conversion (IANA & Windows)
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
<PackageReference Include="DryFish.ILib" Version="2026.8.0" />
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
| `ILogColor(string color, string message)` | Colored log with auto prefix | `ILib.ILogColor("red", "Error!");` |
| `ILogColor(string color, string prefix, string message)` | Colored log with custom prefix | `ILib.ILogColor("cyan", "NET", "Connected");` |
| `ILogDebug(string message)` | Debug log (requires debug mode) | `ILib.ILogDebug("Value: 42");` |

### Console Output Methods

| Method | Description | Example |
|--------|-------------|---------|
| `IWrite(string message)` | Write text without newline | `ILib.IWrite("Enter name: ");` |
| `IWriteLine(string message)` | Write line of text | `ILib.IWriteLine("Hello World!");` |

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
| `ISetBgColor(string color)` | Set background color only | `ILib.ISetBgColor("blue");` |
| `IResetConsoleColor()` | Reset both foreground and background | `ILib.IResetConsoleColor();` |
| `IResetBgColor()` | Reset background color only | `ILib.IResetBgColor();` |

**Supported colors:** 
`black`, `darkblue`, `darkgreen`, `darkcyan`, `darkred`, `darkmagenta`, `darkyellow`, `gray`, `grey`, `darkgray`, `darkgrey`, `blue`, `green`, `cyan`, `red`, `magenta`, `yellow`, `white`

### Timezone Methods

| Method | Description | Example |
|--------|-------------|---------|
| `IGetTimeUtc(string offset)` | UTC + offset | `ILib.IGetTimeUtc("+7");` |
| `IGetTimeUtc(string offset, string format)` | With custom format | `ILib.IGetTimeUtc("+7", "HH:mm");` |
| `IGetTimeZone(string timezoneId)` | IANA timezone | `ILib.IGetTimeZone("Asia/Ho_Chi_Minh");` |
| `IGetTimeZone(string id, string format)` | With custom format | `ILib.IGetTimeZone("America/New_York", "dd/MM/yyyy");` |

**Timezone offset formats:** `"+7"`, `"-5"`, `"+730"`, `"+7:30"`, `"+0530"`

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

### Colored Logging
```csharp
// Using ILogColor with auto prefix
ILib.ILogColor("red", "This is an error message");
ILib.ILogColor("green", "Operation completed successfully");
ILib.ILogColor("yellow", "Warning: Low disk space");
ILib.ILogColor("cyan", "Debug: Variable x = 42");

// Using ILogColor with custom prefix
ILib.ILogColor("magenta", "AUTH", "User logged in");
ILib.ILogColor("blue", "DB", "Connected to database");
```

### Console Colors
```csharp
// Set only foreground
ILib.ISetConsoleColor("cyan");
ILib.INotice("Cyan text");

// Set only background
ILib.ISetBgColor("blue");
ILib.INotice("Text on blue background");

// Set both foreground and background
ILib.ISetConsoleColor("yellow", "blue");
ILib.INotice("Yellow text on blue background");

// Reset colors
ILib.IResetBgColor();      // Reset only background
ILib.IResetConsoleColor(); // Reset both
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

### Direct Console Output
```csharp
// Write without newline
ILib.IWrite("Enter your age: ");
string age = ILib.IReadLine();

// Write line
ILib.IWriteLine($"You entered: {age}");
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
ILib.ILogDebug("This will show");
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
# Build all frameworks
dotnet build src/main/DryFish.ILib.csproj --configuration Release

# Pack NuGet package
dotnet pack src/main/DryFish.ILib.csproj --configuration Release --output ./artifacts

# Include symbols for debugging
dotnet pack src/main/DryFish.ILib.csproj --configuration Release --output ./artifacts --include-symbols
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

- .NET 6.0 or later
- .NET Core 6.0+
- .NET Framework 4.6.2+
- .NET Standard 2.0
- Compatible with Windows, Linux, and macOS

## 🗺️ Roadmap

- [x] Basic logging (Notice, Warn, Info, Error, Complete)
- [x] Colored console output (foreground & background)
- [x] Sync/Async delays
- [x] User input methods
- [x] Direct console output (IWrite, IWriteLine)
- [x] Clear console screen
- [x] Cross-platform timezone
- [x] Debug mode
- [x] Configurable timestamps
- [x] Colored logging (ILogColor)
- [x] Background color control (ISetBgColor, IResetBgColor)

*ILib will no longer be updated, because it has completed what it was supposed to do*

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Guidelines:
- Follow existing code style
- Add tests for new features
- Update documentation
- Ensure all tests pass

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

## 👤 Author

**DryFish**
- GitHub: [@dryfish09](https://github.com/dryfish09)

## 🙏 Acknowledgments

- Built with .NET 6/7/8 and .NET Framework 4.6.2+
- Tested with xUnit
- CI/CD with GitHub Actions
- Cross-platform timezone support
- Dependabot for automated dependency updates

## 📞 Support

- 📧 Create an [Issue](https://github.com/dryfish09/ILib/issues)
- ⭐ Star the repository if you find it useful!
- 🔔 Watch for updates and new releases

---

**Made with ❤️ by DryFish**
