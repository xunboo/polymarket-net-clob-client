using System.Numerics;
using Nethereum.Web3;
using Polymarket.ClobClient.Models;

namespace Polymarket.ClobClient.Utilities
{
    public static class OrderUtils
    {
        public const int CollateralTokenDecimals = 6;

        public static Dictionary<string, RoundConfig> RoundingConfig = new Dictionary<string, RoundConfig>
        {
            { "0.1", new RoundConfig { Price = 1, Size = 2, Amount = 3 } },
            { "0.01", new RoundConfig { Price = 2, Size = 2, Amount = 4 } },
            { "0.001", new RoundConfig { Price = 3, Size = 2, Amount = 5 } },
            { "0.0001", new RoundConfig { Price = 4, Size = 2, Amount = 6 } }
        };

        public static (Side Side, decimal RawMakerAmt, decimal RawTakerAmt) GetOrderRawAmounts(
            Side side, decimal size, decimal price, RoundConfig roundConfig)
        {
            var rawPrice = MathUtils.RoundNormal(price, roundConfig.Price);

            if (side == Side.Buy)
            {
                var rawTakerAmt = MathUtils.RoundDown(size, roundConfig.Size);
                var rawMakerAmt = rawTakerAmt * rawPrice;
                
                if (MathUtils.DecimalPlaces(rawMakerAmt) > roundConfig.Amount)
                {
                    rawMakerAmt = MathUtils.RoundUp(rawMakerAmt, roundConfig.Amount + 4);
                    if (MathUtils.DecimalPlaces(rawMakerAmt) > roundConfig.Amount)
                    {
                        rawMakerAmt = MathUtils.RoundDown(rawMakerAmt, roundConfig.Amount);
                    }
                }

                return (Side.Buy, rawMakerAmt, rawTakerAmt);
            }
            else
            {
                var rawMakerAmt = MathUtils.RoundDown(size, roundConfig.Size);
                var rawTakerAmt = rawMakerAmt * rawPrice;

                if (MathUtils.DecimalPlaces(rawTakerAmt) > roundConfig.Amount)
                {
                    rawTakerAmt = MathUtils.RoundUp(rawTakerAmt, roundConfig.Amount + 4);
                    if (MathUtils.DecimalPlaces(rawTakerAmt) > roundConfig.Amount)
                    {
                        rawTakerAmt = MathUtils.RoundDown(rawTakerAmt, roundConfig.Amount);
                    }
                }

                return (Side.Sell, rawMakerAmt, rawTakerAmt);
            }
        }

        public static (Side Side, decimal RawMakerAmt, decimal RawTakerAmt) GetMarketOrderRawAmounts(
            Side side, decimal amount, decimal price, RoundConfig roundConfig)
        {
            var rawPrice = MathUtils.RoundDown(price, roundConfig.Price);

            if (side == Side.Buy)
            {
                var rawMakerAmt = MathUtils.RoundDown(amount, roundConfig.Size);
                var rawTakerAmt = rawMakerAmt / rawPrice;

                if (MathUtils.DecimalPlaces(rawTakerAmt) > roundConfig.Amount)
                {
                    rawTakerAmt = MathUtils.RoundUp(rawTakerAmt, roundConfig.Amount + 4);
                    if (MathUtils.DecimalPlaces(rawTakerAmt) > roundConfig.Amount)
                    {
                        rawTakerAmt = MathUtils.RoundDown(rawTakerAmt, roundConfig.Amount);
                    }
                }
                return (Side.Buy, rawMakerAmt, rawTakerAmt);
            }
            else
            {
                var rawMakerAmt = MathUtils.RoundDown(amount, roundConfig.Size);
                var rawTakerAmt = rawMakerAmt * rawPrice;

                if (MathUtils.DecimalPlaces(rawTakerAmt) > roundConfig.Amount)
                {
                    rawTakerAmt = MathUtils.RoundUp(rawTakerAmt, roundConfig.Amount + 4);
                    if (MathUtils.DecimalPlaces(rawTakerAmt) > roundConfig.Amount)
                    {
                        rawTakerAmt = MathUtils.RoundDown(rawTakerAmt, roundConfig.Amount);
                    }
                }

                return (Side.Sell, rawMakerAmt, rawTakerAmt);
            }
        }
        
        public static decimal CalculateBuyMarketPrice(List<OrderSummary> positions, decimal amountToMatch, OrderType orderType)
        {
            if (positions == null || positions.Count == 0)
            {
                throw new Exception("no match");
            }

            decimal sum = 0;
            // iterate from end to start (asks highest -> lowest in TS test ordering)
            for (int i = positions.Count - 1; i >= 0; i--)
            {
                var p = positions[i];
                sum += decimal.Parse(p.Size) * decimal.Parse(p.Price);
                if (sum >= amountToMatch)
                {
                    return decimal.Parse(p.Price);
                }
            }

            if (orderType == OrderType.Fok)
            {
                throw new Exception("no match");
            }

            return decimal.Parse(positions[0].Price);
        }

        public static decimal CalculateSellMarketPrice(List<OrderSummary> positions, decimal amountToMatch, OrderType orderType)
        {
            if (positions == null || positions.Count == 0)
            {
                throw new Exception("no match");
            }

            decimal sum = 0;
            for (int i = positions.Count - 1; i >= 0; i--)
            {
                var p = positions[i];
                sum += decimal.Parse(p.Size);
                if (sum >= amountToMatch)
                {
                    return decimal.Parse(p.Price);
                }
            }

            if (orderType == OrderType.Fok)
            {
                throw new Exception("no match");
            }

            return decimal.Parse(positions[0].Price);
        }
        
        public static BigInteger ParseUnits(decimal amount, int decimals)
        {
             return Web3.Convert.ToWei(amount, decimals);
        }
    }
}
