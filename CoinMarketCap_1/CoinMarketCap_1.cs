/*
****************************************************************************
*  Copyright (c),  Skyline Communications NV  All Rights Reserved.        *
****************************************************************************

Revision History:

DATE        VERSION     AUTHOR          COMMENTS

15/05/2024  1.0.0.1     AMO, Skyline    Initial version
18/05/2024  1.0.0.2     AMO, Skyline    Fixed: export all columns + Categories table
18/05/2024  1.0.0.3     AMO, Skyline    feat: added more Latest Quotes parameters + used Skyline.DataMiner.Utils.ExportImport
****************************************************************************
*/

namespace CoinMarketCap_1
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Utils.ExportImport.Factories;
    using Skyline.DataMiner.Utils.SecureCoding.SecureIO;

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
    public class Script
    {
        private const string BaseExportPath = @"C:\Skyline DataMiner\Documents";
        private const int LatestListingsTableId = 1000;
        private const int CategoriesTableId = 2000;

        private static readonly Dictionary<int, string> LatestQuotesPids = new Dictionary<int, string>
        {
            { 300, "Total Market Cap (USD)" },
            { 301, "Total Volume 24h (USD)" },
            { 302, "BTC Dominance (%)" },
            { 303, "ETH Dominance (%)" },
            { 304, "Active Cryptocurrencies" },
            { 305, "Last Updated" },
            { 306, "DeFi 24h Percentage Change (%)" },
            { 307, "Active Exchanges" },
            { 308, "Total Market Cap Yesterday (USD)" },
            { 309, "Total Market Cap Yesterday Percentage Change (%)" },
            { 310, "Total Volume 24h Yesterday (USD)" },
            { 311, "Total Volume 24h Yesterday Percentage Change (%)" },
            { 312, "Altcoin Market Cap (USD)" },
            { 313, "Altcoin Volume 24h (USD)" },
            { 314, "DeFi Market Cap (USD)" },
            { 315, "DeFi Volume 24h (USD)" },
            { 316, "Stablecoin Market Cap (USD)" },
            { 317, "Stablecoin Volume 24h (USD)" },
            { 318, "Stablecoin 24h Percentage Change (%)" },
            { 319, "Derivatives Volume 24h (USD)" },
            { 320, "Derivatives 24h Percentage Change (%)" },
        };

        /// <summary>
        /// The script entry point.
        /// </summary>
        /// <param name="engine">Link with SLAutomation process.</param>
        public void Run(IEngine engine)
        {
            try
            {
                string folderName = engine.GetScriptParam("Folder Name").Value;

                if (string.IsNullOrWhiteSpace(folderName))
                {
                    engine.ExitFail("Folder name parameter is empty");
                    return;
                }

                string exportPath = SecurePath.ConstructSecurePath(BaseExportPath, folderName);

                IDms dms = engine.GetDms();

                var elements = dms.GetElements()
                    .Where(e => e.Protocol.Name.IndexOf("CoinMarketCap", StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                if (!elements.Any())
                {
                    engine.ExitFail("No CoinMarketCap elements found on the DMA.");
                    return;
                }

                engine.GenerateInformation($"Script|Run|Found {elements.Count} CoinMarketCap element(s).");

                if (!Directory.Exists(exportPath))
                    Directory.CreateDirectory(exportPath);

                ExportLatestQuotesToCsv(engine, elements, exportPath);

                foreach (var element in elements)
                {
                    ExportLatestListingsToCsv(engine, element, exportPath);
                    ExportCategoriesToCsv(engine, element, exportPath);
                }
            }
            catch (Exception ex)
            {
                engine.Log($"Script|Run|Exception thrown: {Environment.NewLine}{ex}");
            }
        }

        /// <summary>
        /// Exports standalone Latest Quotes parameters for all elements into one CSV.
        /// Columns are dynamic (built from LatestQuotesPids dictionary), so we assemble
        /// the CSV lines manually and write them via WriterFactory as raw strings.
        /// </summary>
        private void ExportLatestQuotesToCsv(IEngine engine, IList<IDmsElement> elements, string exportPath)
        {
            try
            {
                engine.GenerateInformation($"Script|ExportLatestQuotesToCsv|Exporting Latest Quotes for {elements.Count} element(s).");

                var dtoList = new List<LatestQuotesRow>();

                foreach (var element in elements)
                {
                    string Get(int pid)
                    {
                        try { return element.GetStandaloneParameter<string>(pid).GetValue() ?? string.Empty; }
                        catch (Exception ex)
                        {
                            engine.Log($"Script|ExportLatestQuotesToCsv|Could not read pid {pid} from {element.Name}: {ex.Message}");
                            return string.Empty;
                        }
                    }

                    dtoList.Add(new LatestQuotesRow
                    {
                        ElementName = EscapeCsvValue(element.Name),
                        TotalMarketCapUsd = EscapeCsvValue(Get(300)),
                        TotalVolume24hUsd = EscapeCsvValue(Get(301)),
                        BtcDominance = EscapeCsvValue(Get(302)),
                        EthDominance = EscapeCsvValue(Get(303)),
                        ActiveCryptocurrencies = EscapeCsvValue(Get(304)),
                        LastUpdated = EscapeCsvValue(Get(305)),
                        DeFi24hPercentageChange = EscapeCsvValue(Get(306)),
                        ActiveExchanges = EscapeCsvValue(Get(307)),
                        TotalMarketCapYesterdayUsd = EscapeCsvValue(Get(308)),
                        TotalMarketCapYesterdayPercentageChange = EscapeCsvValue(Get(309)),
                        TotalVolume24hYesterdayUsd = EscapeCsvValue(Get(310)),
                        TotalVolume24hYesterdayPercentageChange = EscapeCsvValue(Get(311)),
                        AltcoinMarketCapUsd = EscapeCsvValue(Get(312)),
                        AltcoinVolume24hUsd = EscapeCsvValue(Get(313)),
                        DeFiMarketCapUsd = EscapeCsvValue(Get(314)),
                        DeFiVolume24hUsd = EscapeCsvValue(Get(315)),
                        StablecoinMarketCapUsd = EscapeCsvValue(Get(316)),
                        StablecoinVolume24hUsd = EscapeCsvValue(Get(317)),
                        Stablecoin24hPercentageChange = EscapeCsvValue(Get(318)),
                        DerivativesVolume24hUsd = EscapeCsvValue(Get(319)),
                        Derivatives24hPercentageChange = EscapeCsvValue(Get(320)),
                    });
                }

                string filePath = SecurePath.ConstructSecurePath(exportPath, "LatestQuotes.csv");
                var writer = WriterFactory.GetWriter<LatestQuotesRow>(filePath);
                writer.Write(dtoList);

                engine.GenerateInformation($"Script|ExportLatestQuotesToCsv|Exported {dtoList.Count} row(s) to: {filePath}");
            }
            catch (Exception ex)
            {
                engine.Log($"Script|ExportLatestQuotesToCsv|Exception thrown:{Environment.NewLine}{ex}");
            }
        }

        private void ExportLatestListingsToCsv(IEngine engine, IDmsElement element, string exportPath)
        {
            try
            {
                engine.GenerateInformation($"Script|ExportLatestListingsToCsv|Exporting element: {element.Name}");

                var table = element.GetTable(LatestListingsTableId);
                var tableRows = table.GetRows();

                if (tableRows == null || tableRows.Length == 0)
                {
                    engine.Log($"Script|ExportLatestListingsToCsv|No rows found for element: {element.Name}");
                    return;
                }

                var dtoList = new List<LatestListingsRow>();
                foreach (var row in tableRows)
                {
                    dtoList.Add(new LatestListingsRow
                    {
                        Id = EscapeCsvValue(Convert.ToString(row[0])),
                        Name = EscapeCsvValue(Convert.ToString(row[1])),
                        Symbol = EscapeCsvValue(Convert.ToString(row[2])),
                        Slug = EscapeCsvValue(Convert.ToString(row[3])),
                        CmcRank = EscapeCsvValue(Convert.ToString(row[4])),
                        PriceUsd = EscapeCsvValue(Convert.ToString(row[5])),
                        PercentChange24h = EscapeCsvValue(Convert.ToString(row[6])),
                        MarketCapUsd = EscapeCsvValue(Convert.ToString(row[7])),
                        Volume24hUsd = EscapeCsvValue(Convert.ToString(row[8])),
                        CirculatingSupply = EscapeCsvValue(Convert.ToString(row[9])),
                        MaxSupply = EscapeCsvValue(Convert.ToString(row[10])),
                        LastUpdated = EscapeCsvValue(Convert.ToString(row[11])),
                        PercentChange1h = EscapeCsvValue(Convert.ToString(row[12])),
                        PercentChange7d = EscapeCsvValue(Convert.ToString(row[13])),
                        PercentChange30d = EscapeCsvValue(Convert.ToString(row[14])),
                        VolumeChange24h = EscapeCsvValue(Convert.ToString(row[15])),
                        FullyDilutedMarketCap = EscapeCsvValue(Convert.ToString(row[16])),
                        TotalSupply = EscapeCsvValue(Convert.ToString(row[17])),
                        InfiniteSupply = EscapeCsvValue(Convert.ToString(row[18])),
                    });
                }

                string filePath = SecurePath.ConstructSecurePath(exportPath, $"{element.Name}_LatestListings.csv");
                var writer = WriterFactory.GetWriter<LatestListingsRow>(filePath);
                writer.Write(dtoList);

                engine.GenerateInformation($"Script|ExportLatestListingsToCsv|Exported {dtoList.Count} rows to: {filePath}");
            }
            catch (Exception ex)
            {
                engine.Log($"Script|ExportLatestListingsToCsv|Exception thrown:{Environment.NewLine}{ex}");
            }
        }

        private void ExportCategoriesToCsv(IEngine engine, IDmsElement element, string exportPath)
        {
            try
            {
                engine.GenerateInformation($"Script|ExportCategoriesToCsv|Exporting element: {element.Name}");

                var table = element.GetTable(CategoriesTableId);
                var tableRows = table.GetRows();

                if (tableRows == null || tableRows.Length == 0)
                {
                    engine.Log($"Script|ExportCategoriesToCsv|No rows found for element: {element.Name}");
                    return;
                }

                var dtoList = new List<CategoriesRow>();
                foreach (var row in tableRows)
                {
                    dtoList.Add(new CategoriesRow
                    {
                        Id = EscapeCsvValue(Convert.ToString(row[0])),
                        Name = EscapeCsvValue(Convert.ToString(row[1])),
                        NumTokens = EscapeCsvValue(Convert.ToString(row[2])),
                        AvgPriceChange = EscapeCsvValue(Convert.ToString(row[3])),
                        VolumeChange = EscapeCsvValue(Convert.ToString(row[4])),
                        MarketCapUsd = EscapeCsvValue(Convert.ToString(row[5])),
                        MarketCapChange = EscapeCsvValue(Convert.ToString(row[6])),
                        Volume24h = EscapeCsvValue(Convert.ToString(row[7])),
                        LastUpdated = EscapeCsvValue(Convert.ToString(row[8])),
                        RefreshButton = EscapeCsvValue(Convert.ToString(row[9])),
                    });
                }

                string filePath = SecurePath.ConstructSecurePath(exportPath, $"{element.Name}_Categories.csv");
                var writer = WriterFactory.GetWriter<CategoriesRow>(filePath);
                writer.Write(dtoList);

                engine.GenerateInformation($"Script|ExportCategoriesToCsv|Exported {dtoList.Count} rows to: {filePath}");
            }
            catch (Exception ex)
            {
                engine.Log($"Script|ExportCategoriesToCsv|Exception thrown:{Environment.NewLine}{ex}");
            }
        }

        private static string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Contains(";") || value.Contains("\"") || value.Contains("\n"))
                return "\"" + value.Replace("\"", "\"\"") + "\"";

            return value;
        }
    }
}