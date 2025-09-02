using System;
using System.Threading.Tasks;
using RobotControlClient.Models;
using RobotControlClient.Services;

namespace RobotControlClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Check if SimpleStatusChecker mode is requested
            if (args.Length > 0 && args[0] == "SimpleStatusChecker.cs")
            {
                await SimpleStatusChecker.RunStatusCheck();
                return;
            }

            Console.WriteLine("=== Robot Control Client - Monitoring Mode ===\n");

            // Initialize clients
            var apiClient = new RobotApiClient("http://localhost:8000");
            var wsClient = new RobotWebSocketClient("ws://localhost:8000");
            var smartPackClient = new SmartPackApiClient("https://kangaroo.smartpack.dk");

            // Setup WebSocket event handlers
            SetupWebSocketEventHandlers(wsClient);

            try
            {
                // Connect to WebSocket for real-time updates
                Console.WriteLine("Connecting to WebSocket...");
                await wsClient.ConnectAsync();
                await Task.Delay(1000); // Give it a moment to establish connection

                // Main menu loop
                bool exit = false;
                while (!exit)
                {
                    Console.WriteLine("\n--- Main Menu ---");
                    Console.WriteLine("1. Robot System Status");
                    Console.WriteLine("2. SmartPack System Status");
                    Console.WriteLine("3. Full Connection Test");
                    Console.WriteLine("4. Emergency Stop");
                    Console.WriteLine("5. Clear Emergency Stop");
                    Console.WriteLine("6. Home Robot");
                    Console.WriteLine("7. Enable Worker");
                    Console.WriteLine("8. Disable Worker");
                    Console.WriteLine("0. Exit");
                    Console.Write("\nSelect option: ");

                    var choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            await ShowRobotStatus(apiClient);
                            break;
                        case "2":
                            await ShowSmartPackStatus(smartPackClient);
                            break;
                        case "3":
                            await SimpleStatusChecker.RunStatusCheck();
                            break;
                        case "4":
                            await EmergencyStop(apiClient, true);
                            break;
                        case "5":
                            await EmergencyStop(apiClient, false);
                            break;
                        case "6":
                            await HomeRobot(apiClient);
                            break;
                        case "7":
                            await SetWorker(apiClient, true);
                            break;
                        case "8":
                            await SetWorker(apiClient, false);
                            break;
                        case "0":
                            exit = true;
                            break;
                        default:
                            Console.WriteLine("Invalid option. Please try again.");
                            break;
                    }
                }

                // Cleanup
                await wsClient.DisconnectAsync();
                Console.WriteLine("\nDisconnected from WebSocket.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                apiClient.Dispose();
                wsClient.Dispose();
                smartPackClient.Dispose();
            }
        }

        static void SetupWebSocketEventHandlers(RobotWebSocketClient wsClient)
        {
            wsClient.OnConnected += (sender, e) =>
            {
                Console.WriteLine("[WS] Connected to WebSocket");
            };

            wsClient.OnDisconnected += (sender, e) =>
            {
                Console.WriteLine("[WS] Disconnected from WebSocket");
            };

            wsClient.OnStatusUpdate += (sender, status) =>
            {
                // Silent real-time monitoring - can be toggled with verbose flag if needed
            };

            wsClient.OnError += (sender, error) =>
            {
                Console.WriteLine($"[WS] Error: {error}");
            };
        }

        static async Task ShowRobotStatus(RobotApiClient apiClient)
        {
            try
            {
                Console.WriteLine("\nFetching Robot System Status...");
                var status = await apiClient.GetSystemStatus(
                    includeSystemStats: true,
                    includeCamera: false,
                    quickCpu: true
                );

                Console.WriteLine("\n--- Robot System Status ---");
                Console.WriteLine($"Hardware Initialized: {status.HardwareInitialized}");
                Console.WriteLine($"Robot Homed: {status.Homed}");
                Console.WriteLine($"Emergency Stop: {status.EmergencyStopped}");
                
                if (status.Position != null)
                {
                    Console.WriteLine($"Position: X={status.Position.XMm ?? status.Position.X:F1}mm, " +
                                    $"Y={status.Position.YMm ?? status.Position.Y:F1}mm, " +
                                    $"Z={status.Position.ZMm ?? status.Position.Z:F1}mm");
                }

                if (status.Worker != null)
                {
                    Console.WriteLine($"Worker Enabled: {status.Worker.Enabled}");
                    Console.WriteLine($"Queue Size: {status.Worker.QueueSize}");
                }

                if (status.Gripper != null)
                {
                    Console.WriteLine($"Vacuum Enabled: {status.Gripper.VacuumEnabled}");
                    Console.WriteLine($"Holding Item: {status.Gripper.HoldingItem}");
                }

                if (status.SystemStats != null)
                {
                    Console.WriteLine($"CPU Usage: {status.SystemStats.CpuPercent:F1}%");
                    Console.WriteLine($"Memory Usage: {status.SystemStats.MemoryPercent:F1}%");
                    Console.WriteLine($"Uptime: {status.SystemStats.Uptime}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting robot status: {ex.Message}");
            }
        }

        static async Task ShowSmartPackStatus(SmartPackApiClient smartPackClient)
        {
            try
            {
                Console.WriteLine("\nFetching SmartPack Status...");
                
                var connected = await smartPackClient.TestConnection();
                Console.WriteLine($"Connection Status: {(connected ? "Connected" : "Failed")}");

                if (connected)
                {
                    var activeTransfers = await smartPackClient.GetActiveTransfers();
                    if (activeTransfers?["data"] != null)
                    {
                        Console.WriteLine($"Active Transfers: {activeTransfers["data"].Count()}");
                    }

                    var history = await smartPackClient.GetTransferHistory(take: 5);
                    if (history?["totalCount"] != null)
                    {
                        Console.WriteLine($"Total Transfer History: {history["totalCount"]}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting SmartPack status: {ex.Message}");
            }
        }

        static async Task EmergencyStop(RobotApiClient apiClient, bool stop)
        {
            try
            {
                if (stop)
                {
                    await apiClient.EmergencyStop();
                    Console.WriteLine("Emergency stop activated!");
                }
                else
                {
                    await apiClient.ClearEmergencyStop();
                    Console.WriteLine("Emergency stop cleared.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static async Task HomeRobot(RobotApiClient apiClient)
        {
            try
            {
                Console.WriteLine("Starting homing sequence...");
                await apiClient.HomeRobot("small");
                Console.WriteLine("Robot homed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error homing robot: {ex.Message}");
            }
        }

        static async Task SetWorker(RobotApiClient apiClient, bool enable)
        {
            try
            {
                await apiClient.SetWorkerEnabled(enable);
                Console.WriteLine($"Worker {(enable ? "enabled" : "disabled")} successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting worker: {ex.Message}");
            }
        }
    }
}