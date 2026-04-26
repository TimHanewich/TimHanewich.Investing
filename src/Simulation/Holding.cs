using System;

namespace TimHanewich.Investing.Simulation
{
    public class Holding
    {
        public string Symbol { get; set; }
        public int Quantity { get; set; }
        public float AverageCostBasis { get; set; }

        public Holding()
        {
            Symbol = "";
        }
    }
}