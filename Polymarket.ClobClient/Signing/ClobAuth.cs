using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Polymarket.ClobClient.Signing
{
    [Struct("ClobAuth")]
    public class ClobAuth
    {
        [Parameter("address", "address", 1)]
        public string Address { get; set; }

        [Parameter("string", "timestamp", 2)]
        public string Timestamp { get; set; }

        [Parameter("uint256", "nonce", 3)]
        public System.Numerics.BigInteger Nonce { get; set; }

        [Parameter("string", "message", 4)]
        public string Message { get; set; }
    }
}
