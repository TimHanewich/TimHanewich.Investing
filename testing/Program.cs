using System;
using TimHanewich.Investing;
using TimHanewich.Investing.Simulation;
using Newtonsoft.Json;
using TimHanewich.Investing.Simulation.Performance;

namespace Testing
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Test2().Wait();
            
        }

        public static async Task Test1()
        {
            Portfolio sp = new Portfolio();
            sp.TradeCost = 7;

            sp.EditCash(500000);
            sp.TradeAsync("BTC-USD", 1, TransactionType.Buy).Wait();

            Console.Write("Waiting... ");
            System.Threading.Tasks.Task.Delay(10_000).Wait();

            Console.WriteLine(JsonConvert.SerializeObject(sp, Formatting.Indented));


            Console.WriteLine("Calculating performance...");
            PortflioPerformance pp = sp.CalculatePerformanceAsync().Result;
            Console.WriteLine(JsonConvert.SerializeObject(pp, Formatting.Indented));
        }

        public static async Task Test2()
        {
            Portfolio p = new Portfolio();

            HoldingTransaction ht = new HoldingTransaction();
            ht.Symbol = "PG";
            ht.Quantity = 5;
            ht.ExecutedPrice = 100.00f;
            ht.TransactedAt = 0;
            p.HoldingTransactionLog.Add(ht);

            HoldingTransaction ht2 = new HoldingTransaction();
            ht2.Symbol = "PG";
            ht2.Quantity = 5;
            ht2.ExecutedPrice = 200.00f;
            ht2.TransactedAt = 0;
            p.HoldingTransactionLog.Add(ht2);

            Holding[] holdings = p.Holdings();
            Console.WriteLine(JsonConvert.SerializeObject(holdings, Formatting.Indented));

            PortflioPerformance pp = await p.CalculatePerformanceAsync();
            Console.WriteLine(JsonConvert.SerializeObject(pp, Formatting.Indented));
        }
    }
}