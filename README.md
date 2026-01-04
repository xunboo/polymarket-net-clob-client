# Polymarket .NET CLOB Client

A C# .NET Class Library for interacting with the Polymarket CLOB (Central Limit Order Book) API. This is a port of the official TypeScript [clob-client](https://github.com/Polymarket/clob-client).

This library is converted by AI and currently in beta and is not yet ready for production use. Use at your own risk.

## Installation

```bash
dotnet add package Polymarket.ClobClient
```

Or add the project reference:
```xml
<ProjectReference Include="path/to/Polymarket.ClobClient.csproj" />
```

## Dependencies

- .NET 8.0+
- `Nethereum.Web3` - Ethereum interactions
- `Nethereum.Signer.EIP712` - EIP-712 typed data signing
- `Newtonsoft.Json` - JSON serialization

## Quick Start

```csharp
using Polymarket.ClobClient;
using Polymarket.ClobClient.Models;

// Initialize client
var client = new PolymarketClient(
    host: "https://clob.polymarket.com",
    chainId: 137, // Polygon Mainnet (use 80002 for Amoy testnet)
    privateKey: "0xYOUR_PRIVATE_KEY"
);

// Get markets
var markets = await client.GetMarkets();

// Get order book
var orderBook = await client.GetOrderBook("TOKEN_ID");
```

## Authentication

### L1 Authentication (EIP-712)
Used for creating/deriving API keys:

```csharp
// Create API key (requires L1 signature)
var apiKey = await client.CreateApiKey(nonce: 0);
```

### L2 Authentication (HMAC-SHA256)
Used for authenticated operations like placing orders:

```csharp
// Set API credentials
client.SetApiCredentials(new ApiKeyCreds
{
    Key = "your-api-key",
    Secret = "your-api-secret",
    Passphrase = "your-passphrase"
});

// Place an order
var order = new UserOrder
{
    TokenId = "TOKEN_ID",
    Price = 0.5m,
    Size = 100,
    Side = Side.Buy
};
var result = await client.PostOrder(order, OrderType.GTC);
```

## Project Structure

```
Polymarket.ClobClient/
├── Constants/
│   ├── Contracts.cs      # Exchange contract addresses
│   └── Endpoints.cs      # API endpoint constants
├── Models/
│   ├── Auth.cs           # ApiKeyCreds, LoginResponse
│   ├── Enums.cs          # Side, OrderType, Chain
│   └── Orders.cs         # UserOrder, OrderSummary, etc.
├── Signing/
│   ├── ClobAuth.cs       # EIP-712 auth structure
│   ├── OrderSigner.cs    # Order EIP-712 signing
│   ├── OrderStruct.cs    # EIP-712 order structure
│   └── SignerUtils.cs    # EIP-712 & HMAC signing
├── Utilities/
│   ├── GeneralUtils.cs   # Hash generation
│   ├── HttpHelper.cs     # HTTP client wrapper
│   ├── MathUtils.cs      # Rounding functions
│   └── OrderUtils.cs     # Order amount calculations
└── PolymarketClient.cs   # Main client class
```

## API Reference

### Public Endpoints
| Method | Description |
|--------|-------------|
| `GetMarkets(cursor?)` | Get available markets |
| `GetOrderBook(tokenId)` | Get order book for a token |

### Authenticated Endpoints (L1)
| Method | Description |
|--------|-------------|
| `CreateApiKey(nonce)` | Create new API key |
| `DeriveApiKey(nonce)` | Derive existing API key |

### Authenticated Endpoints (L2)
| Method | Description |
|--------|-------------|
| `PostOrder(order, orderType)` | Place an order |
| `CancelAllOrders()` | Cancel all open orders |
| `DeleteApiKey()` | Delete current API key |

## Signing Implementation

### EIP-712 (L1 Authentication)
Uses JSON-based EIP-712 signing via `Nethereum.Signer.EIP712`:

```csharp
var signature = SignerUtils.BuildClobEip712Signature(
    privateKey, chainId, timestamp, nonce
);
```

### HMAC-SHA256 (L2 Authentication)
```csharp
var signature = SignerUtils.BuildPolyHmacSignature(
    secret, timestamp, method, requestPath, body
);
```

## Testing

Run the test suite:
```bash
cd Polymarket.ClobClient.Tests
dotnet test
```

**Test Coverage:**
```
Passed! - Failed: 0, Passed: 23, Skipped: 0, Total: 23
```

| Test Category | Tests |
|---------------|-------|
| EIP-712 Signing | 1 |
| HMAC Signing | 3 |
| Headers | 4 |
| Utilities | 6 |
| Order Builder | 7 |
| HTTP Helpers | 2 |

## Chain Configuration

| Chain | Chain ID | Exchange Address |
|-------|----------|------------------|
| Polygon Mainnet | 137 | `0x4bFb41d5B3570DeFd03C39a9A4D8dE6Bd8B8982E` |
| Amoy Testnet | 80002 | `0xdFE02Eb6733538f8Ea35D585af8DE5958AD99E40` |

## License

MIT

## Contributing

Contributions welcome! Please submit PRs against the `main` branch.
