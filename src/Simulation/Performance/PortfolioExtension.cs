using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yahoo.Finance;
using TimHanewich.Investing.Simulation;

namespace TimHanewich.Investing.Simulation.Performance
{
    public static class PortfolioExtension
    {
        public static async Task<PortflioPerformance> CalculatePerformanceAsync(this Portfolio portfolio)
        {
            Holding[] holdings = portfolio.Holdings();
            List<HoldingPerformance> performances = new List<HoldingPerformance>();
            foreach (Holding h in holdings)
            {
                Equity eq = Equity.Create(h.Symbol);
                await eq.DownloadSummaryAsync();
                performances.Add(new HoldingPerformance(h, eq.Summary.Price));
            }

            //Calculate expenses paid total
            float ExpensesPaid = 0.00f;
            foreach (CashTransaction ct in portfolio.CashTransactionLog)
            {
                if (ct.ChangeType == CashTransactionType.Expense)
                {
                    ExpensesPaid = ExpensesPaid + Math.Abs(ct.CashChange);
                }
            }

            PortflioPerformance pp = new PortflioPerformance();
            pp.HoldingPerformances = performances.ToArray();
            pp.ExpensesPaid = ExpensesPaid;
            return pp;
        }
    }
}