using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using Yahoo.Finance;
using Newtonsoft.Json;


namespace TimHanewich.Investing
{
    public class BatchStockDataProvider
    {
        public async Task<EquitySummaryData[]> GetBatchEquitySummaryData(string[] stock_symbols)
        {
            HttpClient hc = new HttpClient();
            Random r = new Random();
            List<Task<string>> Tasks = new List<Task<string>>();

            foreach (string s in stock_symbols)
            {
                Tasks.Add(GetStockDataFromAzureFunction(hc, s, r));
            }

            string[] data_from_az = await Task.WhenAll(Tasks);

            //Convert
            List<EquitySummaryData> summaryDatas = new List<EquitySummaryData>();
            foreach (string s in data_from_az)
            {
                try
                {
                    summaryDatas.Add(JsonConvert.DeserializeObject<EquitySummaryData>(s));
                }
                catch
                {

                }
            }

            //Return
            return summaryDatas.ToArray();

        }

        private async Task<string> GetStockDataFromAzureFunction(HttpClient hc, string stock, Random r)
        {
            await Task.Delay(r.Next(0, 5000));
            string resp = await hc.GetStringAsync("https://papertradesim.azurewebsites.net/api/StockSummaryData?symbol=" + stock);
            return resp;
        }
    }

}