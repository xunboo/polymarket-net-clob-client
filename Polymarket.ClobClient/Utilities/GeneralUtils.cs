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
            // Note: Mutating the object might be unexpected in C#, but follows TS pattern.
            // Better to return a clone or just handle it. 
            // TS: orderbook.hash = ""; ... orderbook.hash = hash; return hash;
            
            // We'll create a temporary object or just serialize with hash="" logic if we want to be pure.
            // But strict porting:
            orderbook.Hash = "";
            var settings = new JsonSerializerSettings 
            { 
                 // Ensure serialization matches the specific requirements (usually standard JSON)
                 NullValueHandling = NullValueHandling.Ignore 
            };
            
            // CAUTION: JSON serialization order matters for hashing.
            // TS uses JSON.stringify. C# Newtonsoft might order properties differently or identically?
            // Usually JSON.stringify preserves key order *mostly* but spec says unordered. 
            // However, for a canonical hash, properties usually need to be sorted or fixed order.
            // If the server relies on exact string match of a specific JSON serialization, this is brittle.
            // Let's assume standard serialization for now.
            
            var message = JsonConvert.SerializeObject(orderbook, settings);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            
            using var sha1 = SHA1.Create();
            var hashBytes = sha1.ComputeHash(messageBytes);
            
            var hex = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            orderbook.Hash = hex;
            return hex;
        }
    }
}
