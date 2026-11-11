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
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Utils.ExportImport;
	using Skyline.DataMiner.Utils.ExportImport.Attributes;
	using Skyline.DataMiner.Utils.ExportImport.Factories;
	using Skyline.DataMiner.Utils.ExportImport.Writers;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
    public class Script
	{
        private const int PID_CRYPTO_RESPONSE = 15;
        private const int PID_CATEGORIES = 1000;
        private const int PID_CATEGORY_NAME = 1002;
        private const int PID_CATEGORY_TITLE = 1003;
        private const int PID_CATEGORY_DESCR = 1004;
        private const int PID_CATEGORY_NUM_TOKEN = 1005;
        private const int PID_CATEGORY_AVG_PRICE_CHANGE = 1006;
        private const int PID_CATEGORY_MARKET_CAP = 1007;
        private const int PID_CATEGORY_MARKET_CAP_CHANGE = 1008;
        private const int PID_CATEGORY_VOLUME = 1009;
        private const int PID_CATEGORY_VOLUME_CHANGE = 1010;
        private const int PID_CATEGORY_LAST_UPDATE = 1011;

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
                var cryptoRows = new List<CryptoRow>();
                var categoryRows = new List<CategoriesRow>();

                var cryptoResponse = element.GetStandaloneParameter<string>(PID_CRYPTO_RESPONSE).GetValue();
                if (String.IsNullOrWhiteSpace(cryptoResponse))
                {
                    engine.GenerateInformation("No cryptocurrency response was found for " + element.Name + ".");
                    continue;
                }

                JObject response;
                try
                {
                    response = JObject.Parse(cryptoResponse);
                }
                catch (Newtonsoft.Json.JsonReaderException)
                {
                    engine.GenerateInformation("The cryptocurrency response is not valid JSON for " + element.Name + ".");
                    continue;
                }

                var cryptoData = response["data"] as JArray;
                if (cryptoData == null)
                {
                    engine.GenerateInformation("The cryptocurrency response has no data array for " + element.Name + ".");
                    continue;
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

                var categoriesTable = element.GetTable(PID_CATEGORIES);
                var categoryNameColumn = categoriesTable.GetColumn<string>(PID_CATEGORY_NAME);
                var categoryTitleColumn = categoriesTable.GetColumn<string>(PID_CATEGORY_TITLE);
                var categoryDescriptionColumn = categoriesTable.GetColumn<string>(PID_CATEGORY_DESCR);
                var categoryNumTokensColumn = categoriesTable.GetColumn<double?>(PID_CATEGORY_NUM_TOKEN);
                var categoryAvgPriceChangeColumn = categoriesTable.GetColumn<double?>(PID_CATEGORY_AVG_PRICE_CHANGE);
                var categoryMarketCapColumn = categoriesTable.GetColumn<double?>(PID_CATEGORY_MARKET_CAP);
                var categoryMarketCapChangeColumn = categoriesTable.GetColumn<double?>(PID_CATEGORY_MARKET_CAP_CHANGE);
                var categoryVolumeColumn = categoriesTable.GetColumn<double?>(PID_CATEGORY_VOLUME);
                var categoryVolumeChangeColumn = categoriesTable.GetColumn<double?>(PID_CATEGORY_VOLUME_CHANGE);
                var categoryLastUpdatedColumn = categoriesTable.GetColumn<double?>(PID_CATEGORY_LAST_UPDATE);

                foreach (var primaryKey in categoriesTable.GetPrimaryKeys())
                {
                    categoryRows.Add(new CategoriesRow
                    {
                        CategoryName = categoryNameColumn.GetValue(primaryKey, KeyType.PrimaryKey),
                        Title = categoryTitleColumn.GetValue(primaryKey, KeyType.PrimaryKey),
                        Description = categoryDescriptionColumn.GetValue(primaryKey, KeyType.PrimaryKey),
                        NumberOfTokens = Convert.ToString(categoryNumTokensColumn.GetValue(primaryKey, KeyType.PrimaryKey), CultureInfo.InvariantCulture),
                        AvgPriceChange = Convert.ToString(categoryAvgPriceChangeColumn.GetValue(primaryKey, KeyType.PrimaryKey), CultureInfo.InvariantCulture),
                        CategoryMarketCap = Convert.ToString(categoryMarketCapColumn.GetValue(primaryKey, KeyType.PrimaryKey), CultureInfo.InvariantCulture),
                        MarketCapChange = Convert.ToString(categoryMarketCapChangeColumn.GetValue(primaryKey, KeyType.PrimaryKey), CultureInfo.InvariantCulture),
                        Volume = Convert.ToString(categoryVolumeColumn.GetValue(primaryKey, KeyType.PrimaryKey), CultureInfo.InvariantCulture),
                        VolumeChanges = Convert.ToString(categoryVolumeChangeColumn.GetValue(primaryKey, KeyType.PrimaryKey), CultureInfo.InvariantCulture),
                        LastUpdated = Convert.ToString(categoryLastUpdatedColumn.GetValue(primaryKey, KeyType.PrimaryKey), CultureInfo.InvariantCulture),
                    });
                }

                var cryptoPath = Path.Combine(folder, element.Name + "Cryptocurrency" + ".csv");
                Writer<CryptoRow> writer = WriterFactory.GetWriter<CryptoRow>(cryptoPath);
                writer.Write(cryptoRows);
                File.WriteAllText(cryptoPath, File.ReadAllText(cryptoPath).Replace(';', ','));

                var categoryPath = Path.Combine(folder, element.Name + "Categories" + ".csv");
                Writer<CategoriesRow> categoryWriter = WriterFactory.GetWriter<CategoriesRow>(categoryPath);
                categoryWriter.Write(categoryRows);
                File.WriteAllText(categoryPath, File.ReadAllText(categoryPath).Replace(';', ','));

                engine.GenerateInformation("Exported " + cryptoRows.Count + " cryptocurrency rows to " + cryptoPath);
                engine.GenerateInformation("Exported " + categoryRows.Count + " category rows to " + categoryPath);
            }
        }

        private static string ReadJsonValue(JToken value)
        {
            return value == null || value.Type == JTokenType.Null ? String.Empty : value.ToString();
        }

    }

    public class CryptoRow
    {
        [CsvHeader("Name")]
        public string Name { get; set; }

        [CsvHeader("Symbol")]
        public string Symbol { get; set; }

        [CsvHeader("Price")]
        public string Price { get; set; }

        [CsvHeader("MarketCap")]
        public string MarketCap { get; set; }

        [CsvHeader("MarketCapDominance")]
        public string MarketCapDominance { get; set; }

        [CsvHeader("Volume24h")]
        public string Volume24h { get; set; }

        [CsvHeader("PercentChange24h")]
        public string PercentChange24h { get; set; }

        [CsvHeader("CirculatingSupply")]
        public string CirculatingSupply { get; set; }

        [CsvHeader("MaxSupply")]
        public string MaxSupply { get; set; }
    }

    public class CategoriesRow
    {
        [CsvHeader("Category Name")]
        public string CategoryName { get; set; }

        [CsvHeader("Title")]
        public string Title { get; set; }

        [CsvHeader("Description")]
        public string Description { get; set; }

        [CsvHeader("Number Of Tokens")]
        public string NumberOfTokens { get; set; }

        [CsvHeader("Avg Price Change")]
        public string AvgPriceChange { get; set; }

        [CsvHeader("Category Market Cap")]
        public string CategoryMarketCap { get; set; }

        [CsvHeader("Market Cap Change")]
        public string MarketCapChange { get; set; }

        [CsvHeader("Volume")]
        public string Volume { get; set; }

        [CsvHeader("Volume Change")]
        public string VolumeChanges { get; set; }

        [CsvHeader("Last Updated")]
        public string LastUpdated { get; set; }
    }
}