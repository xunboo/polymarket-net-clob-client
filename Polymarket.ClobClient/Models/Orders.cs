using Newtonsoft.Json;

namespace Polymarket.ClobClient.Models
{
    public class UserOrder
    {
        [JsonProperty("tokenID")]
        public string TokenId { get; set; } = string.Empty;

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("size")]
        public decimal Size { get; set; }

        [JsonProperty("side")]
        public Side Side { get; set; }

        [JsonProperty("feeRateBps")]
        public int? FeeRateBps { get; set; }

        [JsonProperty("nonce")]
        public int? Nonce { get; set; }

        [JsonProperty("expiration")]
        public int? Expiration { get; set; }

        [JsonProperty("taker")]
        public string? Taker { get; set; }
    }

    public class UserMarketOrder
    {
        [JsonProperty("tokenID")]
        public string TokenId { get; set; } = string.Empty;

        [JsonProperty("price")]
        public decimal? Price { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("side")]
        public Side Side { get; set; }

        [JsonProperty("feeRateBps")]
        public int? FeeRateBps { get; set; }

        [JsonProperty("nonce")]
        public int? Nonce { get; set; }

        [JsonProperty("taker")]
        public string? Taker { get; set; }

        [JsonProperty("orderType")]
        public OrderType? OrderType { get; set; }
    }

    public class OrderSummary
    {
        [JsonProperty("price")]
        public string Price { get; set; } = string.Empty;

        [JsonProperty("size")]
        public string Size { get; set; } = string.Empty;
    }

    public class OrderBookSummary
    {
        [JsonProperty("market")]
        public string Market { get; set; } = string.Empty;

        [JsonProperty("asset_id")]
        public string AssetId { get; set; } = string.Empty;

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; } = string.Empty;

        [JsonProperty("bids")]
        public List<OrderSummary> Bids { get; set; } = new List<OrderSummary>();

        [JsonProperty("asks")]
        public List<OrderSummary> Asks { get; set; } = new List<OrderSummary>();

        [JsonProperty("min_order_size")]
        public string MinOrderSize { get; set; } = string.Empty;

        [JsonProperty("tick_size")]
        public string TickSize { get; set; } = string.Empty;

        [JsonProperty("neg_risk")]
        public bool NegRisk { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; } = string.Empty;
    }

    public class RoundConfig
    {
        public int Price { get; set; }
        public int Size { get; set; }
        public int Amount { get; set; }
    }
}
