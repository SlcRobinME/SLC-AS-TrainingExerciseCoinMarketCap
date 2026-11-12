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
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using Newtonsoft.Json.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Utils.ExportImport;
	using Skyline.DataMiner.Utils.ExportImport.Factories;
	using Skyline.DataMiner.Utils.ExportImport.Writers;

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
	public class Script
    {
        private const int PID_CRYPTO_RESPONSE = 15;
        private const int PID_CATEGORIES = 1000;

        private const string BaseFolder = @"C:\Skyline DataMiner\Documents";

        /// <summary>
        /// The script entry point.
        /// </summary>
        /// <param name="engine">Link with SLAutomation process.</param>
        public void Run(IEngine engine)
        {
            var folderName = engine.GetScriptParam(2).Value;
            if (String.IsNullOrWhiteSpace(folderName))
            {
                engine.GenerateInformation("Enter a valid folder name.");
                return;
            }

            var folder = Path.Combine(BaseFolder, folderName);
            Directory.CreateDirectory(folder);
            var dms = engine.GetDms();

            var elements = dms.GetElements()
                .Where(element => element.Protocol.Name == "Exercise HTTP CoinMarketCap DFA")
                .ToList();

            if (elements.Count == 0)
            {
                engine.GenerateInformation("No CoinMarketCap elements were found using protocol: Exercise HTTP CoinMarketCap DFA.");
                return;
            }

            foreach (var element in elements)
            {
                var cryptoResponse = element.GetStandaloneParameter<string>(PID_CRYPTO_RESPONSE).GetValue();
                if (String.IsNullOrWhiteSpace(cryptoResponse))
                {
                    engine.GenerateInformation("No cryptocurrency response was found for " + element.Name + ".");
                    continue;
                }

                var cryptoRows = GetCryptoRows(engine, cryptoResponse, element);
                if (cryptoRows == null)
                    continue;

                var categoryRows = GetCategoryRows(element);
                if (categoryRows == null)
                    continue;

                var cryptoPath = ExportCsv(folder, element.Name, "Cryptocurrency", cryptoRows);
                var categoryPath = ExportCsv(folder, element.Name, "Categories", categoryRows);

                engine.GenerateInformation("Exported " + cryptoRows.Count + " cryptocurrency rows to " + cryptoPath);
                engine.GenerateInformation("Exported " + categoryRows.Count + " category rows to " + categoryPath);
            }
        }

        private static string ReadJsonValue(JToken value)
        {
            return value == null || value.Type == JTokenType.Null ? String.Empty : value.ToString();
        }

        public List<CryptoRow> GetCryptoRows(IEngine engine, string cryptoResponse, IDmsElement element)
        {
            var cryptoRows = new List<CryptoRow>();

            JObject response;
            try
            {
                response = JObject.Parse(cryptoResponse);
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                engine.GenerateInformation("The cryptocurrency response is not valid JSON for " + element.Name + ".");
                throw;
            }

            var cryptoData = response["data"] as JArray;
            if (cryptoData == null)
            {
                engine.GenerateInformation("The cryptocurrency response has no data array for " + element.Name + ".");
                return null;
            }

            foreach (var crypto in cryptoData)
            {
                var usdQuote = crypto["quote"]["USD"];
                cryptoRows.Add(new CryptoRow
                {
                    Name = ReadJsonValue(crypto["name"]),
                    Symbol = ReadJsonValue(crypto["symbol"]),
                    MaxSupply = ReadJsonValue(crypto["max_supply"]),
                    CirculatingSupply = ReadJsonValue(crypto["circulating_supply"]),
                    Price = ReadJsonValue(usdQuote["price"]),
                    MarketCap = ReadJsonValue(usdQuote["market_cap"]),
                    MarketCapDominance = ReadJsonValue(usdQuote?["market_cap_dominance"]),
                    Volume24h = ReadJsonValue(usdQuote["volume_24h"]),
                    PercentChange24h = ReadJsonValue(usdQuote["percent_change_24h"]),
                });
            }

            return cryptoRows;
        }

        public List<CategoriesRow> GetCategoryRows(IDmsElement element)
        {
            var categoryRows = new List<CategoriesRow>();

            var categoriesTable = element.GetTable(PID_CATEGORIES);
            var categoriesData = categoriesTable.GetData(0);

            foreach (var category in categoriesData.Values)
            {
                categoryRows.Add(new CategoriesRow
                {
                    CategoryName = Convert.ToString(category[1], CultureInfo.InvariantCulture),
                    Title = Convert.ToString(category[2], CultureInfo.InvariantCulture),
                    Description = Convert.ToString(category[3], CultureInfo.InvariantCulture),
                    NumberOfTokens = Convert.ToString(category[4], CultureInfo.InvariantCulture),
                    AvgPriceChange = Convert.ToString(category[5], CultureInfo.InvariantCulture),
                    CategoryMarketCap = Convert.ToString(category[6], CultureInfo.InvariantCulture),
                    MarketCapChange = Convert.ToString(category[7], CultureInfo.InvariantCulture),
                    Volume = Convert.ToString(category[8], CultureInfo.InvariantCulture),
                    VolumeChanges = Convert.ToString(category[9], CultureInfo.InvariantCulture),
                    LastUpdated = Convert.ToString(category[10], CultureInfo.InvariantCulture),
                });
            }

            return categoryRows;
        }

        private static string ExportCsv<T>(string folder, string elementName, string suffix, List<T> rows) where T : class, new()
        {
            var path = Path.Combine(folder, elementName + suffix + ".csv");

            Writer<T> writer = WriterFactory.GetWriter<T>(path);
            writer.Write(rows);

            File.WriteAllText(path, File.ReadAllText(path).Replace(';', ','));

            return path;
        }
    }
}