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
        private const string ElementName = "coin-market";
        private const string BaseExportPath = @"C:\Skyline DataMiner\Documents";

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

                var elements = dms.GetElements().Where(e => e.Name.Contains(ElementName)).ToList();

                if (!elements.Any())
                {
                    engine.ExitFail($"No elements found with this name '{ElementName}'.");
                    return;
                }

                foreach (var element in elements)
                {
                    ExportElementToCsv(engine, element,exportPath);
                }
            }
            catch(Exception ex)
            {
                engine.Log($"Script|Run|Exception thrown: {Environment.NewLine}{ex}");
            }
		}

        private void ExportElementToCsv(IEngine engine, IDmsElement element, string exportPath)
        {
            try
            {
                engine.GenerateInformation($"Script|ExportElementToCsv|Exporting element: {element.Name}");

                var table = element.GetTable(1000);
                var rows = table.GetRows();

                if(rows == null || rows.Length == 0)
                {
                    engine.Log($"Script|ExportElementToCsv|No rows found in Latest Listings table for element: {element.Name}");
                    return;
                }

                var csvRows = new List<string>();

                csvRows.Add("ID,Name,Symbol,Rank,Circulating Supply,Max Supply,Price USD,Market Cap,Volume 24h,Percent Change 1h,Percent Change 24h,Percent Change 7d");

                foreach (var row in rows)
                {
                    csvRows.Add(string.Join(",", new[]
                    {
                        Convert.ToString(row[0]),
                        Convert.ToString(row[1]),
                        Convert.ToString(row[2]),
                        Convert.ToString(row[2]),
                        Convert.ToString(row[3]),
                        Convert.ToString(row[4]),
                        Convert.ToString(row[5]),
                        Convert.ToString(row[6]),
                        Convert.ToString(row[7]),
                        Convert.ToString(row[8]),
                        Convert.ToString(row[9]),
                        Convert.ToString(row[10]),
                        Convert.ToString(row[11]),
                    }));
                }

                if (!Directory.Exists(exportPath))
                {
                    Directory.CreateDirectory(exportPath);
                }

                string filePath = SecurePath.ConstructSecurePath(exportPath, $"{element.Name}.csv");

                File.WriteAllLines(filePath, csvRows);

                engine.GenerateInformation($"Script|ExportElementToCsv|Successfully exported {rows.Count()} rows to: {filePath}");
            }
            catch (Exception ex)
            {
                engine.Log($"Script|ExportElementToCsv|Exception thrown:{Environment.NewLine}{ex}");
            }
        }
	}
}