namespace Polymarket.ClobClient.Constants
{
    public static class Endpoints
    {
        // Server Time
        public const string Time = "/time";

        // API Key endpoints
        public const string CreateApiKey = "/auth/api-key";
        public const string GetApiKeys = "/auth/api-keys";
        public const string DeleteApiKey = "/auth/api-key";
        public const string DeriveApiKey = "/auth/derive-api-key";
        public const string ClosedOnly = "/auth/ban-status/closed-only";
        
        // Markets
        public const string GetMarkets = "/markets";
        public const string GetMarket = "/markets/";
        public const string GetOrderBook = "/book";
        public const string GetOrderBooks = "/books";
        public const string GetPrice = "/price";
        
        // Orders
        public const string PostOrder = "/order";
        public const string PostOrders = "/orders";
        public const string CancelOrder = "/order";
        public const string CancelOrders = "/orders";
        public const string GetOrder = "/data/order/";
        public const string CancelAll = "/cancel-all";
        
        public const string GetTickSize = "/tick-size";
        
        //... Add others as needed
    }
}
