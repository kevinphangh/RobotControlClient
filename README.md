# Robot Control Client

A .NET C# console application for communicating with the Robot Control System API.

## Features

- Full REST API client for robot control
- Real-time WebSocket connection for status updates
- Interactive console menu for all operations
- Support for:
  - System control (emergency stop, worker management)
  - Robot motion (homing, direct movement, queued movements)
  - Box operations (move boxes between shelves)
  - Inventory management (shelf CRUD operations)
  - Task queue management
  - Gripper/vacuum control
  - Smart task creation from barcodes

## Prerequisites

- .NET 8.0 SDK or later
- Robot Control System API running on `http://localhost:8000`

## Installation

1. Navigate to the project directory:
```bash
cd RobotControlClient
```

2. Restore NuGet packages:
```bash
dotnet restore
```

## Running the Application

```bash
dotnet run
```

## Usage

The application presents an interactive menu with the following options:

1. **System Status** - Get comprehensive robot and system status
2. **Emergency Stop** - Activate emergency stop
3. **Clear Emergency Stop** - Clear emergency stop state
4. **Home Robot (Full)** - Perform full homing calibration (2-3 minutes)
5. **Home Robot (Quick)** - Perform quick homing reset (30 seconds)
6. **Move to Position** - Direct synchronous movement to X/Y/Z position
7. **Queue Movement** - Queue asynchronous movement task
8. **Enable/Disable Worker** - Control background task processing
9. **Get All Tasks** - View all queued and completed tasks
10. **Move Box** - Move box between shelves (by ID or coordinates)
11. **Manage Shelves** - Add, list, find, or delete shelf locations
12. **Enable/Disable Vacuum** - Control vacuum gripper
13. **Smart Task** - Create task from barcode scan
14. **Cleanup Stuck Tasks** - Remove old stuck tasks

## WebSocket Events

The application automatically connects to the WebSocket endpoint and displays real-time updates:
- Position updates
- Task progress
- Task completion/failure
- System errors
- Connection status

## Configuration

To connect to a different server, modify the base URLs in `Program.cs`:

```csharp
var apiClient = new RobotApiClient("http://your-server:8000");
var wsClient = new RobotWebSocketClient("ws://your-server:8000");
```

## Architecture

- **Models** - Data models for API requests/responses
- **Services/RobotApiClient.cs** - HTTP client for REST API
- **Services/RobotWebSocketClient.cs** - WebSocket client for real-time updates
- **Program.cs** - Main application with interactive menu

## Error Handling

The application includes error handling for:
- Connection failures
- Invalid input
- API errors
- WebSocket disconnections

## Example Workflow

1. Start the application
2. The system will automatically connect to WebSocket
3. Select option 4 to home the robot (first time setup)
4. Select option 8 to enable the worker
5. Use option 11 to move boxes between shelves
6. Monitor real-time updates in the console

## Dependencies

- Newtonsoft.Json (13.0.3) - JSON serialization
- System.Net.WebSockets.Client (4.3.2) - WebSocket support