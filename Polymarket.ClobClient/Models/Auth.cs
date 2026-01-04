using Newtonsoft.Json;

namespace Polymarket.ClobClient.Models
{
    public class ApiKeyCreds
    {
        [JsonProperty("key")]
        public string Key { get; set; } = string.Empty;

        [JsonProperty("secret")]
        public string Secret { get; set; } = string.Empty;

        [JsonProperty("passphrase")]
        public string Passphrase { get; set; } = string.Empty;
    }

    public class ApiKeyRaw
    {
        [JsonProperty("apiKey")]
           public string ApiKey { get; set; } = string.Empty;

        [JsonProperty("secret")]
           public string Secret { get; set; } = string.Empty;

        [JsonProperty("passphrase")]
           public string Passphrase { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
         [JsonProperty("apiKey")]
            public string ApiKey { get; set; } = string.Empty;
    }
}
