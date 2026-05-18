/*
****************************************************************************
*  Copyright (c),  Skyline Communications NV  All Rights Reserved.        *
****************************************************************************

Revision History:

DATE        VERSION     AUTHOR          COMMENTS

11/01/2024  1.0.0.1     AMO, Skyline    Initial version
11/01/2024  1.0.0.2     AMO, Skyline    Fixed: export all columns + Categories table
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
    using Skyline.DataMiner.Utils.SecureCoding.SecureIO;

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
    public class Script
    {
        private const string ElementName = "CoinMarketCap";
        private const string BaseExportPath = @"C:\Skyline DataMiner\Documents";
        private const int LatestListingsTableId = 1000;
        private const int CategoriesTableId = 2000;

        private static readonly Dictionary<int, string> LatestQuotesParams = new Dictionary<int, string>
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
                    .Where(e => e.Protocol.Name.IndexOf("CoinMarketCap", StringComparison.OrdinalIgnoreCase) >= 0
                             || e.Name.IndexOf("CoinMarketCap", StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                if (!elements.Any())
                {
                    engine.ExitFail($"No elements found with name containing '{ElementName}'.");
                    return;
                }

                engine.GenerateInformation($"Script|Run|Found {elements.Count} CoinMarketCap element(s).");

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
        /// Exports the standalone Latest Quotes parameters (pid 300-320) for ALL elements
        /// into a single CSV file: one row per element, one column per parameter.
        /// </summary>
        private void ExportLatestQuotesToCsv(IEngine engine, IList<IDmsElement> elements, string exportPath)
        {
            try
            {
                engine.GenerateInformation($"Script|ExportLatestQuotesToCsv|Exporting Latest Quotes for {elements.Count} element(s).");

                var csvRows = new List<string>();

                var headerColumns = new List<string> { "Element Name" };
                headerColumns.AddRange(LatestQuotesParams.Values);
                csvRows.Add(string.Join(",", headerColumns));

                foreach (var element in elements)
                {
                    try
                    {
                        var rowValues = new List<string> { EscapeCsvValue(element.Name) };

                        foreach (var kvp in LatestQuotesParams)
                        {
                            try
                            {
                                var param = element.GetStandaloneParameter<string>(kvp.Key);
                                string value = param.GetValue() ?? string.Empty;
                                rowValues.Add(EscapeCsvValue(value));
                            }
                            catch (Exception ex)
                            {
                                engine.Log($"Script|ExportLatestQuotesToCsv|Could not read pid {kvp.Key} from {element.Name}: {ex.Message}");
                                rowValues.Add(string.Empty);
                            }
                        }

                        csvRows.Add(string.Join(",", rowValues));
                    }
                    catch (Exception ex)
                    {
                        engine.Log($"Script|ExportLatestQuotesToCsv|Error processing element {element.Name}: {ex.Message}");
                    }
                }

                EnsureDirectoryExists(exportPath);

                string filePath = SecurePath.ConstructSecurePath(exportPath, "LatestQuotes.csv");
                File.WriteAllLines(filePath, csvRows);

                engine.GenerateInformation($"Script|ExportLatestQuotesToCsv|Exported {elements.Count} row(s) to: {filePath}");
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
                var rows = table.GetRows();

                if (rows == null || rows.Length == 0)
                {
                    engine.Log($"Script|ExportLatestListingsToCsv|No rows found in Latest Listings table for element: {element.Name}");
                    return;
                }

                var csvRows = new List<string>
                {
                    "ID,Name,Symbol,Slug,CMC Rank,Price USD,Percent Change 24h,Market Cap USD," +
                    "Volume 24h USD,Circulating Supply,Max Supply,Last Updated," +
                    "Percent Change 1h,Percent Change 7d,Percent Change 30d," +
                    "Volume Change 24h,Fully Diluted Market Cap,Total Supply,Infinite Supply",
                };

                foreach (var row in rows)
                {
                    var values = new string[19];
                    for (int i = 0; i < 19; i++)
                    {
                        values[i] = EscapeCsvValue(Convert.ToString(row[i]));
                    }

                    csvRows.Add(string.Join(",", values));
                }

                EnsureDirectoryExists(exportPath);

                string filePath = SecurePath.ConstructSecurePath(exportPath, $"{element.Name}_LatestListings.csv");
                File.WriteAllLines(filePath, csvRows);

                engine.GenerateInformation($"Script|ExportLatestListingsToCsv|Exported {rows.Length} rows to: {filePath}");
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
                var rows = table.GetRows();

                if (rows == null || rows.Length == 0)
                {
                    engine.Log($"Script|ExportCategoriesToCsv|No rows found in Categories table for element: {element.Name}");
                    return;
                }

                var csvRows = new List<string>
                {
                    "ID,Name,Num Tokens,Avg Price Change,Volume Change,Market Cap USD," +
                    "Market Cap Change,Volume 24h,Last Updated,Refresh Button",
                };

                foreach (var row in rows)
                {
                    var values = new string[10];
                    for (int i = 0; i < 10; i++)
                    {
                        values[i] = EscapeCsvValue(Convert.ToString(row[i]));
                    }

                    csvRows.Add(string.Join(",", values));
                }

                EnsureDirectoryExists(exportPath);

                string filePath = SecurePath.ConstructSecurePath(exportPath, $"{element.Name}_Categories.csv");
                File.WriteAllLines(filePath, csvRows);

                engine.GenerateInformation($"Script|ExportCategoriesToCsv|Exported {rows.Length} rows to: {filePath}");
            }
            catch (Exception ex)
            {
                engine.Log($"Script|ExportCategoriesToCsv|Exception thrown:{Environment.NewLine}{ex}");
            }
        }

        /// <summary>
        /// Wraps a CSV value in quotes if it contains a comma, quote, or newline.
        /// Escapes internal double-quotes by doubling them.
        /// </summary>
        private static string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                return "\"" + value.Replace("\"", "\"\"") + "\"";

            return value;
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(SecurePath.ConstructSecurePath(path)))
                Directory.CreateDirectory(SecurePath.ConstructSecurePath(path));
        }
    }
}