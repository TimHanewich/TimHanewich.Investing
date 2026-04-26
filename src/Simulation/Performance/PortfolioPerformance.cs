using System;

namespace TimHanewich.Investing.Simulation.Performance
{
    public class PortflioPerformance
    {
        public HoldingPerformance[] HoldingPerformances {get; set;}
        public float ExpensesPaid {get; set;}

        public PortflioPerformance()
        {
            HoldingPerformances = new HoldingPerformance[]{};
        }

        //Focuses only on holding gains/losses
        public float Gain
        {
            get
            {
                float ToReturn = 0.00f;
                foreach (HoldingPerformance hp in HoldingPerformances)
                {
                    ToReturn = ToReturn + hp.Gain;
                }
                return ToReturn;
            }
        }

        //Takes into account trade costs as well (i.e. commission)
        public float Profit
        {
            get
            {
                return Gain - ExpensesPaid;
            }
        }
        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine("=== Portfolio Performance ===");
            sb.AppendLine("Gains on Holdings: $" + Gain.ToString("#,##0.00"));
            sb.AppendLine("Trading Expenses Paid (i.e. commission): $" + ExpensesPaid.ToString("#,##0.00"));
            sb.AppendLine("Profit: $" + Profit.ToString("#,##0.00"));

            sb.AppendLine("");
            sb.AppendLine("--- Holdings (" + HoldingPerformances.Length + ") ---");
            if (HoldingPerformances.Length == 0)
            {
                sb.AppendLine("  (none)");
            }
            foreach (HoldingPerformance hp in HoldingPerformances)
            {
                string sign = hp.Gain >= 0 ? "+" : "";
                string pctSign = hp.PercentGain >= 0 ? "+" : "";
                sb.AppendLine("  " + hp.Holding.Symbol + ": " + hp.Holding.Quantity + " shares | Cost Basis: $" + hp.Holding.CostBasis.ToString("#,##0.00") + " | Current: $" + hp.CurrentPrice.ToString("#,##0.00") + " | Gain: " + sign + "$" + hp.Gain.ToString("#,##0.00") + " (" + pctSign + (hp.PercentGain * 100).ToString("0.00") + "%)");
            }

            return sb.ToString().TrimEnd();
        }
    }
}