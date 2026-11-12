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
}