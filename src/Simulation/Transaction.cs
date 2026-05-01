using System;

namespace TimHanewich.Investing.Simulation
{
    public class Transaction
    {
        public DateTimeOffset TransactedAt { get; set; } //UnixTimeSeconds

        public void UpdateTransactionTime()
        {
            TransactedAt = DateTimeOffset.Now;
        }
    }
}