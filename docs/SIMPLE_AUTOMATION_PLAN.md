# Ultra-Simple Robot Automation Plan

## Goal
Transform the existing monitoring client into an automated worker that moves boxes based on SmartPack's refill needs.

## The Simplest Approach - Just 4 Additions

### 1. Add 3 Methods to SmartPackApiClient.cs

```csharp
// Request a transfer lock (tells SmartPack "I'm taking this job")
public async Task<bool> RequestTransfer(int fromPlacementId, int toPlacementId, string lockHolder = "Robot1")
{
    var data = new {
        fromPlacementId = fromPlacementId,
        toPlacementId = toPlacementId,
        lockHolder = lockHolder
    };
    
    var json = JsonConvert.SerializeObject(data);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    
    var response = await _httpClient.PostAsync("/api/v1/robot/requesttransfer", content);
    if (response.IsSuccessStatusCode)
    {
        var result = JObject.Parse(await response.Content.ReadAsStringAsync());
        return result["success"]?.Value<bool>() ?? false;
    }
    return false;
}

// Commit successful transfer (tells SmartPack "job done")
public async Task<bool> CommitTransfer(int fromPlacementId, int toPlacementId, string lockHolder = "Robot1")
{
    var data = new {
        fromPlacementId = fromPlacementId,
        toPlacementId = toPlacementId,
        lockHolder = lockHolder
    };
    
    var json = JsonConvert.SerializeObject(data);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    
    var response = await _httpClient.PostAsync("/api/v1/robot/committransfer", content);
    if (response.IsSuccessStatusCode)
    {
        var result = JObject.Parse(await response.Content.ReadAsStringAsync());
        return result["success"]?.Value<bool>() ?? false;
    }
    return false;
}

// Cancel failed transfer (tells SmartPack "couldn't do it, release the lock")
public async Task<bool> CancelTransfer(int fromPlacementId, int toPlacementId, string lockHolder = "Robot1")
{
    var data = new {
        fromPlacementId = fromPlacementId,
        toPlacementId = toPlacementId,
        lockHolder = lockHolder
    };
    
    var json = JsonConvert.SerializeObject(data);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    
    var response = await _httpClient.PostAsync("/api/v1/robot/canceltransfer", content);
    if (response.IsSuccessStatusCode)
    {
        var result = JObject.Parse(await response.Content.ReadAsStringAsync());
        return result["success"]?.Value<bool>() ?? false;
    }
    return false;
}
```

### 2. Add 1 Method to RobotApiClient.cs

```csharp
// Execute box movement (queues the task)
public async Task<string?> MoveBox(string sourceShelfBarcode, string destShelfBarcode)
{
    var data = new {
        source_shelf_id = sourceShelfBarcode,
        destination_shelf_id = destShelfBarcode
    };
    
    var json = JsonConvert.SerializeObject(data);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    
    var response = await _httpClient.PostAsync("/robot/operations/move_box", content);
    if (response.IsSuccessStatusCode)
    {
        var result = JsonConvert.DeserializeObject<ApiResponse>(await response.Content.ReadAsStringAsync());
        return result?.TaskId;
    }
    return null;
}
```

### 3. Add Simple Task Finder

Create new file `src/Services/TaskFinder.cs`:

```csharp
using Newtonsoft.Json.Linq;

namespace RobotControlClient.Services
{
    public class RefillTask
    {
        public int FromPlacementId { get; set; }
        public int ToPlacementId { get; set; }
        public string? Sku { get; set; }
    }

    public static class TaskFinder
    {
        public static RefillTask? FindRefillTask(JObject? sectionInfo)
        {
            if (sectionInfo == null) return null;
            
            var placements = sectionInfo["data"] as JArray;
            if (placements == null) return null;
            
            // Find placement that needs refilling
            foreach (var placement in placements)
            {
                // Skip if locked
                if (placement["transferLocked"]?.Value<bool>() == true)
                    continue;
                    
                // Check if needs refill
                var toRefill = placement["toRefill"]?.Value<int>() ?? 0;
                if (toRefill <= 0)
                    continue;
                    
                // Get the SKU needed (if placement has items)
                var items = placement["items"] as JArray;
                if (items == null || items.Count == 0)
                    continue;
                    
                var neededSku = items[0]["sku"]?.Value<string>();
                if (string.IsNullOrEmpty(neededSku))
                    continue;
                    
                var destinationId = placement["placementId"].Value<int>();
                
                // Find source with this SKU in buffer
                foreach (var source in placements)
                {
                    // Skip if locked
                    if (source["transferLocked"]?.Value<bool>() == true)
                        continue;
                        
                    // Must be refill location (buffer)
                    if (source["placementIsRefill"]?.Value<bool>() != true)
                        continue;
                        
                    // Must have stock
                    if (source["itemCount"]?.Value<decimal>() <= 0)
                        continue;
                        
                    // Check if has the needed SKU
                    var sourceItems = source["items"] as JArray;
                    if (sourceItems != null)
                    {
                        foreach (var item in sourceItems)
                        {
                            if (item["sku"]?.Value<string>() == neededSku)
                            {
                                return new RefillTask
                                {
                                    FromPlacementId = source["placementId"].Value<int>(),
                                    ToPlacementId = destinationId,
                                    Sku = neededSku
                                };
                            }
                        }
                    }
                }
            }
            
            return null;
        }
    }
}
```

### 4. Add Worker Mode to Program.cs

Add new menu option "9. Start Automated Worker":

```csharp
case "9":
    Console.WriteLine("\n=== AUTOMATED WORKER MODE ===");
    Console.WriteLine("Press any key to stop...\n");
    
    var workerTask = Task.Run(async () =>
    {
        while (!Console.KeyAvailable)
        {
            try
            {
                // 1. Check robot is ready
                var status = await apiClient.GetSystemStatus();
                if (!status.Homed)
                {
                    Console.WriteLine("[Worker] Robot not homed, waiting...");
                    await Task.Delay(10000);
                    continue;
                }
                if (status.EmergencyStopped)
                {
                    Console.WriteLine("[Worker] Emergency stop active, waiting...");
                    await Task.Delay(5000);
                    continue;
                }
                
                // 2. Get work from SmartPack
                Console.WriteLine("[Worker] Checking for work...");
                var sectionInfo = await smartPackClient.GetSectionInfo();
                var task = TaskFinder.FindRefillTask(sectionInfo);
                
                if (task == null)
                {
                    Console.WriteLine("[Worker] No work available");
                    await Task.Delay(10000);
                    continue;
                }
                
                Console.WriteLine($"[Worker] Found task: Move SKU {task.Sku} from {task.FromPlacementId} to {task.ToPlacementId}");
                
                // 3. Lock the transfer
                Console.WriteLine("[Worker] Requesting transfer lock...");
                bool locked = await smartPackClient.RequestTransfer(
                    task.FromPlacementId, 
                    task.ToPlacementId, 
                    "Robot1");
                
                if (!locked)
                {
                    Console.WriteLine("[Worker] Could not lock transfer");
                    await Task.Delay(5000);
                    continue;
                }
                
                // 4. Execute robot movement
                Console.WriteLine("[Worker] Executing robot movement...");
                var taskId = await apiClient.MoveBox(
                    task.FromPlacementId.ToString(), 
                    task.ToPlacementId.ToString());
                
                if (string.IsNullOrEmpty(taskId))
                {
                    Console.WriteLine("[Worker] Failed to queue movement");
                    await smartPackClient.CancelTransfer(
                        task.FromPlacementId, 
                        task.ToPlacementId, 
                        "Robot1");
                    continue;
                }
                
                // 5. Simple wait for completion (ultra-simple version)
                Console.WriteLine($"[Worker] Task {taskId} queued, waiting 30 seconds...");
                await Task.Delay(30000);  // Just wait 30 seconds for now
                
                // 6. Check if task completed
                var tasks = await apiClient.GetAllTasks();
                var robotTask = tasks.FirstOrDefault(t => t.TaskId == taskId);
                
                if (robotTask?.Status == "completed")
                {
                    Console.WriteLine("[Worker] Movement completed, committing transfer");
                    await smartPackClient.CommitTransfer(
                        task.FromPlacementId, 
                        task.ToPlacementId, 
                        "Robot1");
                }
                else
                {
                    Console.WriteLine("[Worker] Movement not completed, cancelling transfer");
                    await smartPackClient.CancelTransfer(
                        task.FromPlacementId, 
                        task.ToPlacementId, 
                        "Robot1");
                }
                
                Console.WriteLine("[Worker] Cycle complete, waiting before next check...");
                await Task.Delay(5000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Worker] Error: {ex.Message}");
                await Task.Delay(10000);
            }
        }
    });
    
    Console.ReadKey();
    Console.WriteLine("\n[Worker] Stopping...");
    break;
```

## Prerequisites - One-Time Setup

### Map SmartPack Placements to Robot Positions

Before running the automation, you need to tell the Robot API where each SmartPack placement is physically located:

```bash
# For each shelf in your warehouse:
POST http://localhost:8000/robot/inventory/shelves
{
    "x_position": 500.0,
    "y_position": 300.0,
    "shelf_barcode": "348"  # This is the SmartPack placement ID
}
```

Do this once for every shelf. The Robot API will remember these positions.

## How It Works

1. **Run the program**: `dotnet run`
2. **Select option 9**: Start Automated Worker
3. **The system will**:
   - Check SmartPack for items that need refilling
   - Find where to get those items from
   - Lock the transfer in SmartPack (prevents duplicates)
   - Tell robot to move the box
   - Wait for completion
   - Update SmartPack inventory

## Key Points

- **SmartPack decides** what needs moving (based on `toRefill` values)
- **Transfer locks** prevent duplicate commands
- **Simple waiting** (30 seconds) is enough for testing
- **Can improve later** with better task monitoring

## Testing Order

1. First test with one manual move
2. Watch it work
3. Let it run continuously
4. Add better monitoring if needed

## Total Code: ~200 lines across 4 files

That's it! This is the simplest possible working automation.