using System.Security.Cryptography;
using System.Text;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;
using Nethereum.ABI.EIP712; // Added
using Nethereum.Util; 

namespace Polymarket.ClobClient.Signing
{
    public static class SignerUtils
    {
        private const string DomainName = "ClobAuthDomain";
        private const string Version = "1";
        private const string MsgToSign = "This message attests that I control the given wallet";

        public static string BuildClobEip712Signature(string privateKey, int chainId, double timestamp, int nonce)
        {
            var signer = new Eip712TypedDataSigner();
            var key = new EthECKey(privateKey);
            var address = key.GetPublicAddress();
            
            var types = new Dictionary<string, object[]>
            {
                { "EIP712Domain", new object[]
                    {
                        new { name = "name", type = "string" },
                        new { name = "version", type = "string" },
                        new { name = "chainId", type = "uint256" }
                    }
                },
                { "ClobAuth", new object[]
                    {
                        new { name = "address", type = "address" },
                        new { name = "timestamp", type = "string" },
                        new { name = "nonce", type = "uint256" },
                        new { name = "message", type = "string" }
                    }
                }
            };

            var domain = new
            {
                name = DomainName,
                version = Version,
                chainId = chainId
            };

            var message = new
            {
                address = address,
                timestamp = timestamp.ToString(),
                nonce = nonce,
                message = MsgToSign
            };

            var typedData = new
            {
                types = types,
                primaryType = "ClobAuth",
                domain = domain,
                message = message
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(typedData);
            return signer.SignTypedDataV4(json, key);
        }

        public static string BuildPolyHmacSignature(string secret, double timestamp, string method, string requestPath, string body = "")
        {
            var message = $"{timestamp}{method}{requestPath}{body}";
            var secretBytes = ConvertSecretToBytes(secret);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using var hmac = new HMACSHA256(secretBytes);
            var hashBytes = hmac.ComputeHash(messageBytes);
            var sig = Convert.ToBase64String(hashBytes);

            // URL safe replacement
            return sig.Replace("+", "-").Replace("/", "_");
        }

        private static byte[] ConvertSecretToBytes(string base64Secret)
        {
            // Convert base64url to base64
            var sanitized = base64Secret.Replace("-", "+").Replace("_", "/");
            // Remove non-base64 chars (matching TS behavior)
            sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"[^A-Za-z0-9+/=]", "");
            
            // Padding might be needed
            switch (sanitized.Length % 4)
            {
                case 2: sanitized += "=="; break;
                case 3: sanitized += "="; break;
            }

            return Convert.FromBase64String(sanitized);
        }
    }
}
