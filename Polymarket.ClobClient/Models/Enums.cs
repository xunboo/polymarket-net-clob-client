using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Polymarket.ClobClient.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Side
    {
        [EnumMember(Value = "BUY")]
        Buy,
        [EnumMember(Value = "SELL")]
        Sell
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderType
    {
        [EnumMember(Value = "GTC")]
        Gtc, // Good Till Cancelled
        [EnumMember(Value = "FOK")]
        Fok, // Fill Or Kill
        [EnumMember(Value = "GTD")]
        Gtd, // Good Till Date
        [EnumMember(Value = "FAK")]
        Fak, // Fill And Kill
    }

    public enum Chain
    {
        Polygon = 137,
        Amoy = 80002
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum AssetType
    {
        [EnumMember(Value = "COLLATERAL")]
        Collateral,
        [EnumMember(Value = "CONDITIONAL")]
        Conditional
    }
}
