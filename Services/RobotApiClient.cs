using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RobotControlClient.Models;

namespace RobotControlClient.Services
{
    public class RobotApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public RobotApiClient(string baseUrl = "http://localhost:8000")
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        // System Endpoints
        public async Task<ApiResponse> EmergencyStop(bool enabled)
        {
            var response = await _httpClient.PutAsync($"/robot/system/e_stop?enabled={enabled}", null);
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiResponse>(content) ?? new ApiResponse();
        }

        public async Task<ApiResponse> SetWorkerEnabled(bool enabled)
        {
            var response = await _httpClient.PutAsync($"/robot/system/worker?enabled={enabled}", null);
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiResponse>(content) ?? new ApiResponse();
        }

        public async Task<RobotStatus> GetSystemStatus(bool includeWorker = true, bool includeGripper = true, 
            bool includeMotion = true, bool includeSystemStats = true, bool includeWorkspace = true, 
            bool includeCamera = true, bool quickCpu = true)
        {
            var queryParams = $"?include_worker={includeWorker}&include_gripper={includeGripper}" +
                            $"&include_motion={includeMotion}&include_system_stats={includeSystemStats}" +
                            $"&include_workspace={includeWorkspace}&include_camera={includeCamera}&quick_cpu={quickCpu}";
            
            var response = await _httpClient.GetAsync($"/robot/system/status{queryParams}");
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<RobotStatus>(content) ?? new RobotStatus();
        }

        // Motion Endpoints
        public async Task<ApiResponse> Home(string mode = "big")
        {
            var response = await _httpClient.PostAsync($"/robot/motion/home?mode={mode}", null);
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiResponse>(content) ?? new ApiResponse();
        }

        public async Task<ApiResponse> MoveDirectAsync(double? x = null, double? y = null, double? z = null)
        {
            var queryParams = "";
            if (x.HasValue) queryParams += $"x={x}";
            if (y.HasValue) queryParams += (queryParams.Length > 0 ? "&" : "") + $"y={y}";
            if (z.HasValue) queryParams += (queryParams.Length > 0 ? "&" : "") + $"z={z}";

            var response = await _httpClient.PostAsync($"/robot/motion/move_direct?{queryParams}", null);
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiResponse>(content) ?? new ApiResponse();
        }

        public async Task<ApiResponse> QueueMove(double xMm, double yMm, int? rpmX = null, int? rpmY = null)
        {
            var request = new QueueMoveRequest
            {
                XMm = xMm,
                YMm = yMm,
                RpmX = rpmX,
                RpmY = rpmY
            };

            var json = JsonConvert.SerializeObject(request);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/robot/motion/queue_move", httpContent);
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiResponse>(content) ?? new ApiResponse();
        }

        // Operations Endpoints
        public async Task<ApiResponse> MoveBox(MoveBoxRequest request)
        {
            var json = JsonConvert.SerializeObject(request);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/robot/operations/move_box", httpContent);
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiResponse>(content) ?? new ApiResponse();
        }

        // Queue Management
        public async Task<TaskInfo[]> GetAllTasks()
        {
            var response = await _httpClient.GetAsync("/robot/queue/tasks");
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TaskInfo[]>(content) ?? Array.Empty<TaskInfo>();
        }

        public async Task<ApiResponse> CancelTask(string taskId)
        {
            var response = await _httpClient.DeleteAsync($"/robot/queue/tasks/{taskId}");
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiResponse>(content) ?? new ApiResponse();
        }

        public async Task<ApiResponse> CreateSmartTask(string barcode)
        {
            var json = JsonConvert.SerializeObject(new { barcode });
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/robot/queue/tasks/smart_task", httpContent);
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiResponse>(content) ?? new ApiResponse();
        }

        public async Task<ApiResponse> CleanupStuckTasks(double hoursOld = 0.5, string target = "both")
        {
            var response = await _httpClient.PostAsync($"/robot/queue/tasks/cleanup?hours_old={hoursOld}&target={target}", null);
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiResponse>(content) ?? new ApiResponse();
        }

        // Inventory Management
        public async Task<ApiResponse> AddShelf(ShelfLocation shelf)
        {
            var json = JsonConvert.SerializeObject(shelf);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/robot/inventory/shelves", httpContent);
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiResponse>(content) ?? new ApiResponse();
        }

        public async Task<ShelfLocation[]> GetAllShelves()
        {
            var response = await _httpClient.GetAsync("/robot/inventory/shelves");
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ShelfLocation[]>(content) ?? Array.Empty<ShelfLocation>();
        }

        public async Task<ShelfLocation?> GetShelfById(string shelfId)
        {
            var response = await _httpClient.GetAsync($"/robot/inventory/shelves?shelf_id={shelfId}");
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ShelfLocation>(content);
        }

        public async Task<ShelfLocation?> FindNearestShelf(double x, double y, double? maxDistance = null)
        {
            var queryParams = $"?nearest_x={x}&nearest_y={y}";
            if (maxDistance.HasValue)
                queryParams += $"&max_distance={maxDistance}";

            var response = await _httpClient.GetAsync($"/robot/inventory/shelves{queryParams}");
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ShelfLocation>(content);
        }

        public async Task<ApiResponse> UpdateShelf(string shelfId, ShelfLocation shelf)
        {
            var json = JsonConvert.SerializeObject(shelf);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"/robot/inventory/shelves/{shelfId}", httpContent);
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiResponse>(content) ?? new ApiResponse();
        }

        public async Task<ApiResponse> DeleteShelf(string shelfId)
        {
            var response = await _httpClient.DeleteAsync($"/robot/inventory/shelves/{shelfId}");
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiResponse>(content) ?? new ApiResponse();
        }

        // Gripper Control
        public async Task<ApiResponse> SetVacuumEnabled(bool enabled)
        {
            var response = await _httpClient.PutAsync($"/robot/gripper/vacuum?enabled={enabled}", null);
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiResponse>(content) ?? new ApiResponse();
        }

        // Health Check
        public async Task<string> HealthCheck()
        {
            var response = await _httpClient.GetAsync("/health");
            return await response.Content.ReadAsStringAsync();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}