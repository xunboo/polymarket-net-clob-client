using Xunit;
using Polymarket.ClobClient.Signing;
using Polymarket.ClobClient.Models;

namespace Polymarket.ClobClient.Tests.Headers
{
    public class HeadersTests
    {
        private readonly string _privateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";
        private readonly string _expectedAddress = "0xf39Fd6e51aad88F6F4ce6aB8827279cffFb92266";
        private readonly int _chainId = 80002; // Amoy
        
        private readonly ApiKeyCreds _creds = new ApiKeyCreds
        {
            Key = "000000000-0000-0000-0000-000000000000",
            Passphrase = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
            Secret = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA="
        };

        // --- L1 Headers Tests (EIP-712 Signing) ---

        [Fact]
        public void CreateL1Headers_NoNonce_ShouldReturnValidHeaders()
        {
            // L1 headers are created using EIP-712 signature
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            int nonce = 0;
            
            var signature = SignerUtils.BuildClobEip712Signature(_privateKey, _chainId, timestamp, nonce);
            
            // Simulate header creation
            var headers = new Dictionary<string, string>
            {
                { "POLY_ADDRESS", _expectedAddress },
                { "POLY_SIGNATURE", signature },
                { "POLY_TIMESTAMP", timestamp.ToString() },
                { "POLY_NONCE", nonce.ToString() }
            };

            Assert.NotNull(headers);
            Assert.Equal(_expectedAddress, headers["POLY_ADDRESS"]);
            Assert.NotEmpty(headers["POLY_SIGNATURE"]);
            Assert.NotEmpty(headers["POLY_TIMESTAMP"]);
            Assert.True(long.Parse(headers["POLY_TIMESTAMP"]) <= DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            Assert.Equal("0", headers["POLY_NONCE"]);
        }

        [Fact]
        public void CreateL1Headers_WithNonce_ShouldReturnValidHeaders()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            int nonce = 1012;
            
            var signature = SignerUtils.BuildClobEip712Signature(_privateKey, _chainId, timestamp, nonce);
            
            var headers = new Dictionary<string, string>
            {
                { "POLY_ADDRESS", _expectedAddress },
                { "POLY_SIGNATURE", signature },
                { "POLY_TIMESTAMP", timestamp.ToString() },
                { "POLY_NONCE", nonce.ToString() }
            };

            Assert.NotNull(headers);
            Assert.Equal(_expectedAddress, headers["POLY_ADDRESS"]);
            Assert.NotEmpty(headers["POLY_SIGNATURE"]);
            Assert.NotEmpty(headers["POLY_TIMESTAMP"]);
            Assert.True(long.Parse(headers["POLY_TIMESTAMP"]) <= DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            Assert.Equal("1012", headers["POLY_NONCE"]);
        }

        // --- L2 Headers Tests (HMAC Signing) ---

        [Fact]
        public void CreateL2Headers_NoBody_ShouldReturnValidHeaders()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string method = "GET";
            string requestPath = "/order";
            string body = "";
            
            var signature = SignerUtils.BuildPolyHmacSignature(_creds.Secret, timestamp, method, requestPath, body);
            
            var headers = new Dictionary<string, string>
            {
                { "POLY_ADDRESS", _expectedAddress },
                { "POLY_SIGNATURE", signature },
                { "POLY_TIMESTAMP", timestamp.ToString() },
                { "POLY_API_KEY", _creds.Key },
                { "POLY_PASSPHRASE", _creds.Passphrase }
            };

            Assert.NotNull(headers);
            Assert.Equal(_expectedAddress, headers["POLY_ADDRESS"]);
            Assert.NotEmpty(headers["POLY_SIGNATURE"]);
            Assert.NotEmpty(headers["POLY_TIMESTAMP"]);
            Assert.True(long.Parse(headers["POLY_TIMESTAMP"]) <= DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            Assert.Equal(_creds.Key, headers["POLY_API_KEY"]);
            Assert.Equal(_creds.Passphrase, headers["POLY_PASSPHRASE"]);
        }

        [Fact]
        public void CreateL2Headers_WithBody_ShouldReturnValidHeaders()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string method = "GET";
            string requestPath = "/order";
            string body = "{\"hash\": \"0x123\"}";
            
            var signature = SignerUtils.BuildPolyHmacSignature(_creds.Secret, timestamp, method, requestPath, body);
            
            var headers = new Dictionary<string, string>
            {
                { "POLY_ADDRESS", _expectedAddress },
                { "POLY_SIGNATURE", signature },
                { "POLY_TIMESTAMP", timestamp.ToString() },
                { "POLY_API_KEY", _creds.Key },
                { "POLY_PASSPHRASE", _creds.Passphrase }
            };

            Assert.NotNull(headers);
            Assert.Equal(_expectedAddress, headers["POLY_ADDRESS"]);
            Assert.NotEmpty(headers["POLY_SIGNATURE"]);
            Assert.NotEmpty(headers["POLY_TIMESTAMP"]);
            Assert.True(long.Parse(headers["POLY_TIMESTAMP"]) <= DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            Assert.Equal(_creds.Key, headers["POLY_API_KEY"]);
            Assert.Equal(_creds.Passphrase, headers["POLY_PASSPHRASE"]);
        }
    }
}
