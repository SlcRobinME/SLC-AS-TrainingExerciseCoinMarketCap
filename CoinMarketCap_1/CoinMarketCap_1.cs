/*
****************************************************************************
*  Copyright (c),  Skyline Communications NV  All Rights Reserved.        *
****************************************************************************

Revision History:

DATE        VERSION     AUTHOR          COMMENTS

15/05/2026  1.0.0.1     AMO, Skyline    Initial version
18/05/2026  1.0.0.2     AMO, Skyline    Fixed: export all columns + Categories table
18/05/2026  1.0.0.3     AMO, Skyline    feat: added more Latest Quotes parameters + used Skyline.DataMiner.Utils.ExportImport
19/05/2026  1.0.0.4     AMO, Skyline    refactor: generic ExportTableToCsv<T> + LatestQuotesPids-driven export
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

                // Flexible protocol name matching (IndexOf) so minor naming variations don't break the script
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

                // Export all standalone Latest Quotes parameters into one combined CSV (one row per element)
                ExportLatestQuotesToCsv(engine, elements, exportPath);

                // Export per-element table data using the generic helper
                foreach (var element in elements)
                {
                    ExportTableToCsv(engine, element, exportPath, LatestListingsTableId, "LatestListings", MapLatestListingsRow);
                    ExportTableToCsv(engine, element, exportPath, CategoriesTableId, "Categories", MapCategoriesRow);
                }
            }
            catch (Exception ex)
            {
                engine.Log($"Script|Run|Exception thrown: {Environment.NewLine}{ex}");
            }
        }

        /// <summary>
        /// Exports standalone Latest Quotes parameters for all elements into one CSV.
        /// Each PID read has its own try/catch so a single missing parameter never aborts the row.
        /// </summary>
        private void ExportLatestQuotesToCsv(IEngine engine, IList<IDmsElement> elements, string exportPath)
        {
            try
            {
                engine.GenerateInformation($"Script|ExportLatestQuotesToCsv|Exporting Latest Quotes for {elements.Count} element(s).");

                var dtoList = new List<LatestQuotesRow>();

                foreach (var element in elements)
                {
                    string SafeGet(int pid)
                    {
                        try
                        {
                            return element.GetStandaloneParameter<string>(pid).GetValue() ?? string.Empty;
                        }
                        catch (Exception ex)
                        {
                            engine.Log($"Script|ExportLatestQuotesToCsv|Could not read PID {pid} from '{element.Name}': {ex.Message}");
                            return string.Empty;
                        }
                    }

                    dtoList.Add(new LatestQuotesRow
                    {
                        ElementName = ScriptHelpers.EscapeCsvValue(element.Name),
                        TotalMarketCapUsd = ScriptHelpers.EscapeCsvValue(SafeGet(300)),
                        TotalVolume24hUsd = ScriptHelpers.EscapeCsvValue(SafeGet(301)),
                        BtcDominance = ScriptHelpers.EscapeCsvValue(SafeGet(302)),
                        EthDominance = ScriptHelpers.EscapeCsvValue(SafeGet(303)),
                        ActiveCryptocurrencies = ScriptHelpers.EscapeCsvValue(SafeGet(304)),
                        LastUpdated = ScriptHelpers.EscapeCsvValue(SafeGet(305)),
                        DeFi24hPercentageChange = ScriptHelpers.EscapeCsvValue(SafeGet(306)),
                        ActiveExchanges = ScriptHelpers.EscapeCsvValue(SafeGet(307)),
                        TotalMarketCapYesterdayUsd = ScriptHelpers.EscapeCsvValue(SafeGet(308)),
                        TotalMarketCapYesterdayPercentageChange = ScriptHelpers.EscapeCsvValue(SafeGet(309)),
                        TotalVolume24hYesterdayUsd = ScriptHelpers.EscapeCsvValue(SafeGet(310)),
                        TotalVolume24hYesterdayPercentageChange = ScriptHelpers.EscapeCsvValue(SafeGet(311)),
                        AltcoinMarketCapUsd = ScriptHelpers.EscapeCsvValue(SafeGet(312)),
                        AltcoinVolume24hUsd = ScriptHelpers.EscapeCsvValue(SafeGet(313)),
                        DeFiMarketCapUsd = ScriptHelpers.EscapeCsvValue(SafeGet(314)),
                        DeFiVolume24hUsd = ScriptHelpers.EscapeCsvValue(SafeGet(315)),
                        StablecoinMarketCapUsd = ScriptHelpers.EscapeCsvValue(SafeGet(316)),
                        StablecoinVolume24hUsd = ScriptHelpers.EscapeCsvValue(SafeGet(317)),
                        Stablecoin24hPercentageChange = ScriptHelpers.EscapeCsvValue(SafeGet(318)),
                        DerivativesVolume24hUsd = ScriptHelpers.EscapeCsvValue(SafeGet(319)),
                        Derivatives24hPercentageChange = ScriptHelpers.EscapeCsvValue(SafeGet(320)),
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

        /// <summary>
        /// Generic table export helper (from Doc 2).
        /// Iterates table rows, calls <paramref name="rowMapper"/> for each primary key,
        /// and writes the result list to a CSV named "{elementName}_{tableName}.csv".
        /// </summary>
        private void ExportTableToCsv<T>(
            IEngine engine,
            IDmsElement element,
            string exportPath,
            int tableId,
            string tableName,
            Func<IDmsTable, string, T> rowMapper)
            where T : class, new()
        {
            try
            {
                engine.GenerateInformation($"Script|ExportTableToCsv|Exporting '{tableName}' for element: {element.Name}");

                var table = element.GetTable(tableId);
                var rows = table.GetRows();

                // Guard: check null before using Length to avoid NullReferenceException
                if (rows == null || rows.Length == 0)
                {
                    engine.Log($"Script|ExportTableToCsv|No rows found in '{tableName}' for element: {element.Name}");
                    return;
                }

                var exportRows = new List<T>(rows.Length);

                for (int i = 0; i < rows.Length; i++)
                {
                    string primaryKey = Convert.ToString(rows[i][0]);
                    exportRows.Add(rowMapper(table, primaryKey));
                }

                string filePath = SecurePath.ConstructSecurePath(exportPath, $"{element.Name}_{tableName}.csv");
                var writer = WriterFactory.GetWriter<T>(filePath);
                writer.Write(exportRows);

                engine.GenerateInformation($"Script|ExportTableToCsv|Exported {exportRows.Count} rows to: {filePath}");
            }
            catch (Exception ex)
            {
                engine.Log($"Script|ExportTableToCsv|Exception thrown:{Environment.NewLine}{ex}");
            }
        }

        private LatestListingsRow MapLatestListingsRow(IDmsTable table, string primaryKey)
        {
            return new LatestListingsRow
            {
                Id = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(1001).GetValue(primaryKey, KeyType.PrimaryKey))),
                Name = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(1002).GetValue(primaryKey, KeyType.PrimaryKey))),
                Symbol = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(1003).GetValue(primaryKey, KeyType.PrimaryKey))),
                Slug = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(1004).GetValue(primaryKey, KeyType.PrimaryKey))),
                CmcRank = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(1005).GetValue(primaryKey, KeyType.PrimaryKey))),
                PriceUsd = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(1006).GetValue(primaryKey, KeyType.PrimaryKey))),
                PercentChange24h = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(1007).GetValue(primaryKey, KeyType.PrimaryKey))),
                MarketCapUsd = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(1008).GetValue(primaryKey, KeyType.PrimaryKey))),
                Volume24hUsd = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(1009).GetValue(primaryKey, KeyType.PrimaryKey))),
                CirculatingSupply = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(1010).GetValue(primaryKey, KeyType.PrimaryKey))),
                MaxSupply = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(1011).GetValue(primaryKey, KeyType.PrimaryKey))),
                LastUpdated = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(1012).GetValue(primaryKey, KeyType.PrimaryKey))),
                PercentChange1h = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(1013).GetValue(primaryKey, KeyType.PrimaryKey))),
                PercentChange7d = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(1014).GetValue(primaryKey, KeyType.PrimaryKey))),
                PercentChange30d = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(1015).GetValue(primaryKey, KeyType.PrimaryKey))),
                VolumeChange24h = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(1016).GetValue(primaryKey, KeyType.PrimaryKey))),
                FullyDilutedMarketCap = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(1017).GetValue(primaryKey, KeyType.PrimaryKey))),
                TotalSupply = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(1018).GetValue(primaryKey, KeyType.PrimaryKey))),
                InfiniteSupply = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(1019).GetValue(primaryKey, KeyType.PrimaryKey))),
            };
        }

        private CategoriesRow MapCategoriesRow(IDmsTable table, string primaryKey)
        {
            return new CategoriesRow
            {
                Id = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(2001).GetValue(primaryKey, KeyType.PrimaryKey))),
                Name = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(2002).GetValue(primaryKey, KeyType.PrimaryKey))),
                NumTokens = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(2003).GetValue(primaryKey, KeyType.PrimaryKey))),
                AvgPriceChange = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(2004).GetValue(primaryKey, KeyType.PrimaryKey))),
                VolumeChange = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(2005).GetValue(primaryKey, KeyType.PrimaryKey))),
                MarketCapUsd = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(2006).GetValue(primaryKey, KeyType.PrimaryKey))),
                MarketCapChange = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(2007).GetValue(primaryKey, KeyType.PrimaryKey))),
                Volume24h = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(2008).GetValue(primaryKey, KeyType.PrimaryKey))),
                LastUpdated = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(2009).GetValue(primaryKey, KeyType.PrimaryKey))),
                RefreshButton = ScriptHelpers.EscapeCsvValue(Convert.ToString(table.GetColumn<string>(2010).GetValue(primaryKey, KeyType.PrimaryKey))),
            };
        }
    }
}