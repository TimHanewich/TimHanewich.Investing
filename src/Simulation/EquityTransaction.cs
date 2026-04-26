using System;

namespace TimHanewich.Investing.Simulation
{
    public class EquityTransaction:Transaction
    {
        public string Symbol { get; set; }                   // stock symbol like "MSFT" or something like "BTC-USD"
        public int Quantity { get; set; }                    // quantity of shares
        public TransactionType OrderType { get; set; }
        public float ExecutedPrice {get; set;}

        public EquityTransaction()
        {
            Symbol = "";
        }
    }
}