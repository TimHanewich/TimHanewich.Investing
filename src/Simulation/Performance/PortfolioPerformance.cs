using System;

namespace TimHanewich.Investing.Simulation.Performance
{
    public class PortflioPerformance
    {
        public HoldingPerformance[] HoldingPerformances {get; set;}

        public PortflioPerformance()
        {
            HoldingPerformances = new HoldingPerformance[]{};
        }

        public float Profit
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
    }
}