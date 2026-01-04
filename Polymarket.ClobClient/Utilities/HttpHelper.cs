using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace Polymarket.ClobClient.Utilities
{
    public class HttpHelper
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public HttpHelper(HttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl.TrimEnd('/');
        }
        
        public async Task<T> GetAsync<T>(string endpoint, Dictionary<string, string> headers = null, Dictionary<string, object> queryParams = null)
        {
            var url = BuildUrl(endpoint, queryParams);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            AddHeaders(request, headers);

            var response = await _httpClient.SendAsync(request);
            return await HandleResponse<T>(response);
        }

        public async Task<T> PostAsync<T>(string endpoint, object data, Dictionary<string, string> headers = null)
        {
            var url = BuildUrl(endpoint);
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            AddHeaders(request, headers);

            if (data != null)
            {
                var json = JsonConvert.SerializeObject(data);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.SendAsync(request);
            return await HandleResponse<T>(response);
        }

        public async Task<T> DeleteAsync<T>(string endpoint, object data = null, Dictionary<string, string> headers = null)
        {
            var url = BuildUrl(endpoint); 
            // HttpClient DELETE with body is tricky, typically better to use SendAsync with HttpRequestMessage
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            AddHeaders(request, headers);
            
            if (data != null)
            {
                 var json = JsonConvert.SerializeObject(data);
                 request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.SendAsync(request);
            return await HandleResponse<T>(response);
        }

        private string BuildUrl(string endpoint, Dictionary<string, object> queryParams = null)
        {
            var url = $"{_baseUrl}{endpoint}";
            if (queryParams != null && queryParams.Count > 0)
            {
                var query = string.Join("&", queryParams.Select(kv => $"{kv.Key}={kv.Value}"));
                url += $"?{query}";
            }
            return url;
        }

        private void AddHeaders(HttpRequestMessage request, Dictionary<string, string> headers)
        {
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        private async Task<T> HandleResponse<T>(HttpResponseMessage response)
        {
             var content = await response.Content.ReadAsStringAsync();
             if (!response.IsSuccessStatusCode)
             {
                 throw new HttpRequestException($"API Error {response.StatusCode}: {content}");
             }
             
             // Check if T is string, simpler return
             if (typeof(T) == typeof(string))
             {
                 return (T)(object)content;
             }
             
             return JsonConvert.DeserializeObject<T>(content);
        }
    }
}
