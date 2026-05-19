/*
****************************************************************************
*  Copyright (c),  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

Revision History:

DATE		VERSION		AUTHOR			COMMENTS

11/01/2024	1.0.0.1		XXX, Skyline	Initial version
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
	using Skyline.DataMiner.Utils.ExportImport.Writers;
	using Skyline.DataMiner.Utils.SecureCoding.SecureIO;

	public class LatestListingsRow
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Symbol { get; set; }

        public string Rank { get; set; }

        public string CirculatingSupply { get; set; }

        public string MaxSupply { get; set; }

        public string PriceUsd { get; set; }

        public string MarketCap { get; set; }

        public string Volume24h { get; set; }

        public string PercentChange1h { get; set; }

        public string PercentChange24h { get; set; }

        public string PercentChange7d { get; set; }
    }

	public class CategoriesRow
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string NumberOfTokens { get; set; }

        public string AveragePriceChange { get; set; }

        public string MarketCap { get; set; }

        public string MarketCapChange { get; set; }

        public string Volume { get; set; }

        public string VolumeChange { get; set; }

        public string LastUpdated { get; set; }
    }

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
	public class Script
    {
        private const string ProtocolName = "Exercise HTTP CoinMarketCap SCO";
        private const string BaseExportPath = @"C:\Skyline DataMiner\Documents";
        private const int LatestListingsTableId = 1000;
        private const int CategoriesTableId = 2000;
        private const string LatestListingsHeader = "ID;Name;Symbol;Rank;Circulating Supply;Max Supply;Price USD;Market Cap;Volume 24h;Percent Change 1h;Percent Change 24h;Percent Change 7d";
        private const string CategoriesHeader = "ID;Name;Number of Tokens;Average Price Change;Market Cap;Market Cap Change;Volume;Volume Change;Last Updated";
        private static readonly int[] LatestListingsColumnPids = { 1001, 1002, 1003, 1004, 1005, 1006, 1007, 1008, 1009, 1010, 1011, 1012 };
        private static readonly int[] CategoriesColumnPids = { 2001, 2002, 2003, 2004, 2005, 2006, 2007, 2008, 2009 };

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

                var elements = dms.GetElements().Where(e => e.Protocol.Name == "Exercise HTTP CoinMarketCap SCO").ToList();

                if (!elements.Any())
                {
                    engine.ExitFail($"No elements found with this protocol '{ProtocolName}'.");
                    return;
                }

                foreach (var element in elements)
                {
                    ExportTableToCsv(engine, element, exportPath, LatestListingsTableId, "LatestListings", MapLatestListingsRow);
                    ExportTableToCsv(engine, element, exportPath, CategoriesTableId, "Categories", MapCategoriesRow);
                }
            }
            catch(Exception ex)
            {
                engine.Log($"Script|Run|Exception thrown: {Environment.NewLine}{ex}");
            }
		}

        private void ExportTableToCsv<T>(IEngine engine, IDmsElement element, string exportPath, int tableId, string tableName, Func<IDmsTable,string,T> rowMapper) where T : class, new()
        {
            try
            {
                engine.GenerateInformation($"Script|ExportLatestListings|Exporting {tableName}");

                var table = element.GetTable(tableId);
                var rows = table.GetRows();
                int rowCount = rows.Length;

                if (rows == null || rowCount == 0)
                {
                    engine.Log($"Script|ExportCategories|No rows found in Latest Listings table for element: {element.Name}");
                    return;
                }

                var exportRows = new List<T>();

                for (int i = 0; i < rowCount; i++)
                {
                    string primaryKey = Convert.ToString(rows[i][0]);
                    exportRows.Add(rowMapper(table, primaryKey));
                }

                if (!Directory.Exists(exportPath))
                    Directory.CreateDirectory(exportPath);

                string filePath = SecurePath.ConstructSecurePath(exportPath, $"{element.Name}_{tableName}.csv");
                var writer = WriterFactory.GetWriter<T>(filePath);
                writer.Write(exportRows);
                engine.GenerateInformation($"Script|ExportTableToCsv|Successfully exported {rows.Count()} rows to: {filePath}");
            }
            catch (Exception ex)
            {
                engine.Log($"Script|ExportTableToCsv|Exception thrown:{ex.Message}");
            }
        }

        private LatestListingsRow MapLatestListingsRow(IDmsTable table, string primaryKey)
        {
            return new LatestListingsRow
            {
                Id = table.GetColumn<string>(LatestListingsColumnPids[0]).GetValue(primaryKey, KeyType.PrimaryKey),
                Name = table.GetColumn<string>(LatestListingsColumnPids[1]).GetValue(primaryKey, KeyType.PrimaryKey),
                Symbol = table.GetColumn<string>(LatestListingsColumnPids[2]).GetValue(primaryKey, KeyType.PrimaryKey),
                Rank = table.GetColumn<string>(LatestListingsColumnPids[3]).GetValue(primaryKey, KeyType.PrimaryKey),
                CirculatingSupply = table.GetColumn<string>(LatestListingsColumnPids[4]).GetValue(primaryKey, KeyType.PrimaryKey),
                MaxSupply = table.GetColumn<string>(LatestListingsColumnPids[5]).GetValue(primaryKey, KeyType.PrimaryKey),
                PriceUsd = table.GetColumn<string>(LatestListingsColumnPids[6]).GetValue(primaryKey, KeyType.PrimaryKey),
                MarketCap = table.GetColumn<string>(LatestListingsColumnPids[7]).GetValue(primaryKey, KeyType.PrimaryKey),
                Volume24h = table.GetColumn<string>(LatestListingsColumnPids[8]).GetValue(primaryKey, KeyType.PrimaryKey),
                PercentChange1h = table.GetColumn<string>(LatestListingsColumnPids[9]).GetValue(primaryKey, KeyType.PrimaryKey),
                PercentChange24h = table.GetColumn<string>(LatestListingsColumnPids[10]).GetValue(primaryKey, KeyType.PrimaryKey),
                PercentChange7d = table.GetColumn<string>(LatestListingsColumnPids[11]).GetValue(primaryKey, KeyType.PrimaryKey),
            };
        }

        private CategoriesRow MapCategoriesRow(IDmsTable table, string primaryKey)
        {
            return new CategoriesRow
            {
                Id = table.GetColumn<string>(CategoriesColumnPids[0]).GetValue(primaryKey, KeyType.PrimaryKey),
                Name = table.GetColumn<string>(CategoriesColumnPids[1]).GetValue(primaryKey, KeyType.PrimaryKey),
                NumberOfTokens = table.GetColumn<string>(CategoriesColumnPids[2]).GetValue(primaryKey, KeyType.PrimaryKey),
                AveragePriceChange = table.GetColumn<string>(CategoriesColumnPids[3]).GetValue(primaryKey, KeyType.PrimaryKey),
                MarketCap = table.GetColumn<string>(CategoriesColumnPids[4]).GetValue(primaryKey, KeyType.PrimaryKey),
                MarketCapChange = table.GetColumn<string>(CategoriesColumnPids[5]).GetValue(primaryKey, KeyType.PrimaryKey),
                Volume = table.GetColumn<string>(CategoriesColumnPids[6]).GetValue(primaryKey, KeyType.PrimaryKey),
                VolumeChange = table.GetColumn<string>(CategoriesColumnPids[7]).GetValue(primaryKey, KeyType.PrimaryKey),
                LastUpdated = table.GetColumn<string>(CategoriesColumnPids[8]).GetValue(primaryKey, KeyType.PrimaryKey),
            };
        }
    }
}