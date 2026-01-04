using Xunit;
using Polymarket.ClobClient.Signing;
using Polymarket.ClobClient.Models;
using System.Threading.Tasks;

namespace Polymarket.ClobClient.Tests.Signing
{
    public class Eip712Tests
    {
        private readonly string _privateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";

        [Fact]
        public void BuildClobEip712Signature_ShouldReturnCorrectSignature()
        {
            // From TS test: 
            // wallet = new Wallet(privateKey)
            // buildClobEip712Signature(wallet, Chain.AMOY, 10000000, 23)
            // Expected: "0xf62319a987514da40e57e2f4d7529f7bac38f0355bd88bb5adbb3768d80de6c1682518e0af677d5260366425f4361e7b70c25ae232aff0ab2331e2b164a1aedc1b"
            
            // TS: 10000000 (timestamp seems simplified in TS test? or is it? 10000000 seconds is 1970)
            // TS arg order: wallet, chainId, timestamp, nonce
            
            int chainId = 80002; // Amoy
            double timestamp = 10000000;
            int nonce = 23;

            var signature = SignerUtils.BuildClobEip712Signature(_privateKey, chainId, timestamp, nonce);

            Assert.NotNull(signature);
            Assert.NotEmpty(signature);
            Assert.Equal("0xf62319a987514da40e57e2f4d7529f7bac38f0355bd88bb5adbb3768d80de6c1682518e0af677d5260366425f4361e7b70c25ae232aff0ab2331e2b164a1aedc1b", signature);
        }
    }
}
