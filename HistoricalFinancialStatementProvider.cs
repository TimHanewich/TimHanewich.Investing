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

namespace TimHanewich.Investing
{
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