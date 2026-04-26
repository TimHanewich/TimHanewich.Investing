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

                HoldingPerformance hp = new HoldingPerformance();
                hp.Symbol = h.Symbol;
                hp.Quantity = h.Quantity;
                hp.CostBasis = h.CostBasis;
                hp.CurrentPrice = eq.Summary.Price;
                performances.Add(hp);
            }

            PortflioPerformance pp = new PortflioPerformance();
            pp.HoldingPerformances = performances.ToArray();
            return pp;
        }
    }
}