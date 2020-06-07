using System;
using System.Threading.Tasks;
using Xbrl;
using SecuritiesExchangeCommission.Edgar;
using Xbrl.FinancialStatement;
using Xbrl.Helpers;
using System.Collections.Generic;
using System.IO;
using TimHanewich.Csv;
using System.Reflection;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace TimHanewich.Investing
{
    /// <summary>
    /// Provides all historical financial statements (10-K or 10-Q) of a company and then writes this to a CSV file.
    /// </summary>
    public class HistoricalFinancialStatementProvider
    {
        public async Task<FinancialStatement[]> GetFinancialStatementsAsync(string symbol, string filing_type)
        {
            EdgarSearch es = await EdgarSearch.CreateAsync(symbol, filing_type, null, EdgarSearchOwnershipFilter.exclude, EdgarSearchResultsPerPage.Entries40);

            //Get a list of the filings
            List<EdgarSearchResult> Results = new List<EdgarSearchResult>();
            foreach (EdgarSearchResult esr in es.Results)
            {
                if (esr.Filing.Trim().ToLower() == filing_type.Trim().ToLower())
                {
                    Results.Add(esr);
                }
            }

            //Make financial statments for each
            List<FinancialStatement> FinancialStatements = new List<FinancialStatement>();
            foreach (EdgarSearchResult esr in Results)
            {
                try
                {
                    Stream s = await esr.DownloadXbrlDocumentAsync();
                    XbrlInstanceDocument doc = XbrlInstanceDocument.Create(s);
                    FinancialStatement fs = doc.CreateFinancialStatement();
                    FinancialStatements.Add(fs);
                }
                catch
                {

                }
            }
            

            //Arrange from oldest to newest
            List<FinancialStatement> Arranged = new List<FinancialStatement>();
            do
            {
                FinancialStatement Winner = FinancialStatements[0];
                foreach (FinancialStatement fs in FinancialStatements)
                {
                    if (fs.PeriodEnd < Winner.PeriodEnd)
                    {
                        Winner = fs;
                    }
                }
                Arranged.Add(Winner);
                FinancialStatements.Remove(Winner);
            } while (FinancialStatements.Count > 0);


            return Arranged.ToArray();
        }

        /// <summary>
        /// Uses the Luca API to download financial statements in parallel.
        /// </summary>
        public async Task<FinancialStatement[]> GetFinancialStatementsAsyncV2(string symbol, string filing_type)
        {
            //Get query for API
            string filing_type_for_api = "";
            if (filing_type.ToLower().Trim() == "10-k")
            {
                filing_type_for_api = "10k";
            }
            else if (filing_type.ToLower().Trim() == "10-q")
            {
                filing_type_for_api = "10q";
            }
            else
            {
                throw new Exception("Filing '" + filing_type + "' is not valid filing with a financials. Options: '10-K' or '10-Q'.");
            }
            
            EdgarSearch es = await EdgarSearch.CreateAsync(symbol, filing_type, null, EdgarSearchOwnershipFilter.exclude, EdgarSearchResultsPerPage.Entries40);

            //Get a list of the filings
            List<EdgarSearchResult> Results = new List<EdgarSearchResult>();
            foreach (EdgarSearchResult esr in es.Results)
            {
                if (esr.Filing.Trim().ToLower() == filing_type.Trim().ToLower())
                {
                    Results.Add(esr);
                }
            }


            //Set them all up
            HttpClient hc = new HttpClient();
            hc.Timeout = new TimeSpan(1, 0, 0);
            List<Task<HttpResponseMessage>> LucaCalls = new List<Task<HttpResponseMessage>>();
            foreach (EdgarSearchResult esr in Results)
            {
                string url = "https://projectluca.azurewebsites.net/api/GetFinancials?";
                DateTime BeforeDate = esr.FilingDate.AddDays(3);
                string before_date_string = BeforeDate.Month.ToString("00") + BeforeDate.Day.ToString("00") + BeforeDate.Year.ToString("0000");
                url = url + "symbol=" + symbol;
                url = url + "&filing=" + filing_type_for_api;
                url = url + "&before=" + before_date_string;
                LucaCalls.Add(hc.GetAsync(url));
            }

            //Trigger them
            HttpResponseMessage[] Responses = await Task.WhenAll(LucaCalls);

            //Parse them all into statements
            List<FinancialStatement> statements = new List<FinancialStatement>();
            foreach (HttpResponseMessage hrm in Responses)
            {
                if (hrm.StatusCode == HttpStatusCode.OK)
                {
                    try
                    {
                        string JsonData = await hrm.Content.ReadAsStringAsync();
                        FinancialStatement fs = JsonConvert.DeserializeObject<FinancialStatement>(JsonData);
                        statements.Add(fs);
                    }
                    catch
                    {

                    }
                }
            }

            return statements.ToArray();
        }

        public string PrintFinancialStatements(FinancialStatement[] statements)
        {
            CsvFile csv = new CsvFile();

            //Write title
            List<string> PropertiesToWrite = new List<string>();
            List<PropertyInfo> ToWriteProperties = new List<PropertyInfo>();
            DataRow header = csv.AddNewRow();
            PropertyInfo[] info = statements[0].GetType().GetProperties();
            foreach (PropertyInfo pi in info)
            {
                if (pi.PropertyType.IsClass == false)
                {
                    header.Values.Add(pi.Name);
                    PropertiesToWrite.Add(pi.Name);
                    ToWriteProperties.Add(pi);
                }
            }

            //Write all of the properties
            foreach (FinancialStatement fs in statements)
            {
                DataRow dr = csv.AddNewRow();
                foreach (PropertyInfo pi in ToWriteProperties)
                {
                    try
                    {
                        dr.Values.Add(pi.GetValue(fs).ToString());
                    }
                    catch
                    {
                        dr.Values.Add("-");
                    }
                    
                }
            }

            return csv.GenerateAsCsvFileContent();
        }

    }
}