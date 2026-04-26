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
    }
}