using Newtonsoft.Json;

namespace Polymarket.ClobClient.Models
{
    public class ApiKeyCreds
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("secret")]
        public string Secret { get; set; }

        [JsonProperty("passphrase")]
        public string Passphrase { get; set; }
    }

    public class ApiKeyRaw
    {
        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }

        [JsonProperty("secret")]
        public string Secret { get; set; }

        [JsonProperty("passphrase")]
        public string Passphrase { get; set; }
    }

    public class LoginResponse
    {
         [JsonProperty("apiKey")]
         public string ApiKey { get; set; }
    }
}
