using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TradingApp.Helpers
{
    public static class TradeHelpers
    {
        // call api to get previous day OHLC - this is not changing for a day so need to optimise to call once and save somewhere
        public static List<Historical> CallAPIPreviousDayOHLC(ILogger log)
        {
            try
            {
                DateTime today = DateTime.Now;
                DateTime prevWorkday = today.AddDays(-1);
                while (prevWorkday.ToString("dddd") == "Saturday" || prevWorkday.ToString("dddd") == "Sunday")
                {
                    prevWorkday = prevWorkday.AddDays(-1);
                }

                List<Historical> previousDayOHLC = TradingApp.kite.GetHistoricalData(
                 InstrumentToken: "260105",
                 FromDate: prevWorkday,   // 2016-01-01 12:50:00 AM
                 ToDate: prevWorkday,    // 2016-01-01 01:10:00 PM
                 Interval: Constants.INTERVAL_DAY,
                 Continuous: false
             );

                return previousDayOHLC;
            }
            catch (Exception ex)
            {
                log.LogInformation("Exception CallAPIPreviousDayOHLC - " + ex.Message);
            }
            return null;
        }

        private static async Task<HttpResponseMessage> CallKiteApi(string baseUri, string subUri, ILogger log)
        {
            try
            {
                TradingApp.httpClient.BaseAddress = new Uri(baseUri);
                TradingApp.httpClient.DefaultRequestHeaders.Accept.Clear();
                TradingApp.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //GET Method  
                HttpResponseMessage response = await TradingApp.httpClient.GetAsync(subUri);
                return response;
            }
            catch (Exception ex)
            {
                log.LogInformation("Exception CallAPIPreviousDayOHLC - " + ex.Message);
            }
            return null;
        }

        // calculate CPR
        public static Dictionary<string, decimal> CalculateCPR(List<Historical> previousDaydata, ILogger log)
        {
            try
            {
                Dictionary<string, decimal> dataCPR = new Dictionary<string, decimal>();

                decimal centralPivot = (previousDaydata[0].High + previousDaydata[0].Low + previousDaydata[0].Close) / 3;

                decimal bottomCentralPivot = (previousDaydata[0].High + previousDaydata[0].Low) / 2;

                decimal topCentralPivot = (centralPivot - bottomCentralPivot) + centralPivot;

                dataCPR.Add("centralPivot", centralPivot);
                dataCPR.Add("bottomCentralPivot", bottomCentralPivot);
                dataCPR.Add("topCentralPivot", topCentralPivot);

                return dataCPR;
            }
            catch (Exception ex)
            {
                log.LogInformation("Exception CalculateCPR - " + ex.Message);
            }
            return null;
        }

        // call api to get LTP data
        public static Dictionary<string, OHLC> CallAPILTP(ILogger log)
        {
            try
            {
                Dictionary<string, OHLC> ltp = TradingApp.kite.GetOHLC(InstrumentId: new string[] { "NSE:BANKNIFTY" });
                return ltp;
            }
            catch (Exception ex)
            {
                log.LogInformation("Exception CallAPIPreviousDayOHLC - " + ex.Message);
            }
            return null;
        }

        // call api to get last 5 minutes candle data
        public static List<Historical> CallAPIGetFiveMinutesCandleData(ILogger log)
        {
            try
            {
                // i was thinking to use GetHistoricalData api for every 5 minutes candle data and its OHLC
                // pls suggect best approch

                List<Historical> FiveMinutesCandleData = TradingApp.kite.GetHistoricalData(
                 InstrumentToken: "260105",
                 FromDate: new DateTime(2016, 1, 1, 12, 50, 0),   // here will pass latest 5minute difference or pls suggest
                 ToDate: new DateTime(2016, 1, 1, 12, 50, 0),
                 Interval: Constants.INTERVAL_DAY,
                 Continuous: false
             );

                return FiveMinutesCandleData;
            }
            catch (Exception ex)
            {
                log.LogInformation("Exception CallAPIPreviousDayOHLC - " + ex.Message);
            }
            return null;
        }


        // execute trade 
        public static void ExecuteTrade(Dictionary<string, decimal> dataCPR, ILogger log)
        {
            try
            {

                // case - check previous 5min candle close if it is greater than TC then buy

                List<Historical> previous5MinData = TradeHelpers.CallAPIGetFiveMinutesCandleData(log);


                Dictionary<string, OHLC> lastTradedPrice = TradeHelpers.CallAPILTP(log);

                int last5MinHigh = Convert.ToInt32(previous5MinData[0].High);
                int last5MinClose = Convert.ToInt32(previous5MinData[0].Close);

                int _topCentralPivot = Convert.ToInt32(dataCPR["topCentralPivot"]);

                int ltp = Convert.ToInt32(lastTradedPrice["NSE:BANKNIFTY"]);

                if ((last5MinClose >= _topCentralPivot) && (ltp > _topCentralPivot))
                {
                    // execute buy trade immediatly at ltp 
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}
