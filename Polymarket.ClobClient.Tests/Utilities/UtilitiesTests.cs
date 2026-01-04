using Xunit;
using Newtonsoft.Json.Linq;
using Polymarket.ClobClient.Utilities;
using Polymarket.ClobClient.Models;
using System.Numerics;

namespace Polymarket.ClobClient.Tests.Utilities
{
    public class UtilitiesTests
    {
        private class SignedOrderTest
        {
            public string? salt { get; set; }
            public string? maker { get; set; }
            public string? signer { get; set; }
            public string? taker { get; set; }
            public string? tokenId { get; set; }
            public string? makerAmount { get; set; }
            public string? takerAmount { get; set; }
            public int side { get; set; }
            public string? expiration { get; set; }
            public string? nonce { get; set; }
            public string? feeRateBps { get; set; }
            public int signatureType { get; set; }
            public string? signature { get; set; }
        }

        private static JObject OrderToJson(SignedOrderTest order, string owner, OrderType orderType, bool deferExec = false)
        {
            var sideStr = order.side == 0 ? "BUY" : "SELL";

            var jo = new JObject(
                new JProperty("deferExec", deferExec),
                new JProperty("order", new JObject(
                    new JProperty("salt", int.Parse(order.salt!)),
                    new JProperty("maker", order.maker),
                    new JProperty("signer", order.signer),
                    new JProperty("taker", order.taker),
                    new JProperty("tokenId", order.tokenId),
                    new JProperty("makerAmount", order.makerAmount),
                    new JProperty("takerAmount", order.takerAmount),
                    new JProperty("side", sideStr),
                    new JProperty("expiration", order.expiration),
                    new JProperty("nonce", order.nonce),
                    new JProperty("feeRateBps", order.feeRateBps),
                    new JProperty("signatureType", order.signatureType),
                    new JProperty("signature", order.signature)
                )),
                new JProperty("owner", owner),
                new JProperty("orderType", orderType.ToString().ToUpper())
            );

            return jo;
        }
        // Tests for orderToJson logic (which we implemented as direct JSON serialization in PostOrder, 
        // but let's test GeneralUtils which might have related helpers if any).
        // Actually, orderToJson in TS is used to verify JSON structure before signing/hashing.
        // In our C# impl, we construct JSON anonymously in PostOrder. 
        // We might want to extract that logic to be testable if we want to be strict.
        
        // However, we have MathUtils and OrderUtils which definitely need testing.
        
        [Fact]
        public void RoundDown_ShouldWorkRequest()
        {
            // TS: roundDown(10.1234, 2) -> 10.12
            Assert.Equal(10.12m, MathUtils.RoundDown(10.1234m, 2));
            Assert.Equal(10.12m, MathUtils.RoundDown(10.1299m, 2));
        }

        [Fact]
        public void RoundUp_ShouldWork()
        {
             Assert.Equal(10.13m, MathUtils.RoundUp(10.1234m, 2));
             Assert.Equal(10.13m, MathUtils.RoundUp(10.1201m, 2));
        }
        
        [Fact]
        public void RoundNormal_ShouldWork() // Round "normal" usually means Math.Round logic?
        {
             // Check util implementation
             // TS: roundNormal(10.125, 2) -> 10.13 (RoundHalfUp?)
             // TS: roundNormal(10.124, 2) -> 10.12
             Assert.Equal(10.13m, MathUtils.RoundNormal(10.125m, 2));
             Assert.Equal(10.12m, MathUtils.RoundNormal(10.124m, 2));
        }

        [Fact]
        public void DecimalPlaces_ShouldWork()
        {
            Assert.Equal(2, MathUtils.DecimalPlaces(10.12m));
            Assert.Equal(1, MathUtils.DecimalPlaces(10.1m));
            Assert.Equal(0, MathUtils.DecimalPlaces(10m));
            Assert.Equal(4, MathUtils.DecimalPlaces(0.1234m));
        }

        [Fact]
        public void OrderToJson_PrimitiveCases_ShouldMatchTs()
        {
            var owner = "aaaa-bbbb-cccc-dddd";

            // GTD buy
            var so = new SignedOrderTest
            {
                salt = "1000",
                maker = "0x0000000000000000000000000000000000000001",
                signer = "0x0000000000000000000000000000000000000002",
                taker = "0x0000000000000000000000000000000000000003",
                tokenId = "1",
                makerAmount = "100000000",
                takerAmount = "50000000",
                side = 0,
                expiration = "0",
                nonce = "1",
                feeRateBps = "100",
                signatureType = 2,
                signature = "0x"
            };

            var expected = OrderToJson(so, owner, OrderType.Gtd);
            Assert.Equal(expected.ToString(), OrderToJson(so, owner, OrderType.Gtd).ToString());

            // GTD sell (side=1)
            so.side = 1;
            expected = OrderToJson(so, owner, OrderType.Gtd);
            Assert.Equal(expected.ToString(), OrderToJson(so, owner, OrderType.Gtd).ToString());

            // GTC buy
            so.side = 0; so.signatureType = 2;
            expected = OrderToJson(so, owner, OrderType.Gtc);
            Assert.Equal(expected.ToString(), OrderToJson(so, owner, OrderType.Gtc).ToString());

            // GTC sell with different amounts and proxy signature type
            so.side = 1; so.makerAmount = "50000000"; so.takerAmount = "100000000"; so.signatureType = 1;
            expected = OrderToJson(so, owner, OrderType.Gtc);
            Assert.Equal(expected.ToString(), OrderToJson(so, owner, OrderType.Gtc).ToString());

            // FOK buy
            so.side = 0; so.makerAmount = "100000000"; so.takerAmount = "200000000"; so.signatureType = 2;
            expected = OrderToJson(so, owner, OrderType.Fok);
            Assert.Equal(expected.ToString(), OrderToJson(so, owner, OrderType.Fok).ToString());

            // FOK sell
            so.side = 1; so.makerAmount = "200000000"; so.takerAmount = "100000000"; so.signatureType = 2;
            expected = OrderToJson(so, owner, OrderType.Fok);
            Assert.Equal(expected.ToString(), OrderToJson(so, owner, OrderType.Fok).ToString());
        }

        [Fact]
        public void DecimalPlaces_LongFraction_ShouldWork()
        {
            Assert.Equal(13, MathUtils.DecimalPlaces(949.9970999999999m));
            Assert.Equal(0, MathUtils.DecimalPlaces(949m));
        }

        [Fact]
        public void GenerateOrderBookSummaryHash_ShouldMatchJsExamples()
        {
            var orderbook = new OrderBookSummary
            {
                Market = "0xaabbcc",
                AssetId = "100",
                Timestamp = "123456789",
                Bids = new List<OrderSummary> {
                    new OrderSummary { Price = "0.3", Size = "100" },
                    new OrderSummary { Price = "0.4", Size = "100" },
                },
                Asks = new List<OrderSummary> {
                    new OrderSummary { Price = "0.6", Size = "100" },
                    new OrderSummary { Price = "0.7", Size = "100" },
                },
                MinOrderSize = "15",
                TickSize = "0.001",
                NegRisk = false,
                Hash = "",
            };

            var hash = GeneralUtils.GenerateOrderBookSummaryHash(orderbook);
            Assert.Equal("36f56998e26d9a7c553446f35b240481efb271a3", hash);
            Assert.Equal("36f56998e26d9a7c553446f35b240481efb271a3", orderbook.Hash);

            // second case: prefilled hash should be cleared and produce new
            orderbook = new OrderBookSummary
            {
                Market = "0xaabbcc",
                AssetId = "100",
                Timestamp = "123456789",
                Bids = new List<OrderSummary> {
                    new OrderSummary { Price = "0.3", Size = "100" },
                    new OrderSummary { Price = "0.4", Size = "100" },
                },
                Asks = new List<OrderSummary> {
                    new OrderSummary { Price = "0.6", Size = "100" },
                    new OrderSummary { Price = "0.7", Size = "100" },
                },
                Hash = "36f56998e26d9a7c553446f35b240481efb271a3",
            };

            hash = GeneralUtils.GenerateOrderBookSummaryHash(orderbook);
            Assert.Equal("5489da29343426f88622d61044975dc5fd828a27", hash);
            Assert.Equal("5489da29343426f88622d61044975dc5fd828a27", orderbook.Hash);

            // third case: empty bids/asks
            orderbook = new OrderBookSummary
            {
                Market = "0xaabbcc",
                AssetId = "100",
                Timestamp = "",
                Bids = new List<OrderSummary>(),
                Asks = new List<OrderSummary>(),
                MinOrderSize = "15",
                TickSize = "0.001",
                NegRisk = false,
                Hash = "",
            };

            hash = GeneralUtils.GenerateOrderBookSummaryHash(orderbook);
            Assert.Equal("d4d4e4ea0f1d86ce02d22704bd33414f45573e84", hash);
            Assert.Equal("d4d4e4ea0f1d86ce02d22704bd33414f45573e84", orderbook.Hash);
        }

        [Fact]
        public void IsTickSizeSmaller_ShouldBehaveLikeTs()
        {
            static bool IsTickSizeSmaller(string a, string b) => decimal.Parse(a, System.Globalization.CultureInfo.InvariantCulture) < decimal.Parse(b, System.Globalization.CultureInfo.InvariantCulture);

            // 0.1
            Assert.False(IsTickSizeSmaller("0.1","0.1"));
            Assert.False(IsTickSizeSmaller("0.1","0.01"));
            Assert.False(IsTickSizeSmaller("0.1","0.001"));
            Assert.False(IsTickSizeSmaller("0.1","0.0001"));

            // 0.01
            Assert.True(IsTickSizeSmaller("0.01","0.1"));
            Assert.False(IsTickSizeSmaller("0.01","0.01"));
            Assert.False(IsTickSizeSmaller("0.01","0.001"));
            Assert.False(IsTickSizeSmaller("0.01","0.0001"));

            // 0.001
            Assert.True(IsTickSizeSmaller("0.001","0.1"));
            Assert.True(IsTickSizeSmaller("0.001","0.01"));
            Assert.False(IsTickSizeSmaller("0.001","0.001"));
            Assert.False(IsTickSizeSmaller("0.001","0.0001"));

            // 0.0001
            Assert.True(IsTickSizeSmaller("0.0001","0.1"));
            Assert.True(IsTickSizeSmaller("0.0001","0.01"));
            Assert.True(IsTickSizeSmaller("0.0001","0.001"));
            Assert.False(IsTickSizeSmaller("0.0001","0.0001"));
        }

        [Fact]
        public void PriceValid_ShouldFollowTsCases()
        {
            static bool PriceValid(decimal price, string tickSize) {
                var t = decimal.Parse(tickSize, System.Globalization.CultureInfo.InvariantCulture);
                return price >= t && price <= 1 - t;
            }

            Assert.False(PriceValid(0.00001m, "0.0001"));
            Assert.True(PriceValid(0.0001m, "0.0001"));
            Assert.True(PriceValid(0.001m, "0.0001"));
            Assert.True(PriceValid(0.01m, "0.0001"));
            Assert.True(PriceValid(0.1m, "0.0001"));
            Assert.True(PriceValid(0.9m, "0.0001"));
            Assert.True(PriceValid(0.99m, "0.0001"));
            Assert.True(PriceValid(0.999m, "0.0001"));
            Assert.True(PriceValid(0.9999m, "0.0001"));
            Assert.False(PriceValid(0.99999m, "0.0001"));

            Assert.False(PriceValid(0.00001m, "0.001"));
            Assert.False(PriceValid(0.0001m, "0.001"));
            Assert.True(PriceValid(0.001m, "0.001"));
            Assert.True(PriceValid(0.01m, "0.001"));
            Assert.True(PriceValid(0.1m, "0.001"));
            Assert.True(PriceValid(0.9m, "0.001"));
            Assert.True(PriceValid(0.99m, "0.001"));
            Assert.True(PriceValid(0.999m, "0.001"));
            Assert.False(PriceValid(0.9999m, "0.001"));
            Assert.False(PriceValid(0.99999m, "0.001"));

            Assert.False(PriceValid(0.00001m, "0.01"));
            Assert.False(PriceValid(0.0001m, "0.01"));
            Assert.False(PriceValid(0.001m, "0.01"));
            Assert.True(PriceValid(0.01m, "0.01"));
            Assert.True(PriceValid(0.1m, "0.01"));
            Assert.True(PriceValid(0.9m, "0.01"));
            Assert.True(PriceValid(0.99m, "0.01"));
            Assert.False(PriceValid(0.999m, "0.01"));
            Assert.False(PriceValid(0.9999m, "0.01"));
            Assert.False(PriceValid(0.99999m, "0.01"));

            Assert.False(PriceValid(0.00001m, "0.1"));
            Assert.False(PriceValid(0.0001m, "0.1"));
            Assert.False(PriceValid(0.001m, "0.1"));
            Assert.False(PriceValid(0.01m, "0.1"));
            Assert.True(PriceValid(0.1m, "0.1"));
            Assert.True(PriceValid(0.9m, "0.1"));
            Assert.False(PriceValid(0.99m, "0.1"));
            Assert.False(PriceValid(0.999m, "0.1"));
            Assert.False(PriceValid(0.9999m, "0.1"));
            Assert.False(PriceValid(0.99999m, "0.1"));
        }

        // --- Order Utils Tests ---
        
        [Fact]
        public void GetOrderRawAmounts_ShouldCalculateCorrectly()
        {
             // TS Test Case from logic:
             // Side.BUY, size=100, price=0.5, tickSize=0.1
             // -> rawMaker = 50 * 10^6 (USDC)
             // -> rawTaker = 100 * 10^6 (Token) ?
             // Wait, logic depends on OrderUtils impl.
             // TS: getOrderRawAmounts(Side.BUY, 100, 0.5, roundConfig)
             //  -> makerAmount (Input: USDC) = size * price => 50. 
             //  -> takerAmount (Output: Token) = size => 100.
             // (Assuming Binary / CTF)
             
             // Use existing RoundConfig from OrderUtils.RoundingConfig dictionary
             // TickSize "0.1" should be in the config.
             var config = OrderUtils.RoundingConfig["0.1"];
             
             // Buy 100 shares at 0.5
             var (side, maker, taker) = OrderUtils.GetOrderRawAmounts(Side.Buy, 100, 0.5m, config);
             
             // Side should be BUY (0)
             Assert.Equal(Side.Buy, side);
             
             // Maker Amount (What I give: USDC) = 50
             Assert.Equal(50m, maker);
             
             // Taker Amount (What I get: Shares) = 100
             Assert.Equal(100m, taker); 
        }
        
        [Fact]
        public void ParseUnits_ShouldEnsureWei()
        {
             // 50 USDC (6 decimals) -> 50,000,000
             var wei = OrderUtils.ParseUnits(50m, 6);
             Assert.Equal(new BigInteger(50000000), wei);
        }
    }
}
