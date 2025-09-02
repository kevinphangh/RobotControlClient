using Newtonsoft.Json;
using System;

namespace RobotControlClient.Models
{
    public class Position
    {
        [JsonProperty("x")]
        public double? X { get; set; }

        [JsonProperty("y")]
        public double? Y { get; set; }

        [JsonProperty("z")]
        public double? Z { get; set; }

        [JsonProperty("x_mm")]
        public double? XMm { get; set; }

        [JsonProperty("y_mm")]
        public double? YMm { get; set; }

        [JsonProperty("z_mm")]
        public double? ZMm { get; set; }
    }

    public class GripperStatus
    {
        [JsonProperty("vacuum_enabled")]
        public bool VacuumEnabled { get; set; }

        [JsonProperty("holding_item")]
        public bool HoldingItem { get; set; }

        [JsonProperty("held_barcode")]
        public string? HeldBarcode { get; set; }

        [JsonProperty("is_homed")]
        public bool IsHomed { get; set; }

        [JsonProperty("bottom_angle")]
        public double BottomAngle { get; set; }

        [JsonProperty("top_angle")]
        public double TopAngle { get; set; }
    }

    public class WorkerStatus
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("processing")]
        public bool Processing { get; set; }

        [JsonProperty("current_task")]
        public string? CurrentTask { get; set; }

        [JsonProperty("queue_size")]
        public int QueueSize { get; set; }
    }

    public class SystemStats
    {
        [JsonProperty("cpu_percent")]
        public double CpuPercent { get; set; }

        [JsonProperty("memory_percent")]
        public double MemoryPercent { get; set; }

        [JsonProperty("disk_usage_percent")]
        public double DiskUsagePercent { get; set; }

        [JsonProperty("temperature")]
        public double Temperature { get; set; }
    }

    public class WorkspaceBounds
    {
        [JsonProperty("x_min")]
        public double XMin { get; set; }

        [JsonProperty("x_max")]
        public double XMax { get; set; }

        [JsonProperty("y_min")]
        public double YMin { get; set; }

        [JsonProperty("y_max")]
        public double YMax { get; set; }
    }

    public class RobotStatus
    {
        [JsonProperty("hardware_initialized")]
        public bool HardwareInitialized { get; set; }

        [JsonProperty("homed")]
        public bool Homed { get; set; }

        [JsonProperty("emergency_stopped")]
        public bool EmergencyStopped { get; set; }

        [JsonProperty("position")]
        public Position? Position { get; set; }

        [JsonProperty("gripper")]
        public GripperStatus? Gripper { get; set; }

        [JsonProperty("workspace")]
        public WorkspaceBounds? Workspace { get; set; }

        [JsonProperty("worker")]
        public WorkerStatus? Worker { get; set; }

        [JsonProperty("system")]
        public SystemStats? System { get; set; }
    }

    public class ApiResponse
    {
        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonProperty("task_id")]
        public string? TaskId { get; set; }

        [JsonProperty("details")]
        public object? Details { get; set; }
    }

    public class MoveBoxRequest
    {
        [JsonProperty("source_shelf_id")]
        public string? SourceShelfId { get; set; }

        [JsonProperty("destination_shelf_id")]
        public string? DestinationShelfId { get; set; }

        [JsonProperty("source_x_mm")]
        public double? SourceXMm { get; set; }

        [JsonProperty("source_y_mm")]
        public double? SourceYMm { get; set; }

        [JsonProperty("destination_x_mm")]
        public double? DestinationXMm { get; set; }

        [JsonProperty("destination_y_mm")]
        public double? DestinationYMm { get; set; }

        [JsonProperty("barcode")]
        public string? Barcode { get; set; }
    }

    public class QueueMoveRequest
    {
        [JsonProperty("x_mm")]
        public double XMm { get; set; }

        [JsonProperty("y_mm")]
        public double YMm { get; set; }

        [JsonProperty("rpm_x")]
        public int? RpmX { get; set; }

        [JsonProperty("rpm_y")]
        public int? RpmY { get; set; }
    }

    public class ShelfLocation
    {
        [JsonProperty("shelf_id")]
        public string? ShelfId { get; set; }

        [JsonProperty("shelf_barcode")]
        public string? ShelfBarcode { get; set; }

        [JsonProperty("x_position")]
        public double XPosition { get; set; }

        [JsonProperty("y_position")]
        public double YPosition { get; set; }

        [JsonProperty("is_occupied")]
        public bool IsOccupied { get; set; }

        [JsonProperty("occupied_by_barcode")]
        public string? OccupiedByBarcode { get; set; }

        [JsonProperty("is_human_pickup")]
        public bool IsHumanPickup { get; set; }
    }

    public class TaskInfo
    {
        [JsonProperty("task_id")]
        public string? TaskId { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("created_at")]
        public long? CreatedAt { get; set; }

        [JsonProperty("started_at")]
        public long? StartedAt { get; set; }

        [JsonProperty("completed_at")]
        public long? CompletedAt { get; set; }

        [JsonProperty("error")]
        public string? Error { get; set; }

        [JsonProperty("position")]
        public int? Position { get; set; }
    }

    public class WebSocketMessage
    {
        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("hardware_initialized")]
        public bool? HardwareInitialized { get; set; }

        [JsonProperty("homed")]
        public bool? Homed { get; set; }

        [JsonProperty("emergency_stopped")]
        public bool? EmergencyStopped { get; set; }

        [JsonProperty("worker_enabled")]
        public bool? WorkerEnabled { get; set; }

        [JsonProperty("position")]
        public Position? Position { get; set; }

        [JsonProperty("workspace_bounds")]
        public WorkspaceBounds? WorkspaceBounds { get; set; }

        [JsonProperty("vacuum_enabled")]
        public bool? VacuumEnabled { get; set; }

        [JsonProperty("timestamp")]
        public long? Timestamp { get; set; }

        [JsonProperty("task_id")]
        public string? TaskId { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("progress")]
        public int? Progress { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonProperty("result")]
        public string? Result { get; set; }

        [JsonProperty("error")]
        public string? Error { get; set; }

        [JsonProperty("details")]
        public string? Details { get; set; }

        [JsonProperty("severity")]
        public string? Severity { get; set; }

        [JsonProperty("code")]
        public string? Code { get; set; }
    }
}