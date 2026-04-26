using System;

namespace TimHanewich.Investing.Simulation
{
    public class Holding
    {
        public string Symbol { get; set; }
        public int Quantity { get; set; }
        public float CostBasis { get; set; } // weighted average price per share, calculated as total cost of all buys / total shares bought

        public Holding()
        {
            Symbol = "";
        }
    }
}