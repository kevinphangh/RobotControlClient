# Robot Control Client - Dual System Monitor

A simplified .NET console application for monitoring Robot Control System and SmartPack API.

## 🚀 Quick Start

```bash
# Clone/navigate to project
cd RobotControlClient

# Build once
dotnet build

# Run interactive monitor
dotnet run

# Run connection test
dotnet run -- SimpleStatusChecker.cs
```

## Features

**Monitoring (GET requests)**
- ✅ Robot system status with real-time WebSocket updates
- ✅ SmartPack external system status
- ✅ Task queue monitoring
- ✅ Shelf inventory viewing

**Essential Controls (minimal)**
- ✅ Emergency stop/clear
- ✅ Robot homing
- ✅ Worker enable/disable

## Prerequisites

- .NET 8.0 SDK or later
- Robot API running on `http://localhost:8000`
- SmartPack API access (external)

## Running the Application

### Option 1: Interactive Monitor
```bash
dotnet run
```

**Menu Options:**
1. Robot System Status - View robot position, hardware, worker status
2. SmartPack System Status - Check external system transfers
3. Full Connection Test - Test both systems comprehensively
4. Emergency Stop - Activate emergency stop
5. Clear Emergency Stop - Release emergency stop
6. Home Robot - Initialize robot position
7. Enable Worker - Start task processing
8. Disable Worker - Stop task processing
0. Exit

### Option 2: Connection Tester
```bash
dotnet run -- SimpleStatusChecker.cs
```

Tests both systems and displays:
- Robot API connection status
- System hardware/position info
- SmartPack API connection
- Active transfers count
- WebSocket real-time updates

## Project Structure

```
/
├── src/                    # Source code
│   ├── Program.cs         # Interactive monitor
│   ├── SimpleStatusChecker.cs
│   ├── Models/           # Data models
│   └── Services/         # API clients
├── docs/                  # Documentation
│   └── api/              # API references
└── README.md             # This file
```

## Configuration

To connect to different servers, modify URLs in `src/Program.cs`:

```csharp
var apiClient = new RobotApiClient("http://your-robot:8000");
var smartPackClient = new SmartPackApiClient("https://your-smartpack-api");
```

## WebSocket Events

Automatic real-time updates:
- Position changes
- Emergency stop status
- Worker state changes
- System errors

## Example Usage

```bash
# First time setup
dotnet build              # Build once
dotnet run                # Start monitor

# In the menu:
# 1. Press '3' for full connection test
# 2. Press '6' to home robot (if needed)
# 3. Press '1' to check robot status
# 4. Press '2' to check SmartPack status
```

## Dependencies

- **Newtonsoft.Json** (13.0.3) - JSON serialization
- **System.Net.WebSockets.Client** (4.3.2) - WebSocket support

## Troubleshooting

**Build errors:** 
```bash
dotnet clean
dotnet restore
dotnet build
```

**Connection failed:**
- Verify Robot API is running on port 8000
- Check network/firewall settings
- Try the SimpleStatusChecker for diagnostics

## License

MIT