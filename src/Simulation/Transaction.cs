using System;

namespace TimHanewich.Investing.Simulation
{
    public class Transaction
    {
        public long TransactedAt { get; set; } //UnixTimeSeconds

        public void UpdateTransactionTime()
        {
            TransactedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}