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
            Portfolio sp = new Portfolio();
            sp.TradeCost = 7;

            sp.EditCash(500000);
            sp.TradeAsync("BTC-USD", 1, TransactionType.Buy).Wait();

            Console.Write("Waiting... ");
            System.Threading.Tasks.Task.Delay(5_000).Wait();

            Console.WriteLine(JsonConvert.SerializeObject(sp, Formatting.Indented));


            sp.EquityTransactionLog[0].ExecutedPrice = 50.00f;
            Console.WriteLine("Calculating performance...");
            PortflioPerformance pp = sp.CalculatePerformanceAsync().Result;
            Console.WriteLine(JsonConvert.SerializeObject(pp, Formatting.Indented));
        }
    }
}