namespace Polymarket.ClobClient.Constants
{
    public class ContractConfig
    {
        public string Exchange { get; set; } = string.Empty;
        public string NegRiskAdapter { get; set; } = string.Empty;
        public string NegRiskExchange { get; set; } = string.Empty;
        public string Collateral { get; set; } = string.Empty;
        public string ConditionalTokens { get; set; } = string.Empty;
    }

    public static class Contracts
    {
        public static readonly ContractConfig Matic = new ContractConfig
        {
            Exchange = "0x4bFb41d5B3570DeFd03C39a9A4D8dE6Bd8B8982E",
            NegRiskAdapter = "0xd91E80cF2E7be2e162c6513ceD06f1dD0dA35296",
            NegRiskExchange = "0xC5d563A36AE78145C45a50134d48A1215220f80a",
            Collateral = "0x2791Bca1f2de4661ED88A30C99A7a9449Aa84174",
            ConditionalTokens = "0x4D97DCd97eC945f40cF65F87097ACe5EA0476045"
        };

        public static readonly ContractConfig Amoy = new ContractConfig
        {
            Exchange = "0xdFE02Eb6733538f8Ea35D585af8DE5958AD99E40",
            NegRiskAdapter = "0xd91E80cF2E7be2e162c6513ceD06f1dD0dA35296",
            NegRiskExchange = "0xC5d563A36AE78145C45a50134d48A1215220f80a",
            Collateral = "0x9c4e1703476e875070ee25b56a58b008cfb8fa78",
            ConditionalTokens = "0x69308FB512518e39F9b16112fA8d994F4e2Bf8bB"
        };

        public static ContractConfig GetContractConfig(int chainId)
        {
            return chainId switch
            {
                137 => Matic,
                80002 => Amoy,
                _ => throw new ArgumentException("Invalid network")
            };
        }
    }
}
