using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MisasMinerSetup.Helpers
{
    public static class GarlicoinHelper
    {
        public static bool GetBalance(string wallet, out double balance)
        {
            balance = 0;

            using (var client = new WebClient())
            {
                try
                {
                    var downloadedString = client.DownloadString("https://explorer.grlc-bakery.fun/ext/getbalance/" + wallet);
                    if (downloadedString.Substring(0, 4) != "{\"er")
                    {
                        balance = Math.Round(Double.Parse(downloadedString, CultureInfo.InvariantCulture), 2);
                        return true;
                    }
                }
                catch
                {
                    // 404 Garlic Bakery Explorer down
                }
            }

            return false;
        }

        public static string GetCurrentPrice()
        {
            string retVal = String.Empty;

            using (var client = new WebClient())
            {
                try
                {
                    var downloadedWorth = client.DownloadString("https://api.coinmarketcap.com/v1/ticker/garlicoin/"); //Check GRLC current worth in $
                    var toBeSearched = "\"price_usd\": \"";
                    retVal = downloadedWorth.Substring(downloadedWorth.IndexOf(toBeSearched) + toBeSearched.Length, 4);
                }
                catch
                {
                    // 404 downloadedWorth
                }
            }
            
            return retVal;
        }
    }
}
