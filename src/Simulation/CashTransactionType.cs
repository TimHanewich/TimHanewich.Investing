using System;

namespace TimHanewich.Investing.Simulation
{
    public enum CashTransactionType
    {
        Edit = 0,            //Cash deposited/withdrawn (just a change)
        Transaction = 1,     //cash used/suppplied from buy/sell of holding
        Expense = 2          //trading expense (commissio)
    }
}