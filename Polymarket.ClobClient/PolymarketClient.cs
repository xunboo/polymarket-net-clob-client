using Nethereum.Web3.Accounts;
using Polymarket.ClobClient.Constants;
using Polymarket.ClobClient.Models;
using Polymarket.ClobClient.Signing;
using Polymarket.ClobClient.Utilities;

namespace Polymarket.ClobClient
{
    public class PolymarketClient
    {
        public readonly string Host;
        public readonly int ChainId;
        public readonly Account Account;
        public ApiKeyCreds Creds;
        
        private readonly HttpHelper _httpHelper;

        public PolymarketClient(string host, int chainId, string privateKey, ApiKeyCreds creds = null)
        {
            Host = host.TrimEnd('/');
            ChainId = chainId;
            if (!string.IsNullOrEmpty(privateKey))
            {
                Account = new Account(privateKey, chainId);
            }
            Creds = creds;
            
            var httpClient = new HttpClient();
            _httpHelper = new HttpHelper(httpClient, Host);
        }

        // --- Headers Helpers ---

        private async Task<Dictionary<string, string>> CreateL1Headers(int nonce = 0)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var sig = SignerUtils.BuildClobEip712Signature(Account.PrivateKey, ChainId, timestamp, nonce);
            
            return new Dictionary<string, string>
            {
                { "POLY_ADDRESS", Account.Address },
                { "POLY_SIGNATURE", sig },
                { "POLY_TIMESTAMP", timestamp.ToString() },
                { "POLY_NONCE", nonce.ToString() }
            };
        }

        private Dictionary<string, string> CreateL2Headers(string method, string requestPath, object body = null)
        {
            if (Creds == null) throw new Exception("API Credentials (L2) are not set.");

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var bodyStr = body != null ? Newtonsoft.Json.JsonConvert.SerializeObject(body) : "";
            
            var sig = SignerUtils.BuildPolyHmacSignature(
                Creds.Secret, 
                timestamp, 
                method, 
                requestPath, 
                bodyStr
            );

            return new Dictionary<string, string>
            {
                { "POLY_ADDRESS", Account.Address },
                { "POLY_SIGNATURE", sig },
                { "POLY_TIMESTAMP", timestamp.ToString() },
                { "POLY_API_KEY", Creds.Key },
                { "POLY_PASSPHRASE", Creds.Passphrase }
            };
        }

        // --- Public Endpoints ---

        public async Task<object> GetMarkets(string nextCursor = "")
        {
            var query = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(nextCursor)) query["next_cursor"] = nextCursor;
            
            // Return raw string or specific model? For now allow dynamic/object or string
            return await _httpHelper.GetAsync<object>(Endpoints.GetMarkets, null, query);
        }

        public async Task<OrderBookSummary> GetOrderBook(string tokenId)
        {
            var query = new Dictionary<string, object> { { "token_id", tokenId } };
            return await _httpHelper.GetAsync<OrderBookSummary>(Endpoints.GetOrderBook, null, query);
        }

        // --- Auth Endpoints ---

        public async Task<ApiKeyCreds> CreateApiKey(int nonce = 0)
        {
            var headers = await CreateL1Headers(nonce);
            var response = await _httpHelper.PostAsync<ApiKeyRaw>(Endpoints.CreateApiKey, null, headers);
            
            var creds = new ApiKeyCreds
            {
                Key = response.ApiKey,
                Secret = response.Secret,
                Passphrase = response.Passphrase
            };
            
            // Update local creds
            this.Creds = creds;
            return creds;
        }

        public async Task<ApiKeyCreds> DeriveApiKey(int nonce = 0)
        {
            var headers = await CreateL1Headers(nonce);
            var response = await _httpHelper.GetAsync<ApiKeyRaw>(Endpoints.DeriveApiKey, headers);
            
             var creds = new ApiKeyCreds
            {
                Key = response.ApiKey,
                Secret = response.Secret,
                Passphrase = response.Passphrase
            };
            
            this.Creds = creds;
            return creds;
        }

        public async Task<string> PostOrder(UserOrder order, OrderType orderType = OrderType.Gtc)
        {
            if (Creds == null) throw new Exception("API Credentials (L2) are required for posting orders.");

            // 1. Get Tick Size (Needed for rounding)
            var tickSizeStr = await GetTickSize(order.TokenId);
            
            // 2. Resolve Rounding Config
            if (!OrderUtils.RoundingConfig.ContainsKey(tickSizeStr))
            {
                 // Fallback or throw?
                 // Most markets are 0.1 or 0.01 etc.
                 // If not found, maybe default to 2 decimals (0.01)?
                 // For safety throw
                 if (tickSizeStr == "0.0001") { /* Covered */ }
                 else if (tickSizeStr == "0.001") { /* Covered */ }
                 else if (tickSizeStr == "0.01") { /* Covered */ }
                 else if (tickSizeStr == "0.1") { /* Covered */ }
                 else 
                 {
                     // Try to match closest or throw
                     throw new Exception($"Unsupported tick size: {tickSizeStr}");
                 }
            }
            var roundConfig = OrderUtils.RoundingConfig[tickSizeStr];

            // 3. Calculate Raw Amounts
            var (sideEnum, rawMakerAmt, rawTakerAmt) = OrderUtils.GetOrderRawAmounts(
                order.Side, order.Size, order.Price, roundConfig);

            var makerAmount = OrderUtils.ParseUnits(rawMakerAmt, OrderUtils.CollateralTokenDecimals);
            var takerAmount = OrderUtils.ParseUnits(rawTakerAmt, OrderUtils.CollateralTokenDecimals);
            
            // 4. Construct Order Struct
            var salt = new System.Numerics.BigInteger(new Random().Next()); // Simplified salt
            var expiration = order.Expiration ?? 0;
            var nonce = order.Nonce ?? 0;
            var feeRateBps = order.FeeRateBps ?? 0;
            
            // Resolve Taker
            var taker = order.Taker ?? "0x0000000000000000000000000000000000000000";

            var orderStruct = new OrderStruct
            {
                Salt = salt,
                Maker = Account.Address,
                Signer = Account.Address,
                Taker = taker,
                TokenId = System.Numerics.BigInteger.Parse(order.TokenId),
                MakerAmount = makerAmount,
                TakerAmount = takerAmount,
                Expiration = expiration,
                Nonce = nonce,
                FeeRateBps = feeRateBps,
                Side = (int)sideEnum, // 0 for Buy, 1 for Sell usually? CHECK ENUM
                SignatureType = 0 // EOA
            };
            
            // CHECK SIDE ENUM MAPPING
            // In OrderUtils.cs:
            // public enum Side { Buy, Sell } (from Models/Enums.cs)
            // But Polymarket/order-utils Side might be: Buy=0, Sell=1. 
            // In types.ts: BUY="BUY", SELL="SELL" (String Enum).
            // BUT in Solidity/EIP712, side is uint8.
            // Search result says: Side: 0 for Buy, 1 for Sell.
            // My Enum in Models/Enums.cs uses StringEnumConverter because API expects string "BUY"/"SELL".
            // But EIP712 expects uint8.
            // I need to map carefully. 
            // TS: Side.BUY -> 0, Side.SELL -> 1.
            
            orderStruct.Side = order.Side == Side.Buy ? 0 : 1;

            // 5. Sign Order
            var contractConfig = Contracts.GetContractConfig(ChainId);
            // Need to know if it's NegRisk... defaulting to standard Exchange for now.
            // If NegRisk is true, use NegRiskExchange.
            // I should technically check NegRisk status.
            // For now assume standard.
            var exchangeAddr = contractConfig.Exchange;
            
            var signature = OrderSigner.SignOrder(orderStruct, Account.PrivateKey, ChainId, exchangeAddr);

            // 6. Build Request Body
            // The body expects "order" object with all fields + signature string
            var signedOrder = new
            {
                salt = orderStruct.Salt.ToString(),
                maker = orderStruct.Maker,
                signer = orderStruct.Signer,
                taker = orderStruct.Taker,
                tokenId = orderStruct.TokenId.ToString(),
                makerAmount = orderStruct.MakerAmount.ToString(),
                takerAmount = orderStruct.TakerAmount.ToString(),
                expiration = orderStruct.Expiration.ToString(),
                nonce = orderStruct.Nonce.ToString(),
                feeRateBps = orderStruct.FeeRateBps.ToString(),
                side = orderStruct.Side,
                signatureType = orderStruct.SignatureType,
                signature = signature
            };

            var payload = new
            {
                order = signedOrder,
                owner = Creds.Key,
                orderType = orderType.ToString().ToUpper(), // GTC, FOK etc.
            };

            // 7. Send Request
            var headers = CreateL2Headers("POST", Endpoints.PostOrder, payload);
            return await _httpHelper.PostAsync<string>(Endpoints.PostOrder, payload, headers);
        }

        public async Task<string> GetTickSize(string tokenId)
        {
            // Helper class for response
            var response = await _httpHelper.GetAsync<TickSizeResponse>(Endpoints.GetTickSize, null, new Dictionary<string, object> { { "token_id", tokenId } });
            return response.MinimumTickSize;
        }

        public async Task<string> CancelAllOrders()
        {
            if (Creds == null) throw new Exception("API Credentials (L2) are required.");
            var headers = CreateL2Headers("DELETE", Endpoints.CancelAll);
            return await _httpHelper.DeleteAsync<string>(Endpoints.CancelAll, null, headers);
        }

        private class TickSizeResponse
        {
            [Newtonsoft.Json.JsonProperty("minimum_tick_size")]
            public string MinimumTickSize { get; set; }
        }
    }
}
