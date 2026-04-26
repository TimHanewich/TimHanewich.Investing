using System;

namespace TimHanewich.Investing.Simulation.Performance
{
    public class HoldingPerformance
    {
        
        public Holding Holding {get; set;}     //The holding
        public float CurrentPrice {get; set;}  //The current price of the holding (i.e. stock price)

        public HoldingPerformance(Holding holding, float current_price)
        {
            Holding = holding;
            CurrentPrice = current_price;
        }

        public float Gain
        {
            get
            {
                return (CurrentPrice - Holding.CostBasis) * Holding.Quantity;
            }
        }

        public float PercentGain
        {
            get
            {
                return Gain / (Holding.Quantity * Holding.CostBasis);
            }
        }
    }
}