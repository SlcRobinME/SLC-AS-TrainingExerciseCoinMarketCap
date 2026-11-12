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
    using Skyline.DataMiner.Utils.ExportImport.Attributes;

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