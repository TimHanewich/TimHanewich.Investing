using System;

namespace SimulatedInvesting
{
    public class CashTransaction:Transaction
    {
        public float CashChange { get; set; }
        public CashTransactionType ChangeType {get; set;}
    }
}