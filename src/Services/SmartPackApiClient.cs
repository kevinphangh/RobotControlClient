using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RobotControlClient.Services
{
    public class SmartPackApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public SmartPackApiClient(string baseUrl = "https://kangaroo.smartpack.dk")
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };
            
            // Add Basic Authentication
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("Robot:Robot"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        // Simple test connection - try to get active transfers
        public async Task<bool> TestConnection()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/v1/robot/getactivetransfers");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Connection error: {ex.Message}");
                return false;
            }
        }

        // Get Section Info - main GET endpoint for section data
        public async Task<JObject?> GetSectionInfo(string? sectionId = null, int popularityDays = 7)
        {
            try
            {
                var url = "/api/v1/robot/sectioninfo";
                if (!string.IsNullOrEmpty(sectionId))
                {
                    url += $"?sectionid={sectionId}&popularitydays={popularityDays}";
                }

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JObject.Parse(content);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting section info: {ex.Message}");
                return null;
            }
        }

        // Get Active Transfers
        public async Task<JObject?> GetActiveTransfers(string? sectionId = null)
        {
            try
            {
                var url = "/api/v1/robot/getactivetransfers";
                if (!string.IsNullOrEmpty(sectionId))
                {
                    url += $"?sectionid={sectionId}";
                }

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JObject.Parse(content);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting active transfers: {ex.Message}");
                return null;
            }
        }

        // Get Transfer History
        public async Task<JObject?> GetTransferHistory(string? sectionId = null, int skip = 0, int take = 10)
        {
            try
            {
                var url = $"/api/v1/robot/gettransferhistory?skip={skip}&take={take}";
                if (!string.IsNullOrEmpty(sectionId))
                {
                    url += $"&sectionid={sectionId}";
                }

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JObject.Parse(content);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting transfer history: {ex.Message}");
                return null;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}