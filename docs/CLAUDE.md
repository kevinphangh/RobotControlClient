# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

### Build and Run
```bash
# Build the project
dotnet build

# Run the interactive monitoring client
dotnet run

# Run the dual-system connection checker
dotnet run --project . -- SimpleStatusChecker.cs

# Restore dependencies
dotnet restore
```

### Testing
No test framework is currently configured. Check README.md or ask user for testing approach before writing tests.

## Architecture

This is a simplified .NET 8.0 console application for monitoring and basic control of the Robot Control System and SmartPack API.

### Project Structure
```
/
├── src/                              # All source code
│   ├── Program.cs                   # Main menu for monitoring and control
│   ├── SimpleStatusChecker.cs       # Dual-system connection tester
│   ├── Models/
│   │   └── RobotModels.cs          # Data models for status/responses
│   └── Services/
│       ├── RobotApiClient.cs       # Robot API client (GET + essential control)
│       ├── SmartPackApiClient.cs   # SmartPack API client (GET only)
│       └── RobotWebSocketClient.cs # WebSocket for real-time updates
├── docs/                            # Documentation
│   ├── api/
│   │   ├── robot_api.md           # Robot API reference
│   │   └── smartpack.md           # SmartPack API reference
│   └── CLAUDE.md                   # This file
├── RobotControlClient.csproj       # Project configuration
└── README.md                        # User documentation
```

## API Endpoints

The client connects to two systems:
- Robot Control API: `http://localhost:8000` (REST + WebSocket)
- SmartPack API: `https://kangaroo.smartpack.dk` (REST only)

## Key Functionality

**Monitoring (GET only)**
- Robot system status with real-time updates
- SmartPack system status and transfer history
- Task queue information
- Shelf inventory information

**Essential Controls (minimal POST/PUT)**
- Emergency stop/clear
- Robot homing
- Worker enable/disable

## Dependencies

- **Newtonsoft.Json** (13.0.3): JSON serialization
- **System.Net.WebSockets.Client** (4.3.2): WebSocket support

## External Documentation

- **robot_api.md**: Full Robot Control API documentation
- **smartpack.md**: SmartPack external API documentation