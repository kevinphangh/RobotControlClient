using System;
using System.Linq;
using System.Threading.Tasks;
using RobotControlClient.Services;

namespace RobotControlClient
{
    public class SimpleStatusChecker
    {
        public static async Task RunStatusCheck()
        {
            Console.WriteLine("=== Robot/SmartPack Connection Status Checker ===\n");

            // Initialize clients
            var robotClient = new RobotApiClient("http://localhost:8000");
            var smartPackClient = new SmartPackApiClient("https://kangaroo.smartpack.dk");
            var wsClient = new RobotWebSocketClient("ws://localhost:8000");

            // Test ROBOT API Connection
            Console.WriteLine("[1/3] Testing ROBOT API Connection (localhost:8000)...");
            try
            {
                // Simple health check
                var health = await robotClient.HealthCheck();
                Console.WriteLine("✅ Robot API Connected");
                Console.WriteLine($"   Health Response: {health}\n");

                // Get system status
                Console.WriteLine("Getting Robot System Status...");
                var status = await robotClient.GetSystemStatus(
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
                Console.WriteLine($"❌ Robot API Connection Failed: {ex.Message}");
            }

            // Test SMARTPACK API Connection
            Console.WriteLine("\n[2/3] Testing SMARTPACK API Connection (kangaroo.smartpack.dk)...");
            try
            {
                // Test connection
                var connected = await smartPackClient.TestConnection();
                if (connected)
                {
                    Console.WriteLine("✅ SmartPack API Connected");

                    // Get active transfers
                    Console.WriteLine("\n   Getting Active Transfers...");
                    var activeTransfers = await smartPackClient.GetActiveTransfers();
                    if (activeTransfers != null)
                    {
                        var data = activeTransfers["data"];
                        if (data != null)
                        {
                            Console.WriteLine($"   Active Transfers Count: {data.Count()}");
                        }
                    }

                    // Get transfer history (last 5)
                    Console.WriteLine("\n   Getting Transfer History (last 5)...");
                    var history = await smartPackClient.GetTransferHistory(take: 5);
                    if (history != null)
                    {
                        var totalCount = history["totalCount"];
                        if (totalCount != null)
                        {
                            Console.WriteLine($"   Total Transfer History Records: {totalCount}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("❌ SmartPack API Connection Failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SmartPack API Connection Failed: {ex.Message}");
            }

            // Test WebSocket Connection
            Console.WriteLine("\n[3/3] Testing WebSocket Connection (ws://localhost:8000)...");
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
            robotClient.Dispose();
            smartPackClient.Dispose();
            wsClient.Dispose();
        }
    }
}