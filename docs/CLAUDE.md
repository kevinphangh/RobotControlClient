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

# Clean build artifacts
dotnet clean

# Restore dependencies
dotnet restore
```

### Testing
No test framework is currently configured. Check README.md or ask user for testing approach before writing tests.

## Architecture

This is a .NET 8.0 console application that serves as a monitoring and control client for the Robot Warehouse Orchestration System. It provides real-time monitoring and essential control capabilities for both the physical robot system and the SmartPack WMS integration.

### System Context
This client is part of a larger orchestration architecture implementing a **Pull-Model** where:
- Robots actively request tasks from a central Orchestrator
- The Orchestrator manages task assignment and SmartPack integration
- This client monitors and controls the robot-side operations

### Core Components

**Entry Points:**
- `Program.cs`: Interactive menu-driven monitoring interface with WebSocket real-time updates
- `SimpleStatusChecker.cs`: Standalone dual-system connection tester for diagnostics

**Service Layer (`src/Services/`):**
- `RobotApiClient.cs`: HTTP client for Robot Control API (monitoring + essential controls)
- `SmartPackApiClient.cs`: HTTP client for SmartPack external API (monitoring only)
- `RobotWebSocketClient.cs`: WebSocket client for real-time robot status updates

**Data Models (`src/Models/`):**
- `RobotModels.cs`: Strongly-typed models for API responses and system status

### API Integration

The client connects to two distinct systems:

1. **Robot Control API** (`http://localhost:8000`)
   - REST endpoints for status, control, and configuration
   - WebSocket endpoint for real-time updates
   - Supports emergency stop, homing, worker control

2. **SmartPack API** (`https://kangaroo.smartpack.dk`)
   - External WMS system
   - Read-only access for monitoring transfers and system status
   - Uses API key authentication (configured in SmartPackApiClient)

## Code Patterns

### Async/Await Pattern
All API operations use async/await for non-blocking I/O:
```csharp
public async Task<SystemStatus?> GetSystemStatus()
{
    var response = await _httpClient.GetAsync("/system/status");
    // ...
}
```

### Error Handling
Service methods return null on failure with console error output:
```csharp
try 
{
    // API call
    return data;
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    return null;
}
```

### WebSocket Events
Real-time updates handled through event subscriptions:
```csharp
wsClient.OnPositionUpdate += (pos) => { /* handle update */ };
wsClient.OnEmergencyStop += () => { /* handle e-stop */ };
```

## Key Functionality

**Monitoring Capabilities:**
- Robot position, hardware status, worker state
- SmartPack transfers and system health
- Task queue status
- Shelf inventory levels
- Real-time WebSocket updates

**Control Operations:**
- Emergency stop activation/clearing
- Robot homing sequence
- Worker process enable/disable

## Dependencies

- **Newtonsoft.Json** (13.0.3): JSON serialization for API communication
- **System.Net.WebSockets.Client** (4.3.2): WebSocket support for real-time updates
- **.NET 8.0**: Target framework with nullable reference types enabled

## Development Notes

- The project uses `EnableDefaultCompileItems=false` with explicit `src/**/*.cs` compilation
- Nullable reference types are enabled (`<Nullable>enable</Nullable>`)
- The root namespace is `RobotControlClient`
- Connection URLs are hardcoded in Program.cs - modify there for different environments

## Related Documentation

- `docs/api/robot_api.md`: Complete Robot Control API specification
- `docs/api/smartpack_api.md`: SmartPack external API documentation
- `docs/api/robot_orchestrator_plan.md`: Overall system architecture and orchestration strategy