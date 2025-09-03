# 📡 Robot Control System API Documentation

## Table of Contents
- [Overview](#overview)
- [Base Configuration](#base-configuration)
- [Authentication](#authentication)
- [Request/Response Format](#requestresponse-format)
- [Error Handling](#error-handling)
- [API Endpoints](#api-endpoints)
  - [System Endpoints](#system-endpoints)
  - [Motion Endpoints](#motion-endpoints)
  - [Operations Endpoints](#operations-endpoints)
  - [Queue Management](#queue-management)
  - [Inventory Management](#inventory-management)
  - [Gripper Control](#gripper-control)
  - [General Endpoints](#general-endpoints)
- [Data Models](#data-models)
- [Error Codes](#error-codes)
- [Example Workflows](#example-workflows)

## Overview

The Robot Control System API provides comprehensive control over a 3-axis warehouse robot designed for automated box movement operations. The robot features:
- X/Y/Z axis movement with dual-motor gantry system
- Vacuum gripper for box handling
- Barcode scanning capabilities
- Task queue management
- Status monitoring via REST API

## Base Configuration

### Base URL
```
http://localhost:8000
```

### API Version
Current version: `v1`

### Headers
All requests should include:
```http
Content-Type: application/json
Accept: application/json
```

## Authentication

Currently no authentication is required (internal network use only). Future versions may implement API key or OAuth2 authentication.

## Request/Response Format

### Standard Response Structure
```json
{
  "status": "success|error",
  "message": "Human-readable description",
  "task_id": "uuid-string (optional)",
  "details": {
    // Additional response-specific data
  }
}
```

### Timestamp Format
All timestamps use Unix epoch time (seconds since 1970-01-01 00:00:00 UTC)

## Error Handling

### HTTP Status Codes
- `200 OK` - Request successful
- `202 Accepted` - Task queued for asynchronous processing
- `400 Bad Request` - Invalid request parameters
- `404 Not Found` - Resource not found
- `409 Conflict` - Operation conflicts with current state
- `422 Unprocessable Entity` - Validation error
- `500 Internal Server Error` - Server error

### Error Response Format
```json
{
  "detail": "Error description",
  "status_code": 400,
  "request_id": "unique-request-id"
}
```

## API Endpoints

### System Endpoints

#### 🚨 Emergency Stop
`PUT /robot/system/e_stop`

Activate or clear emergency stop state. When activated, immediately stops all robot motion and disables vacuum.

**Query Parameters:**
- `enabled` (boolean, required) - `true` to activate, `false` to clear

**Example Request:**
```bash
# Activate emergency stop
curl -X PUT "http://localhost:8000/robot/system/e_stop?enabled=true"

# Clear emergency stop
curl -X PUT "http://localhost:8000/robot/system/e_stop?enabled=false"
```

**Example Response:**
```json
{
  "status": "success",
  "message": "Emergency stop activated"
}
```

**Notes:**
- Hardware E-stop (GPIO Pin 7) cannot be overridden via API
- Requires homing after emergency stop is cleared

---

#### ⚙️ Worker Control
`PUT /robot/system/worker`

Enable or disable the background task worker that processes queued operations.

**Query Parameters:**
- `enabled` (boolean, required) - `true` to enable, `false` to disable

**Example Request:**
```bash
# Enable worker
curl -X PUT "http://localhost:8000/robot/system/worker?enabled=true"

# Disable worker
curl -X PUT "http://localhost:8000/robot/system/worker?enabled=false"
```

**Example Response:**
```json
{
  "status": "success",
  "message": "Worker enabled",
  "details": {
    "worker_enabled": true,
    "queue_size": 3
  }
}
```

**Error Cases:**
- `409 Conflict` - Robot not homed (unless first task is homing)

---

#### 📊 System Status
`GET /robot/system/status`

Get comprehensive robot and system status information.

**Query Parameters:**
- `include_worker` (boolean, default=true) - Include worker status
- `include_gripper` (boolean, default=true) - Include gripper status
- `include_motion` (boolean, default=true) - Include motion status
- `include_system_stats` (boolean, default=true) - Include CPU/memory stats
- `include_workspace` (boolean, default=true) - Include workspace bounds
- `include_camera` (boolean, default=true) - Include camera status
- `quick_cpu` (boolean, default=true) - Use cached CPU stats for faster response

**Example Request:**
```bash
# Get full status
curl -X GET "http://localhost:8000/robot/system/status"

# Get minimal status (faster)
curl -X GET "http://localhost:8000/robot/system/status?include_system_stats=false&include_camera=false"
```

**Example Response:**
```json
{
  "hardware_initialized": true,
  "homed": true,
  "emergency_stopped": false,
  "position": {
    "x_mm": 500.5,
    "y_mm": 300.2,
    "z_mm": 100.0
  },
  "gripper": {
    "vacuum_enabled": false,
    "holding_item": false,
    "held_barcode": null,
    "is_homed": true,
    "bottom_angle": 90.0,
    "top_angle": 90.0
  },
  "workspace": {
    "x_min": 0,
    "x_max": 1337.0,
    "y_min": 0,
    "y_max": 1184.4
  },
  "worker": {
    "enabled": true,
    "processing": false,
    "current_task": null,
    "queue_size": 0
  },
  "system": {
    "cpu_percent": 15.5,
    "memory_percent": 45.2,
    "disk_usage_percent": 60.1,
    "temperature": 45.5
  },
  "camera": {
    "connected": true,
    "resolution": "1920x1080"
  }
}
```

### Motion Endpoints

#### 🏠 Homing
`POST /robot/motion/home`

Execute homing sequences to establish robot position reference.

**Query Parameters:**
- `mode` (string, default="big") - Homing mode:
  - `"big"` - Full calibration with all axes (2-3 minutes)
  - `"small"` - Quick position reset (30 seconds)
  - `"position"` - Move to (0,0) without homing (10 seconds)

**Example Request:**
```bash
# Full homing calibration
curl -X POST "http://localhost:8000/robot/motion/home?mode=big"

# Quick homing
curl -X POST "http://localhost:8000/robot/motion/home?mode=small"

# Move to home position
curl -X POST "http://localhost:8000/robot/motion/home?mode=position"
```

**Example Response (for big/small modes):**
```json
{
  "task_id": "123e4567-e89b-12d3-a456-426614174000",
  "message": "Started full homing sequence with limit switch calibration",
  "estimated_duration": 180
}
```

**Homing Sequences:**

**Big Homing (Full Calibration):**
1. Z-axis finds MIN limit switch, sets Z=0, retracts to middle
2. Gripper servos find limits and calibrate
3. Y-axis finds MAX switch, measures workspace
4. X/A axes square at MIN, measure to MAX
5. Vacuum sensor baseline calibration

**Small Homing (Quick Reset):**
1. Z-axis reset to MIN, retract to middle
2. Gripper direct positioning
3. X/A axes square at MIN only
4. Y assumes gravity position
5. Quick vacuum check

---

#### 🎯 Direct Movement (Synchronous)
`POST /robot/motion/move_direct`

Move robot directly to specified position. This is a blocking call that waits for movement completion.

**Query Parameters:**
- `x` (float, optional) - X position in mm
- `y` (float, optional) - Y position in mm
- `z` (float, optional) - Z position in mm

**Example Request:**
```bash
# Move to specific X,Y position
curl -X POST "http://localhost:8000/robot/motion/move_direct?x=500&y=300"

# Move only X axis
curl -X POST "http://localhost:8000/robot/motion/move_direct?x=750"

# Move all three axes
curl -X POST "http://localhost:8000/robot/motion/move_direct?x=500&y=300&z=50"
```

**Example Response:**
```json
{
  "status": "success",
  "message": "Movement completed",
  "details": {
    "final_position": {
      "x": 500.0,
      "y": 300.0,
      "z": 50.0
    },
    "duration_ms": 2500
  }
}
```

**Error Cases:**
- `400 Bad Request` - Position outside workspace bounds
- `409 Conflict` - Robot not homed or emergency stopped

---

#### 📋 Queue Movement (Asynchronous)
`POST /robot/motion/queue_move`

Queue a movement task for asynchronous execution.

**Request Body:**
```json
{
  "x_mm": 500.0,
  "y_mm": 300.0,
  "rpm_x": 800,  // Optional, defaults from config
  "rpm_y": 800   // Optional, defaults from config
}
```

**Example Request:**
```bash
curl -X POST "http://localhost:8000/robot/motion/queue_move" \
  -H "Content-Type: application/json" \
  -d '{
    "x_mm": 500,
    "y_mm": 300
  }'
```

**Example Response:**
```json
{
  "status": "accepted",
  "task_id": "123e4567-e89b-12d3-a456-426614174000",
  "message": "Move to task queued successfully",
  "type": "move_to",
  "priority": 5
}
```

---

#### ↕️ Z-Axis Relative Movement
`POST /robot/motion/move_z_relative`

Move Z-axis relative to current position (synchronous).

**Request Body:**
```json
{
  "distance_mm": 50.0,      // Positive = forward (extend), Negative = backward (retract)
  "speed_mm_per_sec": 20.0  // Optional
}
```

**Example Request:**
```bash
# Extend gripper 50mm forward
curl -X POST "http://localhost:8000/robot/motion/move_z_relative" \
  -H "Content-Type: application/json" \
  -d '{"distance_mm": 50}'

# Retract gripper 30mm backward
curl -X POST "http://localhost:8000/robot/motion/move_z_relative" \
  -H "Content-Type: application/json" \
  -d '{"distance_mm": -30}'
```

**Note:** Z-axis coordinate system:
- Z=0 (MIN) = Fully extended at shelf (picking position)
- Z=MAX = Fully retracted away from shelf (safe position)

---

#### 🎮 Control Mode
`PUT /robot/motion/control_mode`

Set robot movement control mode for manual operation.

**Query Parameters:**
- `enabled` (boolean, required) - Enable/disable control mode

**Example Request:**
```bash
curl -X PUT "http://localhost:8000/robot/motion/control_mode?enabled=true"
```

### Operations Endpoints

#### 📦 Move Box
`POST /robot/operations/move_box`

High-level operation to move a box from source to destination shelf.

**Request Body:**
```json
{
  "source_shelf_id": "SHELF_A",        // OR use coordinates
  "destination_shelf_id": "SHELF_B",   // OR use coordinates
  "source_x_mm": 500.0,                // Alternative to shelf_id
  "source_y_mm": 300.0,
  "destination_x_mm": 800.0,
  "destination_y_mm": 400.0,
  "barcode": "BOX123"                  // Optional item barcode
}
```

**Example Request (using shelf IDs):**
```bash
curl -X POST "http://localhost:8000/robot/operations/move_box" \
  -H "Content-Type: application/json" \
  -d '{
    "source_shelf_id": "SHELF_A",
    "destination_shelf_id": "SHELF_B"
  }'
```

**Example Request (using coordinates):**
```bash
curl -X POST "http://localhost:8000/robot/operations/move_box" \
  -H "Content-Type: application/json" \
  -d '{
    "source_x_mm": 500,
    "source_y_mm": 300,
    "destination_x_mm": 800,
    "destination_y_mm": 400
  }'
```

**Example Request (auto-select empty destination):**
```bash
curl -X POST "http://localhost:8000/robot/operations/move_box" \
  -H "Content-Type: application/json" \
  -d '{
    "source_shelf_id": "SHELF_A",
    "destination_shelf_id": "AUTO"
  }'
```

**Example Response:**
```json
{
  "status": "accepted",
  "task_id": "123e4567-e89b-12d3-a456-426614174000",
  "message": "Move Box task queued successfully",
  "type": "move_box",
  "priority": 5
}
```

**Validation:**
- Must specify either shelf_id OR coordinates for both source and destination
- Validates shelves exist in inventory
- Checks destination shelf is empty

### Queue Management

#### 📋 Get All Tasks
`GET /robot/queue/tasks`

Retrieve all tasks from operation history, task manager, and SQL queue.

**Example Request:**
```bash
curl -X GET "http://localhost:8000/robot/queue/tasks"
```

**Example Response:**
```json
[
  {
    "task_id": "123e4567-e89b-12d3-a456-426614174000",
    "type": "move_box",
    "status": "in_progress",
    "description": "Move box from SHELF_A to SHELF_B",
    "created_at": 1700000000,
    "started_at": 1700000010,
    "completed_at": null,
    "error": null
  },
  {
    "task_id": "223e4567-e89b-12d3-a456-426614174001",
    "type": "homing",
    "status": "queued",
    "description": "Big homing calibration",
    "created_at": 1700000020,
    "position": 2
  }
]
```

---

#### ❌ Cancel Task
`DELETE /robot/queue/tasks/{task_id}`

Cancel or delete a queued task.

**Path Parameters:**
- `task_id` (string) - Task ID or frontend format (e.g., "queued-{task_id}-{position}")

**Example Request:**
```bash
# Cancel by task ID
curl -X DELETE "http://localhost:8000/robot/queue/tasks/123e4567-e89b-12d3-a456-426614174000"

# Cancel using frontend format
curl -X DELETE "http://localhost:8000/robot/queue/tasks/queued-123e4567-e89b-12d3-a456-426614174000-2"
```

**Example Response:**
```json
{
  "status": "success",
  "message": "Task cancelled"
}
```

---

#### 🤖 Smart Task Creation
`POST /robot/queue/tasks/smart_task`

Create task automatically from barcode scan (box barcode or shelf QR code).

**Request Body:**
```json
{
  "barcode": "BOX123"  // Or shelf QR code
}
```

**Example Request:**
```bash
curl -X POST "http://localhost:8000/robot/queue/tasks/smart_task" \
  -H "Content-Type: application/json" \
  -d '{"barcode": "BOX123"}'
```

**Example Response:**
```json
{
  "status": "accepted",
  "task_id": "123e4567-e89b-12d3-a456-426614174000",
  "message": "Pick And Place task queued successfully",
  "type": "pick_and_place",
  "priority": 5
}
```

**Smart Task Logic:**
- Box barcode: Moves box to appropriate shelf
- Shelf QR code: Navigates to shelf location
- Handles return-to-home for human pickup locations

---

#### 🧹 Clean Stuck Tasks
`POST /robot/queue/tasks/cleanup`

Clean up stuck tasks in operation history and/or task queue.

**Query Parameters:**
- `hours_old` (float, default=0.5) - Age threshold in hours
- `target` (string, default="both") - "history", "queue", or "both"

**Example Request:**
```bash
# Clean tasks older than 1 hour
curl -X POST "http://localhost:8000/robot/queue/tasks/cleanup?hours_old=1.0"

# Clean only operation history
curl -X POST "http://localhost:8000/robot/queue/tasks/cleanup?target=history"
```

**Example Response:**
```json
{
  "status": "success",
  "message": "Cleanup completed",
  "details": {
    "history_cleaned": 5,
    "queue_cleaned": 2
  }
}
```

### Inventory Management

#### ➕ Add Shelf Location
`POST /robot/inventory/shelves`

Add a new shelf location to the inventory system.

**Request Body:**
```json
{
  "x_position": 500.0,
  "y_position": 300.0,
  "shelf_barcode": "QR_SHELF_A",     // Optional QR code
  "is_occupied": false,
  "occupied_by_barcode": null,
  "is_human_pickup": false            // Mark as human pickup location
}
```

**Example Request:**
```bash
curl -X POST "http://localhost:8000/robot/inventory/shelves" \
  -H "Content-Type: application/json" \
  -d '{
    "x_position": 500,
    "y_position": 300,
    "shelf_barcode": "QR_SHELF_A"
  }'
```

**Example Response:**
```json
{
  "status": "success",
  "message": "Shelf added",
  "shelf_id": "SHELF_A"
}
```

---

#### 🔍 Query Shelves
`GET /robot/inventory/shelves`

Unified endpoint for all shelf queries. Use ONE of the following query parameter combinations:

**Query Parameters (mutually exclusive):**
- `shelf_id` (string) - Get specific shelf by ID
- `nearest_x` & `nearest_y` (float) - Find nearest shelf to coordinates
- `max_distance` (float, optional) - Maximum distance for nearest search
- `item_barcode` (string) - Find shelf containing specific item
- `shelf_barcode` (string) - Find shelf by QR code
- No parameters - Get all shelves

**Example Requests:**
```bash
# Get all shelves
curl -X GET "http://localhost:8000/robot/inventory/shelves"

# Get specific shelf
curl -X GET "http://localhost:8000/robot/inventory/shelves?shelf_id=SHELF_A"

# Find nearest shelf to position
curl -X GET "http://localhost:8000/robot/inventory/shelves?nearest_x=500&nearest_y=300"

# Find nearest shelf within 100mm
curl -X GET "http://localhost:8000/robot/inventory/shelves?nearest_x=500&nearest_y=300&max_distance=100"

# Find shelf containing item
curl -X GET "http://localhost:8000/robot/inventory/shelves?item_barcode=BOX123"

# Find shelf by QR code
curl -X GET "http://localhost:8000/robot/inventory/shelves?shelf_barcode=QR_SHELF_A"
```

**Example Response (single shelf):**
```json
{
  "shelf_id": "SHELF_A",
  "x_position": 500.0,
  "y_position": 300.0,
  "shelf_barcode": "QR_SHELF_A",
  "is_occupied": true,
  "occupied_by_barcode": "BOX123",
  "is_human_pickup": false
}
```

**Example Response (all shelves):**
```json
[
  {
    "shelf_id": "SHELF_A",
    "x_position": 500.0,
    "y_position": 300.0,
    "is_occupied": true,
    "occupied_by_barcode": "BOX123"
  },
  {
    "shelf_id": "SHELF_B",
    "x_position": 800.0,
    "y_position": 400.0,
    "is_occupied": false
  }
]
```

---

#### ✏️ Update Shelf
`PUT /robot/inventory/shelves/{shelf_id}`

Update an existing shelf location.

**Path Parameters:**
- `shelf_id` (string) - Shelf identifier

**Request Body:**
```json
{
  "x_position": 510.0,
  "y_position": 310.0,
  "is_occupied": true,
  "occupied_by_barcode": "BOX456"
}
```

**Example Request:**
```bash
curl -X PUT "http://localhost:8000/robot/inventory/shelves/SHELF_A" \
  -H "Content-Type: application/json" \
  -d '{
    "is_occupied": true,
    "occupied_by_barcode": "BOX456"
  }'
```

---

#### ❌ Delete Shelf
`DELETE /robot/inventory/shelves/{shelf_id}`

Remove a shelf from the inventory system.

**Path Parameters:**
- `shelf_id` (string) - Shelf identifier

**Example Request:**
```bash
curl -X DELETE "http://localhost:8000/robot/inventory/shelves/SHELF_A"
```

---

#### ➕➕ Batch Add Shelves
`POST /robot/inventory/shelves/batch`

Add multiple shelf locations in a single request.

**Request Body:**
```json
[
  {
    "x_position": 500.0,
    "y_position": 300.0,
    "shelf_barcode": "QR_SHELF_A"
  },
  {
    "x_position": 800.0,
    "y_position": 400.0,
    "shelf_barcode": "QR_SHELF_B"
  }
]
```

**Example Request:**
```bash
curl -X POST "http://localhost:8000/robot/inventory/shelves/batch" \
  -H "Content-Type: application/json" \
  -d '[
    {"x_position": 500, "y_position": 300},
    {"x_position": 800, "y_position": 400}
  ]'
```

**Example Response:**
```json
{
  "status": "success",
  "message": "Batch add completed",
  "details": {
    "added": 2,
    "failed": 0,
    "shelf_ids": ["SHELF_A", "SHELF_B"]
  }
}
```

### Gripper Control

#### 🔌 Vacuum Control
`PUT /robot/gripper/vacuum`

Enable or disable the vacuum gripper.

**Query Parameters:**
- `enabled` (boolean, required) - True to enable, false to disable

**Example Request:**
```bash
# Enable vacuum
curl -X PUT "http://localhost:8000/robot/gripper/vacuum?enabled=true"

# Disable vacuum
curl -X PUT "http://localhost:8000/robot/gripper/vacuum?enabled=false"
```

**Example Response:**
```json
{
  "status": "success",
  "message": "Vacuum enabled",
  "details": {
    "vacuum_enabled": true,
    "pressure": 850
  }
}
```

---

#### 🤏 Gripper Positioning
`POST /robot/gripper/position`

Control gripper servo motors for precise positioning.

**Request Body:**
```json
{
  "bottom_duration": 2.0,      // Rotation duration in seconds (0.1-10.0)
  "bottom_clockwise": true,     // Rotation direction
  "top_duration": 2.0,          // Rotation duration in seconds (0.1-10.0)
  "top_clockwise": false,       // Rotation direction
  "speed": 0.5                  // Speed factor (0.1-1.0, optional)
}
```

**Example Request:**
```bash
curl -X POST "http://localhost:8000/robot/gripper/position" \
  -H "Content-Type: application/json" \
  -d '{
    "bottom_duration": 1.5,
    "bottom_clockwise": true,
    "top_duration": 1.5,
    "top_clockwise": false
  }'
```

### General Endpoints

#### 🏠 Root
`GET /`

Basic API information endpoint.

**Example Request:**
```bash
curl -X GET "http://localhost:8000/"
```

**Example Response:**
```json
{
  "message": "Robot Control System with Integrated Worker (v1.0.0) is running."
}
```

---

#### 💚 Health Check
`GET /health`

Health check endpoint for monitoring and load balancers.

**Example Request:**
```bash
curl -X GET "http://localhost:8000/health"
```

**Example Response:**
```json
{
  "status": "healthy",
  "hardware_initialized": true,
  "homed": true
}
```

## Data Models

### Request Models

#### BaseTaskParams
Base model for all task-related requests.
```typescript
{
  task_id?: string  // Auto-generated UUID if not provided
}
```

#### MoveBoxParams
Parameters for box movement operations.
```typescript
{
  source_x_mm?: number         // >= 0
  source_y_mm?: number         // >= 0
  source_shelf_id?: string
  destination_x_mm?: number    // >= 0
  destination_y_mm?: number    // >= 0
  destination_shelf_id?: string
  barcode?: string
  task_id?: string
}
```
**Validation:** Must specify either coordinates OR shelf_id for both source and destination.

#### SmartTaskParams
Parameters for smart task creation from barcode.
```typescript
{
  barcode: string  // 3-500 characters, required
  task_id?: string
}
```

#### MoveToParams
Parameters for movement tasks.
```typescript
{
  x_mm: number      // >= 0, required
  y_mm: number      // >= 0, required
  rpm_x?: number    // > 0, defaults from config
  rpm_y?: number    // > 0, defaults from config
  task_id?: string
}
```

#### HomeParams
Parameters for homing operations.
```typescript
{
  mode: "big" | "small"  // Default: "big"
  task_id?: string
}
```

#### MoveZRelativeParams
Parameters for relative Z-axis movement.
```typescript
{
  distance_mm: number           // Required
  speed_mm_per_sec?: number     // > 0, optional
}
```

#### GripperPositionRequest
Parameters for gripper servo control.
```typescript
{
  bottom_duration: number       // 0.1-10.0 seconds
  bottom_clockwise: boolean     // Default: true
  top_duration: number          // 0.1-10.0 seconds
  top_clockwise: boolean        // Default: true
  speed?: number                // 0.1-1.0, optional
}
```

#### ShelfLocation
Shelf inventory model.
```typescript
{
  shelf_id?: string             // Auto-generated if not provided
  shelf_barcode?: string        // QR code on shelf
  x_position: number            // >= 0, required
  y_position: number            // >= 0, required
  is_occupied?: boolean         // Default: false
  occupied_by_barcode?: string  // Item barcode if occupied
  is_human_pickup?: boolean     // Default: false
}
```

### Response Models

#### StatusResponse
Standard response for status operations.
```typescript
{
  status: string
  message: string
  task_id?: string
  details?: object
}
```

#### GripperStatus
Gripper state information.
```typescript
{
  is_homed: boolean
  bottom_angle: number
  top_angle: number
  bottom_home: number
  top_home: number
  vacuum_enabled: boolean
}
```

## Error Codes

### System Errors (1xxx)
- `1001` - Hardware not initialized
- `1002` - Robot not homed
- `1003` - Emergency stop active
- `1004` - Motion mutex locked
- `1005` - Worker not enabled

### Movement Errors (2xxx)
- `2001` - Position out of bounds
- `2002` - Invalid movement parameters
- `2003` - Movement timeout
- `2004` - Collision detected
- `2005` - Motor error

### Gripper Errors (3xxx)
- `3001` - Vacuum pressure lost
- `3002` - Gripper not homed
- `3003` - Gripper servo error
- `3004` - Item detection failed

### Task Queue Errors (4xxx)
- `4001` - Task not found
- `4002` - Task already processing
- `4003` - Queue full
- `4004` - Invalid task parameters

### Inventory Errors (5xxx)
- `5001` - Shelf not found
- `5002` - Shelf already occupied
- `5003` - Invalid shelf location
- `5004` - Duplicate shelf ID

## Example Workflows

### Complete Box Movement Workflow
```bash
# 1. Initialize system (first time only)
curl -X POST "http://localhost:8000/robot/motion/home?mode=big"

# 2. Enable worker
curl -X PUT "http://localhost:8000/robot/system/worker?enabled=true"

# 3. Add shelf locations
curl -X POST "http://localhost:8000/robot/inventory/shelves" \
  -H "Content-Type: application/json" \
  -d '{"x_position": 500, "y_position": 300, "shelf_barcode": "SHELF_A"}'

curl -X POST "http://localhost:8000/robot/inventory/shelves" \
  -H "Content-Type: application/json" \
  -d '{"x_position": 800, "y_position": 400, "shelf_barcode": "SHELF_B"}'

# 4. Move box from shelf A to shelf B
curl -X POST "http://localhost:8000/robot/operations/move_box" \
  -H "Content-Type: application/json" \
  -d '{
    "source_shelf_id": "SHELF_A",
    "destination_shelf_id": "SHELF_B"
  }'

# 5. Monitor task status
curl -X GET "http://localhost:8000/robot/queue/tasks"

# 6. Get system status
curl -X GET "http://localhost:8000/robot/system/status"
```

### Emergency Recovery Workflow
```bash
# 1. Activate emergency stop
curl -X PUT "http://localhost:8000/robot/system/e_stop?enabled=true"

# 2. Clear emergency stop
curl -X PUT "http://localhost:8000/robot/system/e_stop?enabled=false"

# 3. Perform quick homing
curl -X POST "http://localhost:8000/robot/motion/home?mode=small"

# 4. Re-enable worker
curl -X PUT "http://localhost:8000/robot/system/worker?enabled=true"

# 5. Clean up stuck tasks
curl -X POST "http://localhost:8000/robot/queue/tasks/cleanup"
```

### Smart Task with Barcode Workflow
```bash
# 1. Scan barcode and create smart task
curl -X POST "http://localhost:8000/robot/queue/tasks/smart_task" \
  -H "Content-Type: application/json" \
  -d '{"barcode": "BOX123"}'

# 2. System automatically:
#    - Identifies box location
#    - Finds appropriate destination
#    - Creates movement task
#    - Executes pick and place operation

# 3. Monitor task status
curl -X GET "http://localhost:8000/robot/queue/tasks"
```

### Development/Testing Workflow
```bash
# 1. Start with hardware disabled (set environment variable)
export HARDWARE_DISABLED=True
export DEV_SKIP_HOMING=True

# 2. Start the server
python main.py

# 3. Test API endpoints without hardware
curl -X GET "http://localhost:8000/robot/system/status"

# 4. Simulate movements
curl -X POST "http://localhost:8000/robot/motion/queue_move" \
  -H "Content-Type: application/json" \
  -d '{"x_mm": 500, "y_mm": 300}'
```

## Rate Limits and Performance

### Recommended Limits
- Maximum concurrent API requests: 100
- Task queue maximum size: 1000
- Emergency stop response time: < 100ms

### Performance Characteristics
- Homing (big mode): 2-3 minutes
- Homing (small mode): 30 seconds
- Average box movement: 15-30 seconds
- Position query response: < 200ms
- Status query response: < 100ms (with quick_cpu=true)

## Security Considerations

### Current Implementation
- No authentication (internal network only)
- CORS configured for specific origins
- Request ID tracking for debugging
- Input validation on all endpoints

### Recommended Security Enhancements
1. Implement API key authentication
2. Add rate limiting per client
3. Use HTTPS in production
4. Implement request signing for critical operations
5. Add audit logging for all operations
6. Restrict network access via firewall rules

## Versioning

API version is included in the response headers:
```
X-API-Version: v1.0.0
```

Breaking changes will increment the major version number. New endpoints or optional parameters are considered non-breaking.

## Support and Contact

For issues, bug reports, or feature requests:
- GitHub Issues: [Project Repository]
- Email: support@robotcontrol.example
- Documentation: This document

## Appendix: Z-Axis Coordinate System

**CRITICAL:** The Z-axis coordinate system:

```
Z = 0 (MIN) → EXTENDED at shelf (picking position)
Z = MAX → RETRACTED away from shelf (safe position)

DECREASING Z = Moving TOWARD shelf (extending)
INCREASING Z = Moving AWAY from shelf (retracting)
```

### Key Z Positions
| Position | Value | Purpose |
|----------|-------|---------|
| Z=0 | MIN limit | Fully extended at shelf |
| Z=10 | Z_PICK_POSITION_MM | Pick height |
| Z=20 | Z_PICK_POSITION_MM + Z_PLACE_OFFSET_MM | Place height |
| Z=100 | Z_MIDDLE_MM | Safe travel height |
| Z=550 | Z_MAX_MM | Maximum retraction |

Always ensure Z-axis is at safe height (Z_MIDDLE_MM) before lateral movements to prevent collisions.