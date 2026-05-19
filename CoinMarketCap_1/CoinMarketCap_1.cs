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
	using Skyline.DataMiner.Utils.SecureCoding.SecureIO;

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
	public class Script
    {
        private const string ProtocolName = "Exercise HTTP CoinMarketCap SCO";
        private const string BaseExportPath = @"C:\Skyline DataMiner\Documents";
        private const int LatestListingsTableId = 1000;
        private const int CategoriesTableId = 2000;
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
                    ExportLatestListingsToCsv(engine, element,exportPath);
                    ExportCategoriesToCsv(engine, element, exportPath);
                }
            }
            catch(Exception ex)
            {
                engine.Log($"Script|Run|Exception thrown: {Environment.NewLine}{ex}");
            }
		}

        private void ExportLatestListingsToCsv(IEngine engine, IDmsElement element, string exportPath)
        {
            try
            {
                engine.GenerateInformation($"Script|ExportLatestListings|Exporting element: {element.Name}");

                var table = element.GetTable(LatestListingsTableId);

                var rows = table.GetRows();
                int rowCount = rows.Length;

                if (rows == null || rowCount == 0)
                {
                    engine.Log($"Script|ExportCategories|No rows found in Latest Listings table for element: {element.Name}");
                    return;
                }

                var csvRows = new List<string>();

                csvRows.Add("ID;Name;Symbol;Rank;Circulating Supply;Max Supply;Price USD;Market Cap;Volume 24h;Percent Change 1h;Percent Change 24h;Percent Change 7d");

                for (int i = 0; i < rowCount; i++)
                {
                    var cells = LatestListingsColumnPids.Select(pid =>
                                                  table.GetColumn<string>(pid).GetValue(
                                                    Convert.ToString(rows[i][0]),KeyType.PrimaryKey));

                    csvRows.Add(string.Join(";", cells));
                }

                if (!Directory.Exists(exportPath))
                {
                    Directory.CreateDirectory(exportPath);
                }

                string filePath = SecurePath.ConstructSecurePath(exportPath, $"{element.Name}_LatestListings.csv");

                File.WriteAllLines(filePath, csvRows);

                engine.GenerateInformation($"Script|ExportLatestListings|Successfully exported {rows.Count()} rows to: {filePath}");
            }
            catch (Exception ex)
            {
                engine.Log($"Script|ExportLatestListings|Exception thrown:{Environment.NewLine}{ex}");
            }
        }

        private void ExportCategoriesToCsv(IEngine engine, IDmsElement element, string exportPath)
        {
            try
            {
                engine.GenerateInformation($"Script|ExportCategories|Exporting element: {element.Name}");

                var table = element.GetTable(CategoriesTableId);
                var rows = table.GetRows();
                var rowCount = rows.Length;

                if (rows == null || rowCount == 0)
                {
                    engine.Log($"Script|ExportCategories|No rows found in Latest Listings table for element: {element.Name}");
                    return;
                }

                var csvRows = new List<string>();

                csvRows.Add("ID;Name;Number of Tokens;Average Price Change;Market Cap;Market Cap Change;Volume;Volume Change;Last Updated");

                for (int i = 0; i < rowCount; i++)
                {
                    var cells = CategoriesColumnPids.Select(pid =>
                                                 table.GetColumn<string>(pid).GetValue(
                                                   Convert.ToString(rows[i][0]), KeyType.PrimaryKey));

                    csvRows.Add(string.Join(";", cells));
                }

                if (!Directory.Exists(exportPath))
                {
                    Directory.CreateDirectory(exportPath);
                }

                string filePath = SecurePath.ConstructSecurePath(exportPath, $"{element.Name}_Categories.csv");

                File.WriteAllLines(filePath, csvRows);

                engine.GenerateInformation($"Script|ExportCategories|Successfully exported {rows.Count()} rows to: {filePath}");
            }
            catch (Exception ex)
            {
                engine.Log($"Script|ExportCategories|Exception thrown:{Environment.NewLine}{ex}");
            }
        }
    }
}