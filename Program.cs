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
            Console.WriteLine("=== Robot Control Client ===\n");

            // Initialize clients
            var apiClient = new RobotApiClient("http://localhost:8000");
            var wsClient = new RobotWebSocketClient("ws://localhost:8000");

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
                    Console.WriteLine("1. System Status");
                    Console.WriteLine("2. Emergency Stop");
                    Console.WriteLine("3. Clear Emergency Stop");
                    Console.WriteLine("4. Home Robot (Full)");
                    Console.WriteLine("5. Home Robot (Quick)");
                    Console.WriteLine("6. Move to Position");
                    Console.WriteLine("7. Queue Movement");
                    Console.WriteLine("8. Enable Worker");
                    Console.WriteLine("9. Disable Worker");
                    Console.WriteLine("10. Get All Tasks");
                    Console.WriteLine("11. Move Box");
                    Console.WriteLine("12. Manage Shelves");
                    Console.WriteLine("13. Enable Vacuum");
                    Console.WriteLine("14. Disable Vacuum");
                    Console.WriteLine("15. Smart Task (Barcode)");
                    Console.WriteLine("16. Cleanup Stuck Tasks");
                    Console.WriteLine("0. Exit");
                    Console.Write("\nSelect option: ");

                    var choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            await ShowSystemStatus(apiClient);
                            break;
                        case "2":
                            await EmergencyStop(apiClient, true);
                            break;
                        case "3":
                            await EmergencyStop(apiClient, false);
                            break;
                        case "4":
                            await HomeRobot(apiClient, "big");
                            break;
                        case "5":
                            await HomeRobot(apiClient, "small");
                            break;
                        case "6":
                            await MoveToPosition(apiClient);
                            break;
                        case "7":
                            await QueueMovement(apiClient);
                            break;
                        case "8":
                            await SetWorker(apiClient, true);
                            break;
                        case "9":
                            await SetWorker(apiClient, false);
                            break;
                        case "10":
                            await ShowAllTasks(apiClient);
                            break;
                        case "11":
                            await MoveBox(apiClient);
                            break;
                        case "12":
                            await ManageShelves(apiClient);
                            break;
                        case "13":
                            await SetVacuum(apiClient, true);
                            break;
                        case "14":
                            await SetVacuum(apiClient, false);
                            break;
                        case "15":
                            await CreateSmartTask(apiClient);
                            break;
                        case "16":
                            await CleanupTasks(apiClient);
                            break;
                        case "0":
                            exit = true;
                            break;
                        default:
                            Console.WriteLine("Invalid option!");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                // Cleanup
                await wsClient.DisconnectAsync();
                wsClient.Dispose();
                apiClient.Dispose();
            }
        }

        static void SetupWebSocketEventHandlers(RobotWebSocketClient wsClient)
        {
            wsClient.OnConnected += (sender, e) => 
                Console.WriteLine("[WS] Connected to robot");

            wsClient.OnDisconnected += (sender, e) => 
                Console.WriteLine("[WS] Disconnected from robot");

            wsClient.OnStatusUpdate += (sender, msg) =>
            {
                Console.WriteLine($"[WS Status] Position: X={msg.Position?.X:F2}, Y={msg.Position?.Y:F2}, Z={msg.Position?.Z:F2}");
            };

            wsClient.OnTaskUpdate += (sender, msg) =>
            {
                Console.WriteLine($"[WS Task] {msg.TaskId}: {msg.Status} - {msg.Message} ({msg.Progress}%)");
            };

            wsClient.OnTaskCompleted += (sender, msg) =>
            {
                Console.WriteLine($"[WS Task Complete] {msg.TaskId}: {msg.Result} - {msg.Message}");
            };

            wsClient.OnTaskFailed += (sender, msg) =>
            {
                Console.WriteLine($"[WS Task Failed] {msg.TaskId}: {msg.Error} - {msg.Details}");
            };

            wsClient.OnError += (sender, msg) =>
            {
                Console.WriteLine($"[WS Error] {msg.Severity}: {msg.Message} (Code: {msg.Code})");
            };

            wsClient.OnHeartbeat += (sender, msg) =>
            {
                // Optionally log heartbeats
                // Console.WriteLine($"[WS] Heartbeat at {DateTimeOffset.FromUnixTimeSeconds(msg.Timestamp ?? 0)}");
            };
        }

        static async Task ShowSystemStatus(RobotApiClient client)
        {
            Console.WriteLine("\nFetching system status...");
            var status = await client.GetSystemStatus();

            Console.WriteLine($"\n--- System Status ---");
            Console.WriteLine($"Hardware Initialized: {status.HardwareInitialized}");
            Console.WriteLine($"Homed: {status.Homed}");
            Console.WriteLine($"Emergency Stopped: {status.EmergencyStopped}");
            
            if (status.Position != null)
            {
                Console.WriteLine($"Position: X={status.Position.XMm ?? status.Position.X:F2}mm, " +
                                $"Y={status.Position.YMm ?? status.Position.Y:F2}mm, " +
                                $"Z={status.Position.ZMm ?? status.Position.Z:F2}mm");
            }

            if (status.Worker != null)
            {
                Console.WriteLine($"Worker: Enabled={status.Worker.Enabled}, " +
                                $"Processing={status.Worker.Processing}, " +
                                $"Queue Size={status.Worker.QueueSize}");
            }

            if (status.Gripper != null)
            {
                Console.WriteLine($"Gripper: Vacuum={status.Gripper.VacuumEnabled}, " +
                                $"Holding Item={status.Gripper.HoldingItem}");
            }
        }

        static async Task EmergencyStop(RobotApiClient client, bool enable)
        {
            var action = enable ? "Activating" : "Clearing";
            Console.WriteLine($"\n{action} emergency stop...");
            var response = await client.EmergencyStop(enable);
            Console.WriteLine($"Response: {response.Message}");
        }

        static async Task HomeRobot(RobotApiClient client, string mode)
        {
            Console.WriteLine($"\nStarting {mode} homing sequence...");
            var response = await client.Home(mode);
            Console.WriteLine($"Response: {response.Message}");
            Console.WriteLine($"Task ID: {response.TaskId}");
        }

        static async Task MoveToPosition(RobotApiClient client)
        {
            Console.Write("\nEnter X position (mm) or press Enter to skip: ");
            var xInput = Console.ReadLine();
            double? x = string.IsNullOrEmpty(xInput) ? null : double.Parse(xInput);

            Console.Write("Enter Y position (mm) or press Enter to skip: ");
            var yInput = Console.ReadLine();
            double? y = string.IsNullOrEmpty(yInput) ? null : double.Parse(yInput);

            Console.Write("Enter Z position (mm) or press Enter to skip: ");
            var zInput = Console.ReadLine();
            double? z = string.IsNullOrEmpty(zInput) ? null : double.Parse(zInput);

            Console.WriteLine("\nMoving to position...");
            var response = await client.MoveDirectAsync(x, y, z);
            Console.WriteLine($"Response: {response.Message}");
        }

        static async Task QueueMovement(RobotApiClient client)
        {
            Console.Write("\nEnter X position (mm): ");
            var x = double.Parse(Console.ReadLine() ?? "0");

            Console.Write("Enter Y position (mm): ");
            var y = double.Parse(Console.ReadLine() ?? "0");

            Console.WriteLine("\nQueuing movement...");
            var response = await client.QueueMove(x, y);
            Console.WriteLine($"Response: {response.Message}");
            Console.WriteLine($"Task ID: {response.TaskId}");
        }

        static async Task SetWorker(RobotApiClient client, bool enable)
        {
            var action = enable ? "Enabling" : "Disabling";
            Console.WriteLine($"\n{action} worker...");
            var response = await client.SetWorkerEnabled(enable);
            Console.WriteLine($"Response: {response.Message}");
        }

        static async Task ShowAllTasks(RobotApiClient client)
        {
            Console.WriteLine("\nFetching all tasks...");
            var tasks = await client.GetAllTasks();

            Console.WriteLine($"\n--- Tasks ({tasks.Length}) ---");
            foreach (var task in tasks)
            {
                Console.WriteLine($"ID: {task.TaskId}");
                Console.WriteLine($"  Type: {task.Type}");
                Console.WriteLine($"  Status: {task.Status}");
                Console.WriteLine($"  Description: {task.Description}");
                if (task.Error != null)
                    Console.WriteLine($"  Error: {task.Error}");
                Console.WriteLine();
            }
        }

        static async Task MoveBox(RobotApiClient client)
        {
            Console.WriteLine("\nMove Box Operation");
            Console.WriteLine("1. Use shelf IDs");
            Console.WriteLine("2. Use coordinates");
            Console.Write("Select option: ");
            
            var option = Console.ReadLine();
            var request = new MoveBoxRequest();

            if (option == "1")
            {
                Console.Write("Enter source shelf ID: ");
                request.SourceShelfId = Console.ReadLine();

                Console.Write("Enter destination shelf ID (or 'AUTO' for automatic): ");
                request.DestinationShelfId = Console.ReadLine();
            }
            else
            {
                Console.Write("Enter source X (mm): ");
                request.SourceXMm = double.Parse(Console.ReadLine() ?? "0");

                Console.Write("Enter source Y (mm): ");
                request.SourceYMm = double.Parse(Console.ReadLine() ?? "0");

                Console.Write("Enter destination X (mm): ");
                request.DestinationXMm = double.Parse(Console.ReadLine() ?? "0");

                Console.Write("Enter destination Y (mm): ");
                request.DestinationYMm = double.Parse(Console.ReadLine() ?? "0");
            }

            Console.Write("Enter barcode (optional): ");
            var barcode = Console.ReadLine();
            if (!string.IsNullOrEmpty(barcode))
                request.Barcode = barcode;

            Console.WriteLine("\nQueuing move box operation...");
            var response = await client.MoveBox(request);
            Console.WriteLine($"Response: {response.Message}");
            Console.WriteLine($"Task ID: {response.TaskId}");
        }

        static async Task ManageShelves(RobotApiClient client)
        {
            Console.WriteLine("\nShelf Management");
            Console.WriteLine("1. List all shelves");
            Console.WriteLine("2. Add new shelf");
            Console.WriteLine("3. Find nearest shelf");
            Console.WriteLine("4. Delete shelf");
            Console.Write("Select option: ");

            var option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    var shelves = await client.GetAllShelves();
                    Console.WriteLine($"\n--- Shelves ({shelves.Length}) ---");
                    foreach (var shelf in shelves)
                    {
                        Console.WriteLine($"ID: {shelf.ShelfId}");
                        Console.WriteLine($"  Position: X={shelf.XPosition}, Y={shelf.YPosition}");
                        Console.WriteLine($"  Occupied: {shelf.IsOccupied}");
                        if (shelf.OccupiedByBarcode != null)
                            Console.WriteLine($"  Item: {shelf.OccupiedByBarcode}");
                        Console.WriteLine();
                    }
                    break;

                case "2":
                    var newShelf = new ShelfLocation();
                    Console.Write("Enter X position (mm): ");
                    newShelf.XPosition = double.Parse(Console.ReadLine() ?? "0");

                    Console.Write("Enter Y position (mm): ");
                    newShelf.YPosition = double.Parse(Console.ReadLine() ?? "0");

                    Console.Write("Enter shelf barcode (optional): ");
                    var shelfBarcode = Console.ReadLine();
                    if (!string.IsNullOrEmpty(shelfBarcode))
                        newShelf.ShelfBarcode = shelfBarcode;

                    var addResponse = await client.AddShelf(newShelf);
                    Console.WriteLine($"Response: {addResponse.Message}");
                    break;

                case "3":
                    Console.Write("Enter X position (mm): ");
                    var nearX = double.Parse(Console.ReadLine() ?? "0");

                    Console.Write("Enter Y position (mm): ");
                    var nearY = double.Parse(Console.ReadLine() ?? "0");

                    Console.Write("Enter max distance (mm, optional): ");
                    var maxDistStr = Console.ReadLine();
                    double? maxDist = string.IsNullOrEmpty(maxDistStr) ? null : double.Parse(maxDistStr);

                    var nearestShelf = await client.FindNearestShelf(nearX, nearY, maxDist);
                    if (nearestShelf != null)
                    {
                        Console.WriteLine($"\nNearest Shelf: {nearestShelf.ShelfId}");
                        Console.WriteLine($"  Position: X={nearestShelf.XPosition}, Y={nearestShelf.YPosition}");
                    }
                    else
                    {
                        Console.WriteLine("No shelf found within range");
                    }
                    break;

                case "4":
                    Console.Write("Enter shelf ID to delete: ");
                    var deleteId = Console.ReadLine();
                    var deleteResponse = await client.DeleteShelf(deleteId!);
                    Console.WriteLine($"Response: {deleteResponse.Message}");
                    break;
            }
        }

        static async Task SetVacuum(RobotApiClient client, bool enable)
        {
            var action = enable ? "Enabling" : "Disabling";
            Console.WriteLine($"\n{action} vacuum...");
            var response = await client.SetVacuumEnabled(enable);
            Console.WriteLine($"Response: {response.Message}");
        }

        static async Task CreateSmartTask(RobotApiClient client)
        {
            Console.Write("\nEnter barcode: ");
            var barcode = Console.ReadLine();

            if (string.IsNullOrEmpty(barcode))
            {
                Console.WriteLine("Barcode is required!");
                return;
            }

            Console.WriteLine("\nCreating smart task...");
            var response = await client.CreateSmartTask(barcode);
            Console.WriteLine($"Response: {response.Message}");
            Console.WriteLine($"Task ID: {response.TaskId}");
        }

        static async Task CleanupTasks(RobotApiClient client)
        {
            Console.Write("\nEnter hours old threshold (default 0.5): ");
            var hoursStr = Console.ReadLine();
            double hours = string.IsNullOrEmpty(hoursStr) ? 0.5 : double.Parse(hoursStr);

            Console.WriteLine("Target: 1=History, 2=Queue, 3=Both");
            Console.Write("Select target (default 3): ");
            var targetChoice = Console.ReadLine();
            string target = targetChoice switch
            {
                "1" => "history",
                "2" => "queue",
                _ => "both"
            };

            Console.WriteLine("\nCleaning up stuck tasks...");
            var response = await client.CleanupStuckTasks(hours, target);
            Console.WriteLine($"Response: {response.Message}");
            if (response.Details != null)
            {
                Console.WriteLine($"Details: {response.Details}");
            }
        }
    }
}