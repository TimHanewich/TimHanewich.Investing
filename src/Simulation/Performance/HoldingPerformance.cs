using System;

namespace TimHanewich.Investing.Simulation.Performance
{
    public class HoldingPerformance : Holding
    {
        public float CurrentPrice {get; set;}

        public float Gain
        {
            get
            {
                return (CurrentPrice - AverageCostBasis) * Quantity;
            }
        }

        public float PercentGain
        {
            get
            {
                return Gain / (Quantity * AverageCostBasis);
            }
        }
    }
}