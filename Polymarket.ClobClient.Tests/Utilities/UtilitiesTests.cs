using Xunit;
using Polymarket.ClobClient.Utilities;
using Polymarket.ClobClient.Models;
using System.Numerics;

namespace Polymarket.ClobClient.Tests.Utilities
{
    public class UtilitiesTests
    {
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
