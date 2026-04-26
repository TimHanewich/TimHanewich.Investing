using System;

namespace SimulatedInvesting
{
    public class Transaction
    {
        public DateTimeOffset TransactedOn { get; set; }

        public void UpdateTransactionTime()
        {
            TransactedOn = DateTimeOffset.Now;
        }
    }
}