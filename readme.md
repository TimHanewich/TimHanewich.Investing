# TimHanewich.Investing

A .NET library for simulating stock portfolio trading with live market data from Yahoo Finance.

## Features

- **Simulated Trading** — Buy and sell stocks using live market prices
- **Transaction Logging** — Every trade and cash movement is recorded with Unix timestamps
- **On-the-Fly Holdings** — Current positions and cost basis are computed from the transaction history (no stale state)
- **Performance Tracking** — Calculate gains, losses, and profit on your portfolio using real-time prices
- **Trade Costs** — Configurable per-trade expense (commission) that is automatically deducted

## Quick Start

```csharp
using TimHanewich.Investing.Simulation;
using TimHanewich.Investing.Simulation.Performance;

// Create a portfolio and fund it
Portfolio portfolio = new Portfolio();
portfolio.TradeCost = 4.95f; // $4.95 commission per trade
portfolio.EditCash(10000f); // deposit $10,000

// Buy some stocks (fetches live prices from Yahoo Finance)
await portfolio.TradeAsync("AAPL", 10, TransactionType.Buy);
await portfolio.TradeAsync("MSFT", 5, TransactionType.Buy);

// Check current holdings (computed from transaction log)
foreach (Holding h in portfolio.Holdings())
{
    Console.WriteLine($"{h.Symbol}: {h.Quantity} shares, cost basis ${h.CostBasis:F2}");
}

// Print the full portfolio summary
Console.WriteLine(portfolio.ToString());
```

## Usage

### Creating a Portfolio

```csharp
Portfolio portfolio = new Portfolio();
portfolio.TradeCost = 9.99f; // optional commission per trade
```

### Depositing and Withdrawing Cash

```csharp
portfolio.EditCash(5000f);  // deposit $5,000
portfolio.EditCash(-500f);  // withdraw $500
```

Cash changes are logged as `CashTransaction` entries with type `CashTransactionType.Edit`.

### Buying Stocks

```csharp
await portfolio.TradeAsync("AAPL", 10, TransactionType.Buy);
```

This will:
1. Fetch the current price of AAPL from Yahoo Finance
2. Verify you have enough cash
3. Log a `HoldingTransaction` (Buy)
4. Deduct the cost from your cash balance (logged as `CashTransactionType.Transaction`)
5. Deduct the trade cost/commission if set (logged as `CashTransactionType.Expense`)

### Selling Stocks

```csharp
await portfolio.TradeAsync("AAPL", 5, TransactionType.Sell);
```

This will:
1. Fetch the current price of AAPL
2. Verify you own enough shares to sell
3. Log a `HoldingTransaction` (Sell)
4. Credit the proceeds to your cash balance
5. Deduct the trade cost/commission if set

### Viewing Holdings

Holdings are computed on-the-fly from the transaction history. No stale state is stored.

```csharp
Holding[] holdings = portfolio.Holdings();

foreach (Holding h in holdings)
{
    Console.WriteLine($"{h.Symbol}: {h.Quantity} shares @ ${h.CostBasis:F2} avg cost");
}
```

The `CostBasis` is the weighted average price per share, calculated as the total cost of all buy transactions divided by the total shares bought.

### Checking Performance

Use the `CalculatePerformanceAsync()` extension method to get a real-time performance snapshot:

```csharp
PortflioPerformance perf = await portfolio.CalculatePerformanceAsync();

Console.WriteLine($"Gain on holdings: ${perf.Gain:F2}");
Console.WriteLine($"Expenses paid: ${perf.ExpensesPaid:F2}");
Console.WriteLine($"Net profit: ${perf.Profit:F2}");

// Per-holding breakdown
foreach (HoldingPerformance hp in perf.HoldingPerformances)
{
    Console.WriteLine($"{hp.Holding.Symbol}: ${hp.Gain:F2} ({hp.PercentGain:P2})");
}

// Or just print everything
Console.WriteLine(perf.ToString());
```

### Reviewing Transaction History

```csharp
// Holding (stock) transactions
foreach (HoldingTransaction ht in portfolio.HoldingTransactionLog)
{
    DateTimeOffset time = DateTimeOffset.FromUnixTimeSeconds(ht.TransactedAt);
    Console.WriteLine($"[{time}] {ht.OrderType} {ht.Quantity} {ht.Symbol} @ ${ht.ExecutedPrice:F2}");
}

// Cash transactions
foreach (CashTransaction ct in portfolio.CashTransactionLog)
{
    DateTimeOffset time = DateTimeOffset.FromUnixTimeSeconds(ct.TransactedAt);
    Console.WriteLine($"[{time}] {ct.ChangeType}: ${ct.CashChange:F2}");
}
```

### Printing a Portfolio Summary

Both `Portfolio` and `PortflioPerformance` have `ToString()` overrides for quick inspection:

```csharp
Console.WriteLine(portfolio.ToString());
```

```
=== Portfolio ===
Cash: $5,000.00

--- Holdings (2) ---
  AAPL: 10 shares @ $150.00 cost basis
  MSFT: 5 shares @ $400.00 cost basis

--- Transactions (2) ---
  [2026-04-26 21:01:00] Buy 10 AAPL @ $150.00
  [2026-04-26 21:02:00] Buy 5 MSFT @ $400.00
```

## Class Reference

### `TimHanewich.Investing.Simulation`

| Class | Description |
| --- | --- |
| `Portfolio` | The main portfolio class. Manages cash, executes trades, computes holdings from transaction history. |
| `Holding` | A computed stock position: `Symbol`, `Quantity`, `CostBasis`. |
| `HoldingTransaction` | A recorded stock trade: `Symbol`, `Quantity`, `OrderType`, `ExecutedPrice`. Extends `Transaction`. |
| `CashTransaction` | A recorded cash movement: `CashChange`, `ChangeType`. Extends `Transaction`. |
| `Transaction` | Base class with `TransactedAt` (Unix timestamp in seconds). |
| `TransactionType` | Enum: `Buy`, `Sell` |
| `CashTransactionType` | Enum: `Edit`, `Transaction`, `Expense` |

### `TimHanewich.Investing.Simulation.Performance`

| Class | Description |
| --- | --- |
| `PortflioPerformance` | Portfolio-level performance: `HoldingPerformances[]`, `ExpensesPaid`, computed `Gain` and `Profit`. |
| `HoldingPerformance` | Per-holding performance: wraps a `Holding`, adds `CurrentPrice`, computed `Gain` and `PercentGain`. |
| `PortfolioExtension` | Extension method `CalculatePerformanceAsync()` on `Portfolio`. |
