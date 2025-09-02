using System;
using System.Net.Http;
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

        // Connection Test
        public async Task<bool> TestConnection()
        {
            try
            {
                var response = await _httpClient.GetAsync("/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Health Check
        public async Task<string> HealthCheck()
        {
            var response = await _httpClient.GetAsync("/health");
            return await response.Content.ReadAsStringAsync();
        }

        // System Status - GET only
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

        // Essential Control Operations (minimal POST operations kept for safety)
        public async Task<ApiResponse> EmergencyStop()
        {
            var response = await _httpClient.PutAsync("/robot/system/e_stop?enabled=true", null);
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiResponse>(content) ?? new ApiResponse();
        }

        public async Task<ApiResponse> ClearEmergencyStop()
        {
            var response = await _httpClient.PutAsync("/robot/system/e_stop?enabled=false", null);
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiResponse>(content) ?? new ApiResponse();
        }

        public async Task<ApiResponse> SetWorkerEnabled(bool enabled)
        {
            var response = await _httpClient.PutAsync($"/robot/system/worker?enabled={enabled}", null);
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiResponse>(content) ?? new ApiResponse();
        }

        public async Task<ApiResponse> HomeRobot(string mode = "small")
        {
            var response = await _httpClient.PostAsync($"/robot/motion/home?mode={mode}", null);
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiResponse>(content) ?? new ApiResponse();
        }

        // Queue Information - GET only
        public async Task<TaskInfo[]> GetAllTasks()
        {
            var response = await _httpClient.GetAsync("/robot/queue/tasks");
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TaskInfo[]>(content) ?? Array.Empty<TaskInfo>();
        }

        // Inventory Information - GET only
        public async Task<ShelfLocation[]> GetAllShelves()
        {
            var response = await _httpClient.GetAsync("/robot/inventory/shelves");
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ShelfLocation[]>(content) ?? Array.Empty<ShelfLocation>();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}