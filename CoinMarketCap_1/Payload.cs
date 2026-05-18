namespace CoinMarketCap_1
{
	using Skyline.DataMiner.Utils.ExportImport.Attributes;

    /// <summary>Latest Listings table row (pid 1000, 19 columns).</summary>
	internal class LatestListingsRow
    {
        [CsvHeader("ID")]
        public string Id { get; set; }

        [CsvHeader("Name")]
        public string Name { get; set; }

        [CsvHeader("Symbol")]
        public string Symbol { get; set; }

        [CsvHeader("Slug")]
        public string Slug { get; set; }

        [CsvHeader("CMC Rank")]
        public string CmcRank { get; set; }

        [CsvHeader("Price USD")]
        public string PriceUsd { get; set; }

        [CsvHeader("Percent Change 24h")]
        public string PercentChange24h { get; set; }

        [CsvHeader("Market Cap USD")]
        public string MarketCapUsd { get; set; }

        [CsvHeader("Volume 24h USD")]
        public string Volume24hUsd { get; set; }

        [CsvHeader("Circulating Supply")]
        public string CirculatingSupply { get; set; }

        [CsvHeader("Max Supply")]
        public string MaxSupply { get; set; }

        [CsvHeader("Last Updated")]
        public string LastUpdated { get; set; }

        [CsvHeader("Percent Change 1h")]
        public string PercentChange1h { get; set; }

        [CsvHeader("Percent Change 7d")]
        public string PercentChange7d { get; set; }

        [CsvHeader("Percent Change 30d")]
        public string PercentChange30d { get; set; }

        [CsvHeader("Volume Change 24h")]
        public string VolumeChange24h { get; set; }

        [CsvHeader("Fully Diluted Market Cap")]
        public string FullyDilutedMarketCap { get; set; }

        [CsvHeader("Total Supply")]
        public string TotalSupply { get; set; }

        [CsvHeader("Infinite Supply")]
        public string InfiniteSupply { get; set; }
    }

    /// <summary>Categories table row (pid 2000, 10 columns).</summary>
	internal class CategoriesRow
    {
        [CsvHeader("ID")]
        public string Id { get; set; }

        [CsvHeader("Name")]
        public string Name { get; set; }

        [CsvHeader("Num Tokens")]
        public string NumTokens { get; set; }

        [CsvHeader("Avg Price Change")]
        public string AvgPriceChange { get; set; }

        [CsvHeader("Volume Change")]
        public string VolumeChange { get; set; }

        [CsvHeader("Market Cap USD")]
        public string MarketCapUsd { get; set; }

        [CsvHeader("Market Cap Change")]
        public string MarketCapChange { get; set; }

        [CsvHeader("Volume 24h")]
        public string Volume24h { get; set; }

        [CsvHeader("Last Updated")]
        public string LastUpdated { get; set; }

        [CsvHeader("Refresh Button")]
        public string RefreshButton { get; set; }
    }

	internal class LatestQuotesRow
    {
        [CsvHeader("Element Name")]
        public string ElementName { get; set; }

        [CsvHeader("Total Market Cap (USD)")]
        public string TotalMarketCapUsd { get; set; }

        [CsvHeader("Total Volume 24h (USD)")]
        public string TotalVolume24hUsd { get; set; }

        [CsvHeader("BTC Dominance (%)")]
        public string BtcDominance { get; set; }

        [CsvHeader("ETH Dominance (%)")]
        public string EthDominance { get; set; }

        [CsvHeader("Active Cryptocurrencies")]
        public string ActiveCryptocurrencies { get; set; }

        [CsvHeader("Last Updated")]
        public string LastUpdated { get; set; }

        [CsvHeader("DeFi 24h Percentage Change (%)")]
        public string DeFi24hPercentageChange { get; set; }

        [CsvHeader("Active Exchanges")]
        public string ActiveExchanges { get; set; }

        [CsvHeader("Total Market Cap Yesterday (USD)")]
        public string TotalMarketCapYesterdayUsd { get; set; }

        [CsvHeader("Total Market Cap Yesterday Percentage Change (%)")]
        public string TotalMarketCapYesterdayPercentageChange { get; set; }

        [CsvHeader("Total Volume 24h Yesterday (USD)")]
        public string TotalVolume24hYesterdayUsd { get; set; }

        [CsvHeader("Total Volume 24h Yesterday Percentage Change (%)")]
        public string TotalVolume24hYesterdayPercentageChange { get; set; }

        [CsvHeader("Altcoin Market Cap (USD)")]
        public string AltcoinMarketCapUsd { get; set; }

        [CsvHeader("Altcoin Volume 24h (USD)")]
        public string AltcoinVolume24hUsd { get; set; }

        [CsvHeader("DeFi Market Cap (USD)")]
        public string DeFiMarketCapUsd { get; set; }

        [CsvHeader("DeFi Volume 24h (USD)")]
        public string DeFiVolume24hUsd { get; set; }

        [CsvHeader("Stablecoin Market Cap (USD)")]
        public string StablecoinMarketCapUsd { get; set; }

        [CsvHeader("Stablecoin Volume 24h (USD)")]
        public string StablecoinVolume24hUsd { get; set; }

        [CsvHeader("Stablecoin 24h Percentage Change (%)")]
        public string Stablecoin24hPercentageChange { get; set; }

        [CsvHeader("Derivatives Volume 24h (USD)")]
        public string DerivativesVolume24hUsd { get; set; }

        [CsvHeader("Derivatives 24h Percentage Change (%)")]
        public string Derivatives24hPercentageChange { get; set; }
    }
}
