using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yahoo.Finance;
using TimHanewich.Investing;

namespace TimHanewich.Investing.Simulation
{
    public class Portfolio
    {
        public float Cash { get; set; }
        public List<EquityTransaction> HoldingTransactionLog { get; set; }
        public List<CashTransaction> CashTransactionLog { get; set; }
        public float TradeCost {get; set;} //i.e. commission

        public Portfolio()
        {
            Cash = 0.00f;
            HoldingTransactionLog = new List<EquityTransaction>();
            CashTransactionLog = new List<CashTransaction>();
            TradeCost = 0.00f;
        }

        public Holding[] Holdings()
        {
            Dictionary<string, int> quantities = new Dictionary<string, int>();
            Dictionary<string, float> totalCost = new Dictionary<string, float>();
            Dictionary<string, int> totalBought = new Dictionary<string, int>();
            foreach (EquityTransaction et in HoldingTransactionLog)
            {
                string sym = et.Symbol.ToUpper().Trim();
                if (!quantities.ContainsKey(sym))
                {
                    quantities[sym] = 0;
                    totalCost[sym] = 0;
                    totalBought[sym] = 0;
                }
                if (et.OrderType == TransactionType.Buy)
                {
                    quantities[sym] += et.Quantity;
                    totalCost[sym] += et.ExecutedPrice * et.Quantity;
                    totalBought[sym] += et.Quantity;
                }
                else if (et.OrderType == TransactionType.Sell)
                {
                    quantities[sym] -= et.Quantity;
                }
            }
            List<Holding> holdings = new List<Holding>();
            foreach (var kvp in quantities)
            {
                if (kvp.Value > 0)
                {
                    Holding eh = new Holding();
                    eh.Symbol = kvp.Key;
                    eh.Quantity = kvp.Value;
                    eh.CostBasis = totalBought[kvp.Key] > 0 ? totalCost[kvp.Key] / totalBought[kvp.Key] : 0;
                    holdings.Add(eh);
                }
            }
            return holdings.ToArray();
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

        public async Task TradeAsync(string symbol, int quantity, TransactionType order_type)
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
                HoldingTransactionLog.Add(et);

                //Deduct cash
                EditCash(cash_needed * -1, CashTransactionType.Transaction);

            }
            else if (order_type == TransactionType.Sell)
            {
                //Find our holding
                Holding eh = null;
                foreach (Holding ceh in Holdings())
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
                HoldingTransactionLog.Add(et);

                //Credit cash
                EditCash(quantity * e.Summary.Price, CashTransactionType.Transaction);
            }

            //Take out the commission (if any)
            if (TradeCost > 0)
            {
                EditCash(TradeCost * -1, CashTransactionType.Expense);
            }
        }
    }
}