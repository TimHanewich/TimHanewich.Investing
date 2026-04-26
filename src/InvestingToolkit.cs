using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TimHanewich.Investing
{
    public class InvestingToolkit
    {
        public static async Task<string[]> GetEquityGroupAsync(EquityGroup group)
        {
            if (group == EquityGroup.SP500)
            {
                HttpClient hc = new HttpClient();
                HttpResponseMessage hrm = await hc.GetAsync("https://en.wikipedia.org/wiki/List_of_S%26P_500_companies");
                string content = await hrm.Content.ReadAsStringAsync();

                int loc1 = 0;
                int loc2 = 0;
                List<string> Splitter = new List<string>();

                loc1 = content.IndexOf("<table class=");
                loc1 = content.IndexOf("<tbody>", loc1 + 1);
                loc2 = content.IndexOf("</tbody>", loc1 + 1);
                string tablecontent = content.Substring(loc1, loc2 - loc1 - 1);

                //Split into rows
                Splitter.Clear();
                Splitter.Add("<tr>");
                string[] rows = tablecontent.Split(Splitter.ToArray(), StringSplitOptions.None);
                Splitter.Clear();

                //Extract the symbol from each row
                List<string> StockSymbols = new List<string>();
                int t = 0;
                for (t = 2; t < rows.Length; t++)
                {
                    Splitter.Add("<td>");
                    string[] cols = rows[t].Split(Splitter.ToArray(), StringSplitOptions.None);
                    loc1 = cols[1].IndexOf(">");
                    loc2 = cols[1].IndexOf("<", loc1 + 1);
                    string symbol = cols[1].Substring(loc1 + 1, loc2 - loc1 - 1);
                    StockSymbols.Add(symbol);
                }
                return StockSymbols.ToArray();
            }
            else
            {
                throw new Exception("Method for collecting stock grouping '" + group.ToString() + "' does not exist.");
            }
        }
    }
}