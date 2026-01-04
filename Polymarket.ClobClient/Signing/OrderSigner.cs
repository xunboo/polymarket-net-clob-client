using System.Numerics;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;
using Nethereum.ABI.EIP712; // Added
using Polymarket.ClobClient.Constants;
using Polymarket.ClobClient.Models;
using Polymarket.ClobClient.Utilities;

namespace Polymarket.ClobClient.Signing
{
    public static class OrderSigner
    {
        private const string DomainName = "CTF Exchange";
        private const string Version = "1";

        public static string SignOrder(
            OrderStruct order, 
            string privateKey, 
            int chainId, 
            string exchangeAddress)
        {
            var signer = new Eip712TypedDataSigner();
            var key = new EthECKey(privateKey);

            var types = new Dictionary<string, object[]>
            {
                { "EIP712Domain", new object[]
                    {
                        new { name = "name", type = "string" },
                        new { name = "version", type = "string" },
                        new { name = "chainId", type = "uint256" },
                        new { name = "verifyingContract", type = "address" }
                    }
                },
                { "Order", new object[]
                    {
                        new { name = "salt", type = "uint256" },
                        new { name = "maker", type = "address" },
                        new { name = "signer", type = "address" },
                        new { name = "taker", type = "address" },
                        new { name = "tokenId", type = "uint256" },
                        new { name = "makerAmount", type = "uint256" },
                        new { name = "takerAmount", type = "uint256" },
                        new { name = "expiration", type = "uint256" },
                        new { name = "nonce", type = "uint256" },
                        new { name = "feeRateBps", type = "uint256" },
                        new { name = "side", type = "uint8" },
                        new { name = "signatureType", type = "uint8" }
                    }
                }
            };

            var domain = new
            {
                name = DomainName,
                version = Version,
                chainId = chainId,
                verifyingContract = exchangeAddress
            };

            var message = new
            {
                salt = order.Salt.ToString(), // Nethereum JSON usually expects strings for bigints or raw numbers?
                // Standard EIP712 JSON serialization: uint256 as Hex string or Decimal string?
                // Nethereum handles Decimal String or Hex String.
                // ToString() of BigInteger gives decimal string.
                maker = order.Maker,
                signer = order.Signer,
                taker = order.Taker,
                tokenId = order.TokenId.ToString(),
                makerAmount = order.MakerAmount.ToString(),
                takerAmount = order.TakerAmount.ToString(),
                expiration = order.Expiration.ToString(),
                nonce = order.Nonce.ToString(),
                feeRateBps = order.FeeRateBps.ToString(),
                side = order.Side,
                signatureType = order.SignatureType
            };

            var typedData = new
            {
                types = types,
                primaryType = "Order",
                domain = domain,
                message = message
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(typedData);
            return signer.SignTypedDataV4(json, key);
        }
    }
}
