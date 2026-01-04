using Xunit;
using Polymarket.ClobClient.Models;
using Polymarket.ClobClient.Utilities;

namespace Polymarket.ClobClient.Tests.OrderBuilder
{
    public class HelpersTests
    {
        // These tests verify the order raw amount calculations
        // which are critical for correct order construction.
        
        [Theory]
        [InlineData(0.5, 100, 50, 100)]     // Buy 100 at 0.5 -> pay 50 USDC, get 100 tokens
        [InlineData(0.8, 100, 80, 100)]     // Buy 100 at 0.8 -> pay 80 USDC, get 100 tokens
        [InlineData(0.1, 50, 5, 50)]        // Buy 50 at 0.1 -> pay 5 USDC, get 50 tokens
        public void GetOrderRawAmounts_Buy_ShouldCalculateCorrectly(
            decimal price, decimal size, decimal expectedMaker, decimal expectedTaker)
        {
            var config = OrderUtils.RoundingConfig["0.1"];
            var (side, rawMaker, rawTaker) = OrderUtils.GetOrderRawAmounts(Side.Buy, size, price, config);

            Assert.Equal(Side.Buy, side);
            Assert.Equal(expectedMaker, rawMaker);
            Assert.Equal(expectedTaker, rawTaker);
        }

        [Theory]
        [InlineData(0.5, 100, 100, 50)]     // Sell 100 at 0.5 -> give 100 tokens, get 50 USDC
        [InlineData(0.8, 100, 100, 80)]     // Sell 100 at 0.8 -> give 100 tokens, get 80 USDC  
        [InlineData(0.1, 50, 50, 5)]        // Sell 50 at 0.1 -> give 50 tokens, get 5 USDC
        public void GetOrderRawAmounts_Sell_ShouldCalculateCorrectly(
            decimal price, decimal size, decimal expectedMaker, decimal expectedTaker)
        {
            var config = OrderUtils.RoundingConfig["0.1"];
            var (side, rawMaker, rawTaker) = OrderUtils.GetOrderRawAmounts(Side.Sell, size, price, config);

            Assert.Equal(Side.Sell, side);
            Assert.Equal(expectedMaker, rawMaker);
            Assert.Equal(expectedTaker, rawTaker);
        }

        [Fact]
        public void RoundingConfig_ShouldContainExpectedTickSizes()
        {
            Assert.True(OrderUtils.RoundingConfig.ContainsKey("0.1"));
            Assert.True(OrderUtils.RoundingConfig.ContainsKey("0.01"));
            Assert.True(OrderUtils.RoundingConfig.ContainsKey("0.001"));
            Assert.True(OrderUtils.RoundingConfig.ContainsKey("0.0001"));
        }
    }
}
