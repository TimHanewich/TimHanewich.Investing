using System;

namespace TimHanewich.Investing.Simulation
{
    public class CashTransaction:Transaction
    {
        public float CashChange { get; set; }
        public CashTransactionType ChangeType {get; set;}
    }
}