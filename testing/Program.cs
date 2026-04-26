using System;
using TimHanewich.Investing;
using TimHanewich.Investing.Simulation;
using Newtonsoft.Json;
namespace Testing
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SimulatedPortfolio sp = SimulatedPortfolio.Create("TimHanewich");
            sp.TradeCost = 7;

            sp.EditCash(500000);
            sp.TradeEquityAsync("BTC-USD", 1, TransactionType.Buy).Wait();

            Console.Write("Waiting... ");
            System.Threading.Tasks.Task.Delay(3_000).Wait();

            Console.WriteLine(JsonConvert.SerializeObject(sp, Formatting.Indented));
            
            float f = sp.CalculateNetProfitAsync().Result;
            Console.WriteLine(f);
        }
    }
}