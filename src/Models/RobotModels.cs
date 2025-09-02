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

        [JsonProperty("uptime")]
        public string? Uptime { get; set; }
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

        [JsonProperty("worker")]
        public WorkerStatus? Worker { get; set; }

        [JsonProperty("system_stats")]
        public SystemStats? SystemStats { get; set; }
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

        [JsonProperty("error")]
        public string? Error { get; set; }
    }

    public class ShelfLocation
    {
        [JsonProperty("shelf_id")]
        public string? ShelfId { get; set; }

        [JsonProperty("x_position")]
        public double XPosition { get; set; }

        [JsonProperty("y_position")]
        public double YPosition { get; set; }

        [JsonProperty("is_occupied")]
        public bool IsOccupied { get; set; }

        [JsonProperty("occupied_by_barcode")]
        public string? OccupiedByBarcode { get; set; }
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

        [JsonProperty("vacuum_enabled")]
        public bool? VacuumEnabled { get; set; }

        [JsonProperty("timestamp")]
        public long? Timestamp { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonProperty("error")]
        public string? Error { get; set; }

        [JsonProperty("severity")]
        public string? Severity { get; set; }
    }
}