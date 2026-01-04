using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Polymarket.ClobClient.Models;

namespace Polymarket.ClobClient.Utilities
{
    public static class GeneralUtils
    {
        public static string GenerateOrderBookSummaryHash(OrderBookSummary orderbook)
        {
                // Reset hash to empty string before hashing, matching TS logic
                orderbook.Hash = "";

                // Build JSON with explicit property order to match JS `JSON.stringify` behavior
                // and only include fields when they were present in the original TS test objects.
                var jo = new Newtonsoft.Json.Linq.JObject();
                jo.Add("market", orderbook.Market);
                jo.Add("asset_id", orderbook.AssetId);
                jo.Add("timestamp", orderbook.Timestamp);

                // bids
                var bids = new Newtonsoft.Json.Linq.JArray();
                if (orderbook.Bids != null)
                {
                    foreach (var b in orderbook.Bids)
                    {
                        var o = new Newtonsoft.Json.Linq.JObject();
                        o.Add("price", b.Price);
                        o.Add("size", b.Size);
                        bids.Add(o);
                    }
                }
                jo.Add("bids", bids);

                // asks
                var asks = new Newtonsoft.Json.Linq.JArray();
                if (orderbook.Asks != null)
                {
                    foreach (var a in orderbook.Asks)
                    {
                        var o = new Newtonsoft.Json.Linq.JObject();
                        o.Add("price", a.Price);
                        o.Add("size", a.Size);
                        asks.Add(o);
                    }
                }
                jo.Add("asks", asks);

                // Only include optional fields when they are non-null/meaningfully set in a way
                // that matches the TypeScript tests: include min_order_size and tick_size when non-empty,
                // and include neg_risk when tick/min are present (the TS cases do this), otherwise omit.
                if (!string.IsNullOrEmpty(orderbook.MinOrderSize))
                {
                    jo.Add("min_order_size", orderbook.MinOrderSize);
                }

                if (!string.IsNullOrEmpty(orderbook.TickSize))
                {
                    jo.Add("tick_size", orderbook.TickSize);
                }

                if ((!string.IsNullOrEmpty(orderbook.MinOrderSize) || !string.IsNullOrEmpty(orderbook.TickSize)))
                {
                    jo.Add("neg_risk", orderbook.NegRisk);
                }

                // ensure hash property exists and is empty string during digest
                jo.Add("hash", "");

                var message = jo.ToString(Newtonsoft.Json.Formatting.None);
                var messageBytes = Encoding.UTF8.GetBytes(message);

                using var sha1 = SHA1.Create();
                var hashBytes = sha1.ComputeHash(messageBytes);

                var hex = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                orderbook.Hash = hex;
                return hex;
        }
    }
}
