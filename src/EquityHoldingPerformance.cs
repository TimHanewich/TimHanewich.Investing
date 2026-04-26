using System;

namespace SimulatedInvesting
{
    //This represents gain/loss performance for an individual stock.
    //Placed this on hold as of 5/24/2020 while a FIFO/LIFO method for recording stock purchases and sales is created in the core SimulatedPortfolio class.
    public class EquityHoldingPerformance
    {
        public string Symbol {get; set;}
        public float DollarsInvested {get; set;}
        public float HoldingValue {get; set;}
        public float DollarProfit {get; set;}
        public float PercentProfit {get; set;}
    }
}