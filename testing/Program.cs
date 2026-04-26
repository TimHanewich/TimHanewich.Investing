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
            
        }
    }
}