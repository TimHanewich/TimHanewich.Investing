using System;

namespace SimulatedInvesting
{
    public class EquityTransaction:Transaction
    {
        public string StockSymbol { get; set; }
        public int Quantity { get; set; }
        public TransactionType OrderType { get; set; }
        public float PriceExecutedAt {get; set;}
    }
}