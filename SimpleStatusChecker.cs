using System;
using System.Threading.Tasks;
using RobotControlClient.Services;

namespace RobotControlClient
{
    public class SimpleStatusChecker
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== Robot/SmartPack Connection Status Checker ===\n");

            var apiClient = new RobotApiClient("http://localhost:8000");
            var wsClient = new RobotWebSocketClient("ws://localhost:8000");

            // Test REST API Connection
            Console.WriteLine("Testing REST API Connection...");
            try
            {
                // Simple health check
                var health = await apiClient.HealthCheck();
                Console.WriteLine("✅ REST API Connected");
                Console.WriteLine($"   Health Response: {health}\n");

                // Get system status
                Console.WriteLine("Getting System Status...");
                var status = await apiClient.GetSystemStatus(
                    includeSystemStats: false,  // Faster response
                    includeCamera: false,
                    quickCpu: true
                );

                Console.WriteLine("📊 System Status:");
                Console.WriteLine($"   Hardware Initialized: {status.HardwareInitialized}");
                Console.WriteLine($"   Robot Homed: {status.Homed}");
                Console.WriteLine($"   Emergency Stop: {status.EmergencyStopped}");
                
                if (status.Position != null)
                {
                    Console.WriteLine($"   Position: X={status.Position.XMm ?? status.Position.X:F1}mm, " +
                                    $"Y={status.Position.YMm ?? status.Position.Y:F1}mm, " +
                                    $"Z={status.Position.ZMm ?? status.Position.Z:F1}mm");
                }

                if (status.Worker != null)
                {
                    Console.WriteLine($"   Worker Enabled: {status.Worker.Enabled}");
                    Console.WriteLine($"   Queue Size: {status.Worker.QueueSize}");
                }

                if (status.Gripper != null)
                {
                    Console.WriteLine($"   Vacuum Enabled: {status.Gripper.VacuumEnabled}");
                    Console.WriteLine($"   Holding Item: {status.Gripper.HoldingItem}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ REST API Connection Failed: {ex.Message}");
            }

            // Test WebSocket Connection
            Console.WriteLine("\n\nTesting WebSocket Connection...");
            try
            {
                // Set up simple event handlers
                bool wsConnected = false;
                wsClient.OnConnected += (s, e) => 
                {
                    wsConnected = true;
                    Console.WriteLine("✅ WebSocket Connected");
                };

                wsClient.OnStatusUpdate += (s, msg) =>
                {
                    Console.WriteLine($"📡 Live Status Update - Position: X={msg.Position?.X:F1}, Y={msg.Position?.Y:F1}, Z={msg.Position?.Z:F1}");
                };

                wsClient.OnDisconnected += (s, e) =>
                {
                    Console.WriteLine("⚠️  WebSocket Disconnected");
                };

                // Connect
                await wsClient.ConnectAsync();
                
                // Wait a moment for initial status
                await Task.Delay(2000);

                if (wsConnected)
                {
                    Console.WriteLine("   WebSocket is receiving real-time updates");
                }

                // Keep running for 5 seconds to show some updates
                Console.WriteLine("\n📊 Monitoring for 5 seconds...");
                await Task.Delay(5000);

                // Disconnect
                await wsClient.DisconnectAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ WebSocket Connection Failed: {ex.Message}");
            }

            // Summary
            Console.WriteLine("\n=== Connection Test Complete ===");
            Console.WriteLine("Both REST API and WebSocket connections have been tested.");
            
            // Cleanup
            apiClient.Dispose();
            wsClient.Dispose();
        }
    }
}