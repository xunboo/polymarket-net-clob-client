using Xunit;
using Polymarket.ClobClient.Models;
using Polymarket.ClobClient.Utilities;
using Polymarket.ClobClient.Signing;
using Polymarket.ClobClient.Constants;
using System.Numerics;
using Nethereum.Signer;

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

        [Fact]
        public void CalculateBuyMarketPrice_FOK_Empty_ShouldThrow()
        {
            Assert.Throws<Exception>(() => OrderUtils.CalculateBuyMarketPrice(new List<OrderSummary>(), 100, OrderType.Fok));
        }

        [Fact]
        public void CalculateBuyMarketPrice_FOK_NotEnough_ShouldThrow()
        {
            var positions = new List<OrderSummary>
            {
                new OrderSummary { Price = "0.5", Size = "100" },
                new OrderSummary { Price = "0.4", Size = "100" }
            };
            Assert.Throws<Exception>(() => OrderUtils.CalculateBuyMarketPrice(positions, 100, OrderType.Fok));
        }

        [Fact]
        public void CalculateBuyMarketPrice_FOK_Ok()
        {
            var positions = new List<OrderSummary>
            {
                new OrderSummary { Price = "0.5", Size = "100" },
                new OrderSummary { Price = "0.4", Size = "100" },
                new OrderSummary { Price = "0.3", Size = "100" }
            };
            Assert.Equal(0.5m, OrderUtils.CalculateBuyMarketPrice(positions, 100, OrderType.Fok));

            positions = new List<OrderSummary>
            {
                new OrderSummary { Price = "0.5", Size = "100" },
                new OrderSummary { Price = "0.4", Size = "200" },
                new OrderSummary { Price = "0.3", Size = "100" }
            };
            Assert.Equal(0.4m, OrderUtils.CalculateBuyMarketPrice(positions, 100, OrderType.Fok));

            positions = new List<OrderSummary>
            {
                new OrderSummary { Price = "0.5", Size = "120" },
                new OrderSummary { Price = "0.4", Size = "100" },
                new OrderSummary { Price = "0.3", Size = "100" }
            };
            Assert.Equal(0.5m, OrderUtils.CalculateBuyMarketPrice(positions, 100, OrderType.Fok));

            positions = new List<OrderSummary>
            {
                new OrderSummary { Price = "0.5", Size = "200" },
                new OrderSummary { Price = "0.4", Size = "100" },
                new OrderSummary { Price = "0.3", Size = "100" }
            };
            Assert.Equal(0.5m, OrderUtils.CalculateBuyMarketPrice(positions, 100, OrderType.Fok));
        }

        [Fact]
        public void CalculateSellMarketPrice_FOK_Empty_ShouldThrow()
        {
            Assert.Throws<Exception>(() => OrderUtils.CalculateSellMarketPrice(new List<OrderSummary>(), 100, OrderType.Fok));
        }

        [Fact]
        public void CalculateSellMarketPrice_FOK_NotEnough_ShouldThrow()
        {
            var positions = new List<OrderSummary>
            {
                new OrderSummary { Price = "0.4", Size = "10" },
                new OrderSummary { Price = "0.5", Size = "10" }
            };
            Assert.Throws<Exception>(() => OrderUtils.CalculateSellMarketPrice(positions, 100, OrderType.Fok));
        }

        [Fact]
        public void CalculateSellMarketPrice_FOK_Ok()
        {
            var positions = new List<OrderSummary>
            {
                new OrderSummary { Price = "0.3", Size = "100" },
                new OrderSummary { Price = "0.4", Size = "100" },
                new OrderSummary { Price = "0.5", Size = "100" }
            };
            Assert.Equal(0.5m, OrderUtils.CalculateSellMarketPrice(positions, 100, OrderType.Fok));

            Assert.Equal(0.3m, OrderUtils.CalculateSellMarketPrice(positions, 300, OrderType.Fok));

            positions = new List<OrderSummary>
            {
                new OrderSummary { Price = "0.3", Size = "100" },
                new OrderSummary { Price = "0.4", Size = "200" },
                new OrderSummary { Price = "0.5", Size = "100" }
            };
            Assert.Equal(0.4m, OrderUtils.CalculateSellMarketPrice(positions, 300, OrderType.Fok));

            positions = new List<OrderSummary>
            {
                new OrderSummary { Price = "0.3", Size = "334" },
                new OrderSummary { Price = "0.4", Size = "100" },
                new OrderSummary { Price = "0.5", Size = "1000" }
            };
            Assert.Equal(0.5m, OrderUtils.CalculateSellMarketPrice(positions, 600, OrderType.Fok));
        }

        [Fact]
        public void CalculateBuyMarketPrice_FAK_Behavior()
        {
            Assert.Throws<Exception>(() => OrderUtils.CalculateBuyMarketPrice(new List<OrderSummary>(), 100, OrderType.Fak));

            var positions = new List<OrderSummary>
            {
                new OrderSummary { Price = "0.5", Size = "100" },
                new OrderSummary { Price = "0.4", Size = "100" }
            };
            Assert.Equal(0.5m, OrderUtils.CalculateBuyMarketPrice(positions, 100, OrderType.Fak));

            positions = new List<OrderSummary>
            {
                new OrderSummary { Price = "0.6", Size = "100" },
                new OrderSummary { Price = "0.55", Size = "100" },
                new OrderSummary { Price = "0.5", Size = "100" }
            };
            Assert.Equal(0.6m, OrderUtils.CalculateBuyMarketPrice(positions, 200, OrderType.Fak));

            positions = new List<OrderSummary>
            {
                new OrderSummary { Price = "0.5", Size = "100" },
                new OrderSummary { Price = "0.4", Size = "200" },
                new OrderSummary { Price = "0.3", Size = "100" }
            };
            Assert.Equal(0.4m, OrderUtils.CalculateBuyMarketPrice(positions, 100, OrderType.Fak));
        }

        [Fact]
        public void CalculateSellMarketPrice_FAK_Behavior()
        {
            Assert.Throws<Exception>(() => OrderUtils.CalculateSellMarketPrice(new List<OrderSummary>(), 100, OrderType.Fak));

            var positions = new List<OrderSummary>
            {
                new OrderSummary { Price = "0.4", Size = "10" },
                new OrderSummary { Price = "0.5", Size = "10" }
            };
            Assert.Equal(0.4m, OrderUtils.CalculateSellMarketPrice(positions, 100, OrderType.Fak));

            var positionsOk = new List<OrderSummary>
            {
                new OrderSummary { Price = "0.3", Size = "100" },
                new OrderSummary { Price = "0.4", Size = "100" },
                new OrderSummary { Price = "0.5", Size = "100" }
            };
            Assert.Equal(0.5m, OrderUtils.CalculateSellMarketPrice(positionsOk, 100, OrderType.Fak));
            Assert.Equal(0.3m, OrderUtils.CalculateSellMarketPrice(positionsOk, 300, OrderType.Fak));

            positionsOk = new List<OrderSummary>
            {
                new OrderSummary { Price = "0.3", Size = "100" },
                new OrderSummary { Price = "0.4", Size = "200" },
                new OrderSummary { Price = "0.5", Size = "100" }
            };
            Assert.Equal(0.4m, OrderUtils.CalculateSellMarketPrice(positionsOk, 300, OrderType.Fak));

            positionsOk = new List<OrderSummary>
            {
                new OrderSummary { Price = "0.3", Size = "334" },
                new OrderSummary { Price = "0.4", Size = "100" },
                new OrderSummary { Price = "0.5", Size = "1000" }
            };
            Assert.Equal(0.5m, OrderUtils.CalculateSellMarketPrice(positionsOk, 600, OrderType.Fak));
        }

        // --- Ported buildOrderCreationArgs checks ---
        [Fact]
        public void BuildOrderCreationArgs_Buy_Tick_0_1()
        {
            var roundConfig = OrderUtils.RoundingConfig["0.1"];
            var res = OrderUtils.GetOrderRawAmounts(Side.Buy, 21.04m, 0.5m, roundConfig);
            Assert.Equal(Side.Buy, res.Side);

            var makerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals).ToString();
            var takerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals).ToString();

            Assert.Equal("10520000", makerAmount);
            Assert.Equal("21040000", takerAmount);
        }

        [Fact]
        public void BuildOrderCreationArgs_Buy_Tick_0_01()
        {
            var roundConfig = OrderUtils.RoundingConfig["0.01"];
            var res = OrderUtils.GetOrderRawAmounts(Side.Buy, 21.04m, 0.56m, roundConfig);
            var makerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals).ToString();
            var takerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals).ToString();

            Assert.Equal("11782400", makerAmount);
            Assert.Equal("21040000", takerAmount);
        }

        [Fact]
        public void BuildOrderCreationArgs_Sell_Tick_0_1()
        {
            var roundConfig = OrderUtils.RoundingConfig["0.1"];
            var res = OrderUtils.GetOrderRawAmounts(Side.Sell, 21.04m, 0.5m, roundConfig);
            var makerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals).ToString();
            var takerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals).ToString();

            Assert.Equal("21040000", makerAmount);
            Assert.Equal("10520000", takerAmount);
        }

        // --- Market order creation args ---
        [Fact]
        public void BuildMarketOrderCreationArgs_Buy_Tick_0_1()
        {
            var roundConfig = OrderUtils.RoundingConfig["0.1"];
            var res = OrderUtils.GetMarketOrderRawAmounts(Side.Buy, 100m, 0.5m, roundConfig);
            var makerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals).ToString();
            var takerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals).ToString();

            Assert.Equal("100000000", makerAmount);
            Assert.Equal("200000000", takerAmount);
        }

        [Fact]
        public void BuildMarketOrderCreationArgs_Sell_Tick_0_01()
        {
            var roundConfig = OrderUtils.RoundingConfig["0.01"];
            var res = OrderUtils.GetMarketOrderRawAmounts(Side.Sell, 100m, 0.56m, roundConfig);
            var makerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals).ToString();
            var takerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals).ToString();

            Assert.Equal("100000000", makerAmount);
            Assert.Equal("56000000", takerAmount);
        }

        // --- Ported looped validations from TS helpers.test.ts ---
        [Fact]
        public void GetOrderRawAmounts_Buy_Tick_0_1_Loop()
        {
            var deltaPrice = 0.1m;
            var deltaSize = 0.01m;
            var roundConfig = OrderUtils.RoundingConfig["0.1"];

            for (var size = 0.01m; size <= 1000m; size += deltaSize)
            {
                for (var price = 0.1m; price <= 1m; price += deltaPrice)
                {
                    var (side, rawMakerAmt, rawTakerAmt) = OrderUtils.GetOrderRawAmounts(Side.Buy, size, price, roundConfig);
                    Assert.True(MathUtils.DecimalPlaces(rawMakerAmt) <= 3);
                    Assert.True(MathUtils.DecimalPlaces(rawTakerAmt) <= 2);
                    Assert.True(MathUtils.RoundNormal(rawMakerAmt / rawTakerAmt, 2) >= MathUtils.RoundNormal(price, 2));
                }
            }
        }

        [Fact]
        public void GetOrderRawAmounts_Buy_Tick_0_01_Loop()
        {
            var deltaPrice = 0.01m;
            var deltaSize = 0.01m;
            var roundConfig = OrderUtils.RoundingConfig["0.01"];

            for (var size = 0.01m; size <= 100m; size += deltaSize)
            {
                for (var price = 0.01m; price <= 1m; price += deltaPrice)
                {
                    var (side, rawMakerAmt, rawTakerAmt) = OrderUtils.GetOrderRawAmounts(Side.Buy, size, price, roundConfig);
                    Assert.True(MathUtils.DecimalPlaces(rawMakerAmt) <= 4);
                    Assert.True(MathUtils.DecimalPlaces(rawTakerAmt) <= 2);
                    Assert.True(MathUtils.RoundNormal(rawMakerAmt / rawTakerAmt, 4) >= MathUtils.RoundNormal(price, 4));
                }
            }
        }

        [Fact]
        public void GetOrderRawAmounts_Buy_Tick_0_001_Loop()
        {
            var deltaPrice = 0.001m;
            var deltaSize = 0.01m;
            var roundConfig = OrderUtils.RoundingConfig["0.001"];

            for (var size = 0.01m; size <= 10m; size += deltaSize)
            {
                for (var price = 0.001m; price <= 1m; price += deltaPrice)
                {
                    var (side, rawMakerAmt, rawTakerAmt) = OrderUtils.GetOrderRawAmounts(Side.Buy, size, price, roundConfig);
                    Assert.True(MathUtils.DecimalPlaces(rawMakerAmt) <= 5);
                    Assert.True(MathUtils.DecimalPlaces(rawTakerAmt) <= 2);
                    Assert.True(MathUtils.RoundNormal(rawMakerAmt / rawTakerAmt, 6) >= MathUtils.RoundNormal(price, 6));
                }
            }
        }

        [Fact]
        public void GetOrderRawAmounts_Buy_Tick_0_0001_Loop()
        {
            var deltaPrice = 0.0001m;
            var deltaSize = 0.01m;
            var roundConfig = OrderUtils.RoundingConfig["0.0001"];

            for (var size = 0.01m; size <= 1m; size += deltaSize)
            {
                for (var price = 0.0001m; price <= 1m; price += deltaPrice)
                {
                    var p = MathUtils.RoundNormal(price, 8);
                    var (side, rawMakerAmt, rawTakerAmt) = OrderUtils.GetOrderRawAmounts(Side.Buy, size, p, roundConfig);
                    Assert.True(MathUtils.DecimalPlaces(rawMakerAmt) <= 6);
                    Assert.True(MathUtils.DecimalPlaces(rawTakerAmt) <= 2);
                    Assert.True(MathUtils.RoundNormal(rawMakerAmt / rawTakerAmt, 8) >= MathUtils.RoundNormal(price, 8));
                }
            }
        }

        [Fact]
        public void GetOrderRawAmounts_Sell_Loops_AllTicks()
        {
            // 0.1
            var roundConfig = OrderUtils.RoundingConfig["0.1"];
            for (var size = 0.01m; size <= 1000m; size += 0.01m)
            {
                for (var price = 0.1m; price <= 1m; price += 0.1m)
                {
                    var (side, rawMakerAmt, rawTakerAmt) = OrderUtils.GetOrderRawAmounts(Side.Sell, size, price, roundConfig);
                    Assert.True(MathUtils.DecimalPlaces(rawMakerAmt) <= 2);
                    Assert.True(MathUtils.DecimalPlaces(rawTakerAmt) <= 3);
                    Assert.True(MathUtils.RoundNormal(rawTakerAmt / rawMakerAmt, 2) <= MathUtils.RoundNormal(price, 2));
                }
            }

            // 0.01
            roundConfig = OrderUtils.RoundingConfig["0.01"];
            for (var size = 0.01m; size <= 100m; size += 0.01m)
            {
                for (var price = 0.01m; price <= 1m; price += 0.01m)
                {
                    var (side, rawMakerAmt, rawTakerAmt) = OrderUtils.GetOrderRawAmounts(Side.Sell, size, price, roundConfig);
                    Assert.True(MathUtils.DecimalPlaces(rawMakerAmt) <= 2);
                    Assert.True(MathUtils.DecimalPlaces(rawTakerAmt) <= 4);
                    Assert.True(MathUtils.RoundNormal(rawTakerAmt / rawMakerAmt, 4) <= MathUtils.RoundNormal(price, 4));
                }
            }

            // 0.001
            roundConfig = OrderUtils.RoundingConfig["0.001"];
            for (var size = 0.01m; size <= 10m; size += 0.01m)
            {
                for (var price = 0.001m; price <= 1m; price += 0.001m)
                {
                    var (side, rawMakerAmt, rawTakerAmt) = OrderUtils.GetOrderRawAmounts(Side.Sell, size, price, roundConfig);
                    Assert.True(MathUtils.DecimalPlaces(rawMakerAmt) <= 2);
                    Assert.True(MathUtils.DecimalPlaces(rawTakerAmt) <= 5);
                    Assert.True(MathUtils.RoundNormal(rawTakerAmt / rawMakerAmt, 6) <= MathUtils.RoundNormal(price, 6));
                }
            }

            // 0.0001
            roundConfig = OrderUtils.RoundingConfig["0.0001"];
            for (var size = 0.01m; size <= 1m; size += 0.01m)
            {
                for (var price = 0.0001m; price <= 1m; price += 0.0001m)
                {
                    var (side, rawMakerAmt, rawTakerAmt) = OrderUtils.GetOrderRawAmounts(Side.Sell, size, price, roundConfig);
                    Assert.True(MathUtils.DecimalPlaces(rawMakerAmt) <= 2);
                    Assert.True(MathUtils.DecimalPlaces(rawTakerAmt) <= 6);
                    Assert.True(MathUtils.RoundNormal(rawTakerAmt / rawMakerAmt, 8) <= MathUtils.RoundNormal(price, 8));
                }
            }
        }

        // --- Market order raw amounts loops ---
        [Fact]
        public void GetMarketOrderRawAmounts_Buy_Loops_AllTicks()
        {
            // 0.1
            var roundConfig = OrderUtils.RoundingConfig["0.1"];
            for (var size = 0.01m; size <= 1000m; size += 0.01m)
            {
                for (var price = 0.1m; price <= 1m; price += 0.1m)
                {
                    var p = MathUtils.RoundNormal(price, 8);
                    var res = OrderUtils.GetMarketOrderRawAmounts(Side.Buy, size, p, roundConfig);
                    Assert.True(MathUtils.DecimalPlaces(res.RawMakerAmt) <= 2);
                    Assert.True(MathUtils.DecimalPlaces(res.RawTakerAmt) <= 3);
                    Assert.True(MathUtils.RoundNormal(res.RawMakerAmt / res.RawTakerAmt, 2) >= MathUtils.RoundNormal(price, 2));
                }
            }

            // 0.01
            roundConfig = OrderUtils.RoundingConfig["0.01"];
            for (var size = 0.01m; size <= 100m; size += 0.01m)
            {
                for (var price = 0.01m; price <= 1m; price += 0.01m)
                {
                    var p = MathUtils.RoundNormal(price, 8);
                    var res = OrderUtils.GetMarketOrderRawAmounts(Side.Buy, size, p, roundConfig);
                    Assert.True(MathUtils.DecimalPlaces(res.RawMakerAmt) <= 2);
                    Assert.True(MathUtils.DecimalPlaces(res.RawTakerAmt) <= 4);
                    Assert.True(MathUtils.RoundNormal(res.RawMakerAmt / res.RawTakerAmt, 4) >= MathUtils.RoundNormal(price, 4));
                }
            }

            // 0.001
            roundConfig = OrderUtils.RoundingConfig["0.001"];
            for (var size = 0.01m; size <= 10m; size += 0.01m)
            {
                for (var price = 0.001m; price <= 1m; price += 0.001m)
                {
                    var p = MathUtils.RoundNormal(price, 8);
                    var res = OrderUtils.GetMarketOrderRawAmounts(Side.Buy, size, p, roundConfig);
                    Assert.True(MathUtils.DecimalPlaces(res.RawMakerAmt) <= 2);
                    Assert.True(MathUtils.DecimalPlaces(res.RawTakerAmt) <= 5);
                    Assert.True(MathUtils.RoundNormal(res.RawMakerAmt / res.RawTakerAmt, 6) >= MathUtils.RoundNormal(price, 6));
                }
            }

            // 0.0001
            roundConfig = OrderUtils.RoundingConfig["0.0001"];
            for (var size = 0.01m; size <= 1m; size += 0.01m)
            {
                for (var price = 0.0001m; price <= 1m; price += 0.0001m)
                {
                    var p = MathUtils.RoundNormal(price, 8);
                    var res = OrderUtils.GetMarketOrderRawAmounts(Side.Buy, size, p, roundConfig);
                    Assert.True(MathUtils.DecimalPlaces(res.RawMakerAmt) <= 2);
                    Assert.True(MathUtils.DecimalPlaces(res.RawTakerAmt) <= 6);
                    Assert.True(MathUtils.RoundNormal(res.RawMakerAmt / res.RawTakerAmt, 8) >= MathUtils.RoundNormal(price, 8));
                }
            }
        }

        // --- createOrder / signing smoke tests ---
        [Fact]
        public void CreateOrder_Buy_Signature_NotEmpty()
        {
            const string privateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";
            var key = new EthECKey(privateKey);
            var address = key.GetPublicAddress();
            var chainId = (int)Models.Chain.Amoy;
            var exchange = Contracts.GetContractConfig(chainId).Exchange;

            var roundConfig = OrderUtils.RoundingConfig["0.1"];
            var res = OrderUtils.GetOrderRawAmounts(Side.Buy, 21.04m, 0.5m, roundConfig);
            var makerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals);
            var takerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals);

            var orderStruct = new OrderStruct
            {
                Salt = new BigInteger(123),
                Maker = address,
                Signer = address,
                Taker = "0x0000000000000000000000000000000000000000",
                TokenId = new BigInteger(123),
                MakerAmount = makerAmount,
                TakerAmount = takerAmount,
                Expiration = new BigInteger(50000),
                Nonce = new BigInteger(123),
                FeeRateBps = new BigInteger(111),
                Side = 0,
                SignatureType = 0
            };

            var signature = OrderSigner.SignOrder(orderStruct, privateKey, chainId, exchange);
            Assert.False(string.IsNullOrEmpty(signature));
            Assert.Equal("10520000", OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals).ToString());
            Assert.Equal("21040000", OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals).ToString());
        }

        [Fact]
        public void CreateMarketOrder_Buy_Signature_NotEmpty()
        {
            const string privateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";
            var key = new EthECKey(privateKey);
            var address = key.GetPublicAddress();
            var chainId = (int)Models.Chain.Amoy;
            var exchange = Contracts.GetContractConfig(chainId).Exchange;

            var roundConfig = OrderUtils.RoundingConfig["0.01"];
            var res = OrderUtils.GetMarketOrderRawAmounts(Side.Buy, 100m, 0.56m, roundConfig);
            var makerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals);
            var takerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals);

            var orderStruct = new OrderStruct
            {
                Salt = new BigInteger(123),
                Maker = address,
                Signer = address,
                Taker = "0x0000000000000000000000000000000000000000",
                TokenId = new BigInteger(123),
                MakerAmount = makerAmount,
                TakerAmount = takerAmount,
                Expiration = BigInteger.Zero,
                Nonce = new BigInteger(123),
                FeeRateBps = new BigInteger(111),
                Side = 0,
                SignatureType = 0
            };

            var signature = OrderSigner.SignOrder(orderStruct, privateKey, chainId, exchange);
            Assert.False(string.IsNullOrEmpty(signature));
            Assert.Equal("100000000", OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals).ToString());
            Assert.Equal("178571400", OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals).ToString());
        }

        // --- Deterministic createOrder tests (limit orders) ported from TS ---
        [Fact]
        public void CreateOrder_CTF_Buy_Tick_0_1()
        {
            const string privateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";
            var key = new Nethereum.Signer.EthECKey(privateKey);
            var address = key.GetPublicAddress();
            var chainId = (int)Models.Chain.Amoy;
            var exchange = Contracts.GetContractConfig(chainId).Exchange;

            var roundConfig = OrderUtils.RoundingConfig["0.1"];
            var res = OrderUtils.GetOrderRawAmounts(Side.Buy, 21.04m, 0.5m, roundConfig);
            var makerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals).ToString();
            var takerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals).ToString();

            var orderStruct = new OrderStruct
            {
                Salt = new BigInteger(123),
                Maker = address,
                Signer = address,
                Taker = "0x0000000000000000000000000000000000000000",
                TokenId = new BigInteger(123),
                MakerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals),
                TakerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals),
                Expiration = new BigInteger(50000),
                Nonce = new BigInteger(123),
                FeeRateBps = new BigInteger(111),
                Side = 0,
                SignatureType = 0
            };

            var signature = OrderSigner.SignOrder(orderStruct, privateKey, chainId, exchange);
            Assert.False(string.IsNullOrEmpty(signature));
            Assert.Equal("10520000", makerAmount);
            Assert.Equal("21040000", takerAmount);
        }

        [Fact]
        public void CreateOrder_CTF_Buy_Tick_0_01()
        {
            const string privateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";
            var key = new Nethereum.Signer.EthECKey(privateKey);
            var address = key.GetPublicAddress();
            var chainId = (int)Models.Chain.Amoy;
            var exchange = Contracts.GetContractConfig(chainId).Exchange;

            var roundConfig = OrderUtils.RoundingConfig["0.01"];
            var res = OrderUtils.GetOrderRawAmounts(Side.Buy, 21.04m, 0.56m, roundConfig);
            var makerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals).ToString();
            var takerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals).ToString();

            var orderStruct = new OrderStruct
            {
                Salt = new BigInteger(123), Maker = address, Signer = address,
                Taker = "0x0000000000000000000000000000000000000000",
                TokenId = new BigInteger(123), MakerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals),
                TakerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals), Expiration = new BigInteger(50000),
                Nonce = new BigInteger(123), FeeRateBps = new BigInteger(111), Side = 0, SignatureType = 0
            };

            var signature = OrderSigner.SignOrder(orderStruct, privateKey, chainId, exchange);
            Assert.False(string.IsNullOrEmpty(signature));
            Assert.Equal("11782400", makerAmount);
            Assert.Equal("21040000", takerAmount);
        }

        [Fact]
        public void CreateOrder_CTF_Buy_Tick_0_001()
        {
            const string privateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";
            var key = new Nethereum.Signer.EthECKey(privateKey);
            var address = key.GetPublicAddress();
            var chainId = (int)Models.Chain.Amoy;
            var exchange = Contracts.GetContractConfig(chainId).Exchange;

            var roundConfig = OrderUtils.RoundingConfig["0.001"];
            var res = OrderUtils.GetOrderRawAmounts(Side.Buy, 21.04m, 0.056m, roundConfig);
            var makerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals).ToString();
            var takerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals).ToString();

            var orderStruct = new OrderStruct
            {
                Salt = new BigInteger(123), Maker = address, Signer = address,
                Taker = "0x0000000000000000000000000000000000000000",
                TokenId = new BigInteger(123), MakerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals),
                TakerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals), Expiration = new BigInteger(50000),
                Nonce = new BigInteger(123), FeeRateBps = new BigInteger(111), Side = 0, SignatureType = 0
            };

            var signature = OrderSigner.SignOrder(orderStruct, privateKey, chainId, exchange);
            Assert.False(string.IsNullOrEmpty(signature));
            Assert.Equal("1178240", makerAmount);
            Assert.Equal("21040000", takerAmount);
        }

        [Fact]
        public void CreateOrder_CTF_Buy_Tick_0_0001()
        {
            const string privateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";
            var key = new Nethereum.Signer.EthECKey(privateKey);
            var address = key.GetPublicAddress();
            var chainId = (int)Models.Chain.Amoy;
            var exchange = Contracts.GetContractConfig(chainId).Exchange;

            var roundConfig = OrderUtils.RoundingConfig["0.0001"];
            var res = OrderUtils.GetOrderRawAmounts(Side.Buy, 21.04m, 0.0056m, roundConfig);
            var makerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals).ToString();
            var takerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals).ToString();

            var orderStruct = new OrderStruct
            {
                Salt = new BigInteger(123), Maker = address, Signer = address,
                Taker = "0x0000000000000000000000000000000000000000",
                TokenId = new BigInteger(123), MakerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals),
                TakerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals), Expiration = new BigInteger(50000),
                Nonce = new BigInteger(123), FeeRateBps = new BigInteger(111), Side = 0, SignatureType = 0
            };

            var signature = OrderSigner.SignOrder(orderStruct, privateKey, chainId, exchange);
            Assert.False(string.IsNullOrEmpty(signature));
            Assert.Equal("117824", makerAmount);
            Assert.Equal("21040000", takerAmount);
        }

        [Fact]
        public void CreateOrder_CTF_Sell_Tick_0_1()
        {
            const string privateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";
            var key = new Nethereum.Signer.EthECKey(privateKey);
            var address = key.GetPublicAddress();
            var chainId = (int)Models.Chain.Amoy;
            var exchange = Contracts.GetContractConfig(chainId).Exchange;

            var roundConfig = OrderUtils.RoundingConfig["0.1"];
            var res = OrderUtils.GetOrderRawAmounts(Side.Sell, 21.04m, 0.5m, roundConfig);
            var makerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals).ToString();
            var takerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals).ToString();

            var orderStruct = new OrderStruct
            {
                Salt = BigInteger.Zero, Maker = address, Signer = address,
                Taker = "0x0000000000000000000000000000000000000000",
                TokenId = new BigInteger(5), MakerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals),
                TakerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals), Expiration = BigInteger.Zero,
                Nonce = BigInteger.Zero, FeeRateBps = BigInteger.Zero, Side = 1, SignatureType = 2
            };

            var signature = OrderSigner.SignOrder(orderStruct, privateKey, chainId, exchange);
            Assert.False(string.IsNullOrEmpty(signature));
            Assert.Equal("21040000", makerAmount);
            Assert.Equal("10520000", takerAmount);
        }

        [Fact]
        public void CreateOrder_CTF_Sell_Tick_0_01()
        {
            const string privateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";
            var key = new Nethereum.Signer.EthECKey(privateKey);
            var address = key.GetPublicAddress();
            var chainId = (int)Models.Chain.Amoy;
            var exchange = Contracts.GetContractConfig(chainId).Exchange;

            var roundConfig = OrderUtils.RoundingConfig["0.01"];
            var res = OrderUtils.GetOrderRawAmounts(Side.Sell, 21.04m, 0.56m, roundConfig);
            var makerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals).ToString();
            var takerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals).ToString();

            var orderStruct = new OrderStruct
            {
                Salt = BigInteger.Zero, Maker = address, Signer = address,
                Taker = "0x0000000000000000000000000000000000000000",
                TokenId = new BigInteger(5), MakerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals),
                TakerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals), Expiration = BigInteger.Zero,
                Nonce = BigInteger.Zero, FeeRateBps = BigInteger.Zero, Side = 1, SignatureType = 2
            };

            var signature = OrderSigner.SignOrder(orderStruct, privateKey, chainId, exchange);
            Assert.False(string.IsNullOrEmpty(signature));
            Assert.Equal("21040000", makerAmount);
            Assert.Equal("11782400", takerAmount);
        }

        [Fact]
        public void CreateOrder_CTF_Sell_Tick_0_001()
        {
            const string privateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";
            var key = new Nethereum.Signer.EthECKey(privateKey);
            var address = key.GetPublicAddress();
            var chainId = (int)Models.Chain.Amoy;
            var exchange = Contracts.GetContractConfig(chainId).Exchange;

            var roundConfig = OrderUtils.RoundingConfig["0.001"];
            var res = OrderUtils.GetOrderRawAmounts(Side.Sell, 21.04m, 0.056m, roundConfig);
            var makerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals).ToString();
            var takerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals).ToString();

            var orderStruct = new OrderStruct
            {
                Salt = BigInteger.Zero, Maker = address, Signer = address,
                Taker = "0x0000000000000000000000000000000000000000",
                TokenId = new BigInteger(5), MakerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals),
                TakerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals), Expiration = BigInteger.Zero,
                Nonce = BigInteger.Zero, FeeRateBps = BigInteger.Zero, Side = 1, SignatureType = 2
            };

            var signature = OrderSigner.SignOrder(orderStruct, privateKey, chainId, exchange);
            Assert.False(string.IsNullOrEmpty(signature));
            Assert.Equal("21040000", makerAmount);
            Assert.Equal("1178240", takerAmount);
        }

        [Fact]
        public void CreateOrder_CTF_Sell_Tick_0_0001()
        {
            const string privateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";
            var key = new Nethereum.Signer.EthECKey(privateKey);
            var address = key.GetPublicAddress();
            var chainId = (int)Models.Chain.Amoy;
            var exchange = Contracts.GetContractConfig(chainId).Exchange;

            var roundConfig = OrderUtils.RoundingConfig["0.0001"];
            var res = OrderUtils.GetOrderRawAmounts(Side.Sell, 21.04m, 0.0056m, roundConfig);
            var makerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals).ToString();
            var takerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals).ToString();

            var orderStruct = new OrderStruct
            {
                Salt = BigInteger.Zero, Maker = address, Signer = address,
                Taker = "0x0000000000000000000000000000000000000000",
                TokenId = new BigInteger(5), MakerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals),
                TakerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals), Expiration = BigInteger.Zero,
                Nonce = BigInteger.Zero, FeeRateBps = BigInteger.Zero, Side = 1, SignatureType = 2
            };

            var signature = OrderSigner.SignOrder(orderStruct, privateKey, chainId, exchange);
            Assert.False(string.IsNullOrEmpty(signature));
            Assert.Equal("21040000", makerAmount);
            Assert.Equal("117824", takerAmount);
        }

        // --- Deterministic createMarketOrder tests (market orders) ported from TS ---
        [Fact]
        public void CreateMarketOrder_CTF_Buy_Tick_0_1()
        {
            const string privateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";
            var key = new Nethereum.Signer.EthECKey(privateKey);
            var address = key.GetPublicAddress();
            var chainId = (int)Models.Chain.Amoy;
            var exchange = Contracts.GetContractConfig(chainId).Exchange;

            var roundConfig = OrderUtils.RoundingConfig["0.1"];
            var res = OrderUtils.GetMarketOrderRawAmounts(Side.Buy, 100m, 0.5m, roundConfig);
            var makerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals).ToString();
            var takerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals).ToString();

            var orderStruct = new OrderStruct
            {
                Salt = new BigInteger(123), Maker = address, Signer = address,
                Taker = "0x0000000000000000000000000000000000000000",
                TokenId = new BigInteger(123), MakerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals),
                TakerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals), Expiration = BigInteger.Zero,
                Nonce = new BigInteger(123), FeeRateBps = new BigInteger(111), Side = 0, SignatureType = 0
            };

            var signature = OrderSigner.SignOrder(orderStruct, privateKey, chainId, exchange);
            Assert.False(string.IsNullOrEmpty(signature));
            Assert.Equal("100000000", makerAmount);
            Assert.Equal("200000000", takerAmount);
        }

        [Fact]
        public void CreateMarketOrder_CTF_Buy_Tick_0_01()
        {
            const string privateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";
            var key = new Nethereum.Signer.EthECKey(privateKey);
            var address = key.GetPublicAddress();
            var chainId = (int)Models.Chain.Amoy;
            var exchange = Contracts.GetContractConfig(chainId).Exchange;

            var roundConfig = OrderUtils.RoundingConfig["0.01"];
            var res = OrderUtils.GetMarketOrderRawAmounts(Side.Buy, 100m, 0.56m, roundConfig);
            var makerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals).ToString();
            var takerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals).ToString();

            var orderStruct = new OrderStruct
            {
                Salt = new BigInteger(123), Maker = address, Signer = address,
                Taker = "0x0000000000000000000000000000000000000000",
                TokenId = new BigInteger(123), MakerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals),
                TakerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals), Expiration = BigInteger.Zero,
                Nonce = new BigInteger(123), FeeRateBps = new BigInteger(111), Side = 0, SignatureType = 0
            };

            var signature = OrderSigner.SignOrder(orderStruct, privateKey, chainId, exchange);
            Assert.False(string.IsNullOrEmpty(signature));
            Assert.Equal("100000000", makerAmount);
            Assert.Equal("178571400", takerAmount);
        }

        [Fact]
        public void CreateMarketOrder_CTF_Buy_Tick_0_001()
        {
            const string privateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";
            var key = new Nethereum.Signer.EthECKey(privateKey);
            var address = key.GetPublicAddress();
            var chainId = (int)Models.Chain.Amoy;
            var exchange = Contracts.GetContractConfig(chainId).Exchange;

            var roundConfig = OrderUtils.RoundingConfig["0.001"];
            var res = OrderUtils.GetMarketOrderRawAmounts(Side.Buy, 100m, 0.056m, roundConfig);
            var makerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals).ToString();
            var takerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals).ToString();

            var orderStruct = new OrderStruct
            {
                Salt = new BigInteger(123), Maker = address, Signer = address,
                Taker = "0x0000000000000000000000000000000000000000",
                TokenId = new BigInteger(123), MakerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals),
                TakerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals), Expiration = BigInteger.Zero,
                Nonce = new BigInteger(123), FeeRateBps = new BigInteger(111), Side = 0, SignatureType = 0
            };

            var signature = OrderSigner.SignOrder(orderStruct, privateKey, chainId, exchange);
            Assert.False(string.IsNullOrEmpty(signature));
            Assert.Equal("100000000", makerAmount);
            Assert.Equal("1785714280", takerAmount);
        }

        [Fact]
        public void CreateMarketOrder_CTF_Buy_Tick_0_0001()
        {
            const string privateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";
            var key = new Nethereum.Signer.EthECKey(privateKey);
            var address = key.GetPublicAddress();
            var chainId = (int)Models.Chain.Amoy;
            var exchange = Contracts.GetContractConfig(chainId).Exchange;

            var roundConfig = OrderUtils.RoundingConfig["0.0001"];
            var res = OrderUtils.GetMarketOrderRawAmounts(Side.Buy, 100m, 0.0056m, roundConfig);
            var makerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals).ToString();
            var takerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals).ToString();

            var orderStruct = new OrderStruct
            {
                Salt = new BigInteger(123), Maker = address, Signer = address,
                Taker = "0x0000000000000000000000000000000000000000",
                TokenId = new BigInteger(123), MakerAmount = OrderUtils.ParseUnits(res.RawMakerAmt, OrderUtils.CollateralTokenDecimals),
                TakerAmount = OrderUtils.ParseUnits(res.RawTakerAmt, OrderUtils.CollateralTokenDecimals), Expiration = BigInteger.Zero,
                Nonce = new BigInteger(123), FeeRateBps = new BigInteger(111), Side = 0, SignatureType = 0
            };

            var signature = OrderSigner.SignOrder(orderStruct, privateKey, chainId, exchange);
            Assert.False(string.IsNullOrEmpty(signature));
            Assert.Equal("100000000", makerAmount);
            Assert.Equal("17857142857", takerAmount);
        }
    }
}
