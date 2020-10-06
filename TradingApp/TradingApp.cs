using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using TradingApp.Helpers;

namespace TradingApp
{
    public static class TradingApp
    {
        public static HttpClient httpClient = new HttpClient();

        // instances of Kite and Ticker
        public static Kite kite = new Kite(MyAPIKey, Debug: true);

        // Initialize key and secret of your app
        public static string MyAPIKey = "abcdefghijklmnopqrstuvwxyz";
        public static string MySecret = "abcdefghijklmnopqrstuvwxyz";
        public static string MyUserId = "ZR0000";
        // persist these data in settings or db or file
        public static string MyPublicToken = "abcdefghijklmnopqrstuvwxyz";
        public static string MyAccessToken = "abcdefghijklmnopqrstuvwxyz";

        [FunctionName("TradingApp")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            //log.LogInformation("start"+ DateTime.Now);
            try
            {
                log.LogInformation("C# HTTP trigger function processed a request.");
                kite.SetAccessToken(MyAccessToken);

                List<Historical> previousDaydata = TradeHelpers.CallAPIPreviousDayOHLC(log);

                Dictionary<string, decimal> dataCPR = new Dictionary<string, decimal>();

                if (previousDaydata != null)
                {
                    dataCPR = TradeHelpers.CalculateCPR(previousDaydata, log);
                }
                else
                {
                    log.LogInformation("PreviousDaydata is null");
                }

                if (dataCPR != null)
                {
                    TradeHelpers.ExecuteTrade(dataCPR, log);
                }
                else
                {
                    log.LogInformation("CalculateCPR is null");
                }
            }
            catch (Exception ex)
            {
                log.LogInformation("Exception Main Function- " + ex.Message);
            }
            string responseMessage = "End";
            //log.LogInformation("end" + DateTime.Now);
            return new OkObjectResult(responseMessage);
        }
    }
}
