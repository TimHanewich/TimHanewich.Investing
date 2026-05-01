using System;
using TimHanewich.Investing;
using TimHanewich.Investing.Simulation;
using System;
using Newtonsoft.Json;

namespace Testing
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Test1().Wait();
            
        }

        public static async Task Test1()
        {
            Portfolio p = new Portfolio();
            p.EditCash(100_000.00f, CashTransactionType.Edit);
            p.Trade("TIMH", 10, 100.00f, TransactionType.Buy);
            p.Trade("TIMH", 20, 200.00f, TransactionType.Buy);
            Console.WriteLine(JsonConvert.SerializeObject(p.Holdings(), Formatting.Indented));

            p.Trade("TIMH", 25, 300.00f, TransactionType.Sell);
            Console.WriteLine(JsonConvert.SerializeObject(p.Holdings(), Formatting.Indented));
        }

    }
}