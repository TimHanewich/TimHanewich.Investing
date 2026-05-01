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
            p.Buy("TIMH", 10, 100.00f);
            p.Buy("TIMH", 20, 200.00f);
            Console.WriteLine(JsonConvert.SerializeObject(p.Holdings(), Formatting.Indented));

            p.Sell("TIMH", 25, 300.00f);
            Console.WriteLine(JsonConvert.SerializeObject(p.Holdings(), Formatting.Indented));
        }

    }
}