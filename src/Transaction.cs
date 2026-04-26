using System;

namespace TimHanewich.Investing.Simulation
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