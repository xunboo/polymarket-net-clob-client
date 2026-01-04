using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Polymarket.ClobClient.Signing
{
    [Struct("Order")]
    public class OrderStruct
    {
        [Parameter("uint256", "salt", 1)]
        public BigInteger Salt { get; set; }

        [Parameter("address", "maker", 2)]
        public string Maker { get; set; } = string.Empty;

        [Parameter("address", "signer", 3)]
        public string Signer { get; set; } = string.Empty;

        [Parameter("address", "taker", 4)]
        public string Taker { get; set; } = string.Empty;

        [Parameter("uint256", "tokenId", 5)]
        public BigInteger TokenId { get; set; }

        [Parameter("uint256", "makerAmount", 6)]
        public BigInteger MakerAmount { get; set; }

        [Parameter("uint256", "takerAmount", 7)]
        public BigInteger TakerAmount { get; set; }

        [Parameter("uint256", "expiration", 8)]
        public BigInteger Expiration { get; set; }

        [Parameter("uint256", "nonce", 9)]
        public BigInteger Nonce { get; set; }

        [Parameter("uint256", "feeRateBps", 10)]
        public BigInteger FeeRateBps { get; set; }

        [Parameter("uint8", "side", 11)]
        public int Side { get; set; }

        [Parameter("uint8", "signatureType", 12)]
        public int SignatureType { get; set; }
    }
}
