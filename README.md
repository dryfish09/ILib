
# DryFish.ILib

[![Build and Test](https://github.com/dryfish09/ILib/actions/workflows/build.yml/badge.svg)](https://github.com/dryfish09/ILib/actions/workflows/build.yml)
[![NuGet Version](https://img.shields.io/nuget/v/DryFish.ILib)](https://www.nuget.org/packages/DryFish.ILib)
[![NuGet Downloads](https://img.shields.io/nuget/dt/DryFish.ILib)](https://www.nuget.org/packages/DryFish.ILib)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)


A simple, lightweight C# library for console operations, logging, delays, and application control.

## ✨ Features

- 📝 **Logging** - Multiple log levels (Info, Debug, Warn, Notice)
- ⏱️ **Delays** - Both synchronous and asynchronous delay methods
- 🚪 **Exit** - Controlled application exit with status codes
- 🎨 **Colored Console** - Visual feedback with colors
- 📦 **Lightweight** - Zero external dependencies
- 🔧 **Easy to Use** - Simple static API

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
<PackageReference Include="DryFish.ILib" Version="2026.2.0" />
```

## 🚀 Quick Start

```csharp
using DryFish.ILib;

class Program
{
    static void Main()
    {
        ILib.INotice("Application starting...");
        ILib.ILogInfo("Initializing components");
        
        // Check dependencies
        string dependency = "no";
        string requirement = "ILib";
        
        if (dependency != requirement)
        {
            ILib.IWarn("Missing dependencies detected!");
            ILib.IDelay(1000);
            ILib.IExit(1);
        }
        
        ILib.ILogInfo("Ready!");
    }
}
```

## 📚 API Reference

### Logging Methods

| Method | Description | Example |
|--------|-------------|---------|
| `INotice(string message)` | Display a notice message | `ILib.INotice("Hello");` |
| `IWarn(string message)` | Display a warning (yellow) | `ILib.IWarn("Warning!");` |
| `ILogInfo(string message)` | Info log with timestamp | `ILib.ILogInfo("Data saved");` |
| `ILog(string prefix, string message)` | Custom prefixed log | `ILib.ILog("APP", "Message");` |
| `ILogDebug(string message)` | Debug log (DEBUG builds only) | `ILib.ILogDebug("Value: 42");` |

### Control Methods

| Method | Description | Example |
|--------|-------------|---------|
| `IDelay(int milliseconds)` | Synchronous delay | `ILib.IDelay(500);` |
| `IDelayAsync(int milliseconds)` | Asynchronous delay | `await ILib.IDelayAsync(500);` |
| `IExit(int exitCode)` | Exit application | `ILib.IExit(0);` |

## 💡 Examples

### Basic Logging
```csharp
ILib.INotice("Server started");
ILib.ILogInfo("User logged in: admin");
ILib.IWarn("Disk space low: 10% remaining");
ILib.ILog("AUTH", "Authentication successful");
```

### Async Delay
```csharp
async Task ProcessData()
{
    ILib.ILogInfo("Processing started");
    await ILib.IDelayAsync(2000);
    ILib.ILogInfo("Processing completed");
}
```

### Error Handling
```csharp
try
{
    // Your code here
}
catch (Exception ex)
{
    ILib.IWarn($"Error: {ex.Message}");
    ILib.ILog("ERROR", ex.StackTrace);
    ILib.IDelay(1000);
    ILib.IExit(1);
}
```

### Progress Simulation
```csharp
for (int i = 0; i <= 100; i += 20)
{
    ILib.ILogInfo($"Progress: {i}%");
    ILib.IDelay(500);
}
ILib.INotice("Complete!");
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
├── .github/workflows/
│   └── build.yml
├── README.md
├── LICENSE
└── DryFish.ILib.sln
```

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

## 📞 Support

- Create an [Issue](https://github.com/dryfish09/ILib/issues)
- Star ⭐ the repository if you find it useful!

---

**Made with ❤️ by DryFish**

