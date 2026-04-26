using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yahoo.Finance;
using TimHanewich.Investing;

namespace TimHanewich.Investing.Simulation
{
    public class SimulatedPortfolio
    {
        public Guid Id { get; set;  }
        public DateTimeOffset CreatedOn { get; set;  }
        public string Owner { get; set; }
        public float Cash { get; set; }
        public List<EquityTransaction> EquityTransactionLog { get; set; }
        public List<CashTransaction> CashTransactionLog { get; set; }
        public float TradeCost {get; set;} //i.e. commission

        public EquityHolding[] EquityHoldings
        {
            get
            {
                Dictionary<string, int> quantities = new Dictionary<string, int>();
                foreach (EquityTransaction et in EquityTransactionLog)
                {
                    string sym = et.Symbol.ToUpper().Trim();
                    if (!quantities.ContainsKey(sym))
                    {
                        quantities[sym] = 0;
                    }
                    if (et.OrderType == TransactionType.Buy)
                    {
                        quantities[sym] += et.Quantity;
                    }
                    else if (et.OrderType == TransactionType.Sell)
                    {
                        quantities[sym] -= et.Quantity;
                    }
                }
                List<EquityHolding> holdings = new List<EquityHolding>();
                foreach (var kvp in quantities)
                {
                    if (kvp.Value > 0)
                    {
                        EquityHolding eh = new EquityHolding();
                        eh.Symbol = kvp.Key;
                        eh.Quantity = kvp.Value;
                        holdings.Add(eh);
                    }
                }
                return holdings.ToArray();
            }
        }

        public static SimulatedPortfolio Create(string ownername = "")
        {
            SimulatedPortfolio ReturnInstance = new SimulatedPortfolio();

            ReturnInstance.Id = Guid.NewGuid();
            ReturnInstance.EquityTransactionLog = new List<EquityTransaction>();
            ReturnInstance.CashTransactionLog = new List<CashTransaction>();
            ReturnInstance.CreatedOn = DateTimeOffset.Now;
            ReturnInstance.Owner = ownername;

            return ReturnInstance;
        }

        public void EditCash(float cash_edit, CashTransactionType ctt = CashTransactionType.Edit)
        {
            //Error check
            if (cash_edit < 0)
            {
                if (Math.Abs(cash_edit) > Cash)
                {
                    throw new Exception("Trying to withdraw more cash than portfolio has.  Trying to withdraw: $" + cash_edit.ToString("#,##0.00") + ", portfolio has $" + Cash.ToString("#,##0.00"));
                }
            }

            //Perform the edit!
            Cash = Cash + cash_edit;
            CashTransaction ct = new CashTransaction();
            ct.ChangeType = ctt;
            ct.UpdateTransactionTime();
            ct.CashChange = cash_edit;
            CashTransactionLog.Add(ct);
        }

        public async Task TradeEquityAsync(string symbol, int quantity, TransactionType order_type)
        {
            Equity e = Equity.Create(symbol);
            try
            {
                await e.DownloadSummaryAsync();
            }
            catch
            {
                throw new Exception("Critical error while fetching equity '" + symbol + "'.  Does this equity exist?");
            }
            

            if (order_type == TransactionType.Buy)
            {
                //Be sure we have enough cash to buy
                float cash_needed = e.Summary.Price * quantity;
                if (Cash < cash_needed)
                {
                    throw new Exception("You do not have enough cash to execute this buy order of " + symbol.ToUpper() + ".  Cash needed: $" + cash_needed.ToString("#,##0.00") + ".  Cash balance: $" + Cash.ToString("#,##0.00"));
                }

                //Log the transaction
                EquityTransaction et = new EquityTransaction();
                et.TransactedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                et.Symbol = symbol.ToUpper().Trim();
                et.OrderType = TransactionType.Buy;
                et.Quantity = quantity;
                et.ExecutedPrice = e.Summary.Price;
                EquityTransactionLog.Add(et);

                //Deduct cash
                Cash = Cash - cash_needed;

            }
            else if (order_type == TransactionType.Sell)
            {
                //Find our holding
                EquityHolding eh = null;
                foreach (EquityHolding ceh in EquityHoldings)
                {
                    if (ceh.Symbol.ToUpper() == symbol.ToUpper())
                    {
                        eh = ceh;
                    }
                }

                //Throw an error if we do not have any of those shares.
                if (eh == null)
                {
                    throw new Exception("You do not have any shares of " + symbol.ToUpper() + " to sell.");
                }

                //Throw an error if we do not have enough shares
                if (eh.Quantity < quantity)
                {
                    throw new Exception("You do not have " + quantity.ToString() + " shares to sell!  You only have " + eh.Quantity.ToString() + " shares.");
                }

                //Log the transaction
                EquityTransaction et = new EquityTransaction();
                et.UpdateTransactionTime();
                et.Symbol = symbol.ToUpper().Trim();
                et.OrderType = TransactionType.Sell;
                et.Quantity = quantity;
                et.ExecutedPrice = e.Summary.Price;
                EquityTransactionLog.Add(et);

                //Credit cash
                Cash = Cash + (quantity * e.Summary.Price);
            }

            //Take out the commission (if any)
            if (TradeCost > 0)
            {
                EditCash(TradeCost * -1, CashTransactionType.TradingRelatedCharge);
            }
        }

        public async Task<float> CalculateNetProfitAsync()
        {
            //Get list of all stocks
            List<string> stocks = new List<string>();
            foreach (EquityHolding eh in EquityHoldings)
            {
                if (stocks.Contains(eh.Symbol.Trim().ToUpper()) == false)
                {
                    stocks.Add(eh.Symbol.Trim().ToUpper());
                }
            }
           
            //Get the equity data
            List<EquitySummaryData> esdList = new List<EquitySummaryData>();
            foreach (string symbol in stocks)
            {
                Equity eq = Equity.Create(symbol);
                await eq.DownloadSummaryAsync();
                esdList.Add(eq.Summary);
            }
            EquitySummaryData[] esds = esdList.ToArray();

            //Add up our portfolio value
            float PortValue = 0;
            foreach (EquityHolding eh in EquityHoldings)
            {
                foreach (EquitySummaryData esd in esds)
                {
                    if (esd.StockSymbol.ToUpper().Trim() == eh.Symbol.ToUpper().Trim())
                    {
                        float thisstockval = eh.Quantity * esd.Price;
                        PortValue = PortValue + thisstockval;
                    }
                }
            }
            
            //Find how much cash was invested
            float CashInvested = 0;
            foreach (CashTransaction ct in CashTransactionLog)
            {
                if (ct.ChangeType == CashTransactionType.Edit) //Only count it if it was added. If it was a charge for trading (commission), do not count it.
                {
                    CashInvested = CashInvested + ct.CashChange;
                }
            }


            return PortValue + Cash - CashInvested;
            
        }

        public async Task<EquityHoldingPerformance[]> CalculateEquityHoldingPerformances()
        {
            List<EquityHoldingPerformance> ToReturn = new List<EquityHoldingPerformance>();

            //Check if there are no holdings
            if (EquityHoldings.Length == 0)
            {
                return ToReturn.ToArray(); 
            }

            //Get a list of all equities
            List<string> Stocks = new List<string>();
            foreach (EquityHolding eh in EquityHoldings)
            {
                if (Stocks.Contains(eh.Symbol.Trim().ToUpper()) == false)
                {
                    Stocks.Add(eh.Symbol.Trim().ToUpper());
                }
            }

            //Get all stock data
            List<EquitySummaryData> esdList = new List<EquitySummaryData>();
            foreach (string symbol in Stocks)
            {
                Equity eq = Equity.Create(symbol);
                await eq.DownloadSummaryAsync();
                esdList.Add(eq.Summary);
            }
            EquitySummaryData[] ESDs = esdList.ToArray();

            //Check if we have all of them
            if (Stocks.Count != ESDs.Length)
            {
                string DontHave = "";
                foreach (string s in Stocks)
                {
                    bool HasIt = false;
                    foreach (EquitySummaryData esd in ESDs)
                    {
                        if (esd.StockSymbol.Trim().ToUpper() == s.Trim().ToUpper())
                        {
                            HasIt = true;
                        }
                    }
                    if (HasIt == false)
                    {
                        DontHave = DontHave + s + ",";
                    }
                }
                throw new Exception("Unable to calculate all of the performance logs for your holdings because access of data failed: " + DontHave);
            }


            //Calculate the performances
            foreach (string s in Stocks)
            {
                EquityHoldingPerformance ehp = new EquityHoldingPerformance();

                //Find the right Equity Data
                EquitySummaryData esd = null;
                foreach (EquitySummaryData es in ESDs)
                {
                    if (es.StockSymbol.Trim().ToUpper() == s.Trim().ToUpper())
                    {
                        esd = es;
                    }
                }
                if (esd == null)
                {
                    throw new Exception("Fatal error while calculating performance for " + s +".");
                }

                //Find the right equity holding
                EquityHolding eh = null;
                foreach (EquityHolding h in EquityHoldings)
                {
                    if (h.Symbol.Trim().ToUpper() == s.Trim().ToUpper())
                    {
                        eh = h;
                    }
                }
                if (eh == null)
                {
                    throw new Exception("Fatal error while find holding for " + s +".");
                }

                //Add symbol
                ehp.Symbol = s.Trim().ToUpper();

                //Get dollars invested
                ehp.DollarsInvested = CalculateAverageCostBasis(s) * eh.Quantity;

                //Get holding value
                ehp.HoldingValue = eh.Quantity * esd.Price;

                //Get dollar profit
                ehp.DollarProfit = ehp.HoldingValue - ehp.DollarsInvested;

                //Get percent profit
                ehp.PercentProfit = ehp.DollarProfit / ehp.DollarsInvested;

                ToReturn.Add(ehp);

            }


            //Return them
            return ToReturn.ToArray();
        }
    
        private float CalculateAverageCostBasis(string symbol)
        {
            float totalCost = 0;
            int totalQuantity = 0;
            foreach (EquityTransaction et in EquityTransactionLog)
            {
                if (et.Symbol.ToUpper().Trim() == symbol.ToUpper().Trim())
                {
                    if (et.OrderType == TransactionType.Buy)
                    {
                        totalCost += et.ExecutedPrice * et.Quantity;
                        totalQuantity += et.Quantity;
                    }
                    else if (et.OrderType == TransactionType.Sell)
                    {
                        totalQuantity -= et.Quantity;
                    }
                }
            }
            if (totalQuantity == 0)
            {
                return 0;
            }
            return totalCost / totalQuantity;
        }
    }
}