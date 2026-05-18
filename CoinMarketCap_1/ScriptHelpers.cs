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
    internal static class ScriptHelpers
    {
        public static string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Contains(";") || value.Contains("\"") || value.Contains("\n"))
                return "\"" + value.Replace("\"", "\"\"") + "\"";

            return value;
        }
    }
}