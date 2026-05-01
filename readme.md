# TimHanewich.Investing

A lightweight .NET library for simulating stock portfolio trading.

## Features

- **Simulated Trading** — Buy and sell stocks at specified prices
- **Transaction Logging** — Every trade and cash movement is recorded with timestamps
- **On-the-Fly Holdings** — Current positions and cost basis are computed from the transaction history (no stale state)
- **Trade Costs** — Configurable per-trade commission that is automatically deducted

## Installation

Install from [NuGet](https://www.nuget.org/packages/TimHanewich.Investing/):

```bash
dotnet add package TimHanewich.Investing
```

## Quick Start

```csharp
using TimHanewich.Investing;
using TimHanewich.Investing.Simulation;

// Create a portfolio and fund it
Portfolio portfolio = new Portfolio();
portfolio.EditCash(100_000.00f);

// Buy and sell stocks
portfolio.Buy("AAPL", 10, 150.00f);
portfolio.Buy("AAPL", 20, 175.00f);
portfolio.Sell("AAPL", 5, 200.00f);

// Check current holdings (computed from transaction log)
foreach (Holding h in portfolio.Holdings())
{
    Console.WriteLine($"{h.Symbol}: {h.Quantity} shares, cost basis per share ${h.CostBasisPerShare:F2}, total position cost ${h.CostBasisTotalPosition:F2}");
}
```

## Usage

### Creating a Portfolio

```csharp
Portfolio portfolio = new Portfolio();
portfolio.TradeCost = 4.95f; // optional commission per trade
```

### Depositing and Withdrawing Cash

```csharp
portfolio.EditCash(5000f);  // deposit $5,000
portfolio.EditCash(-500f);  // withdraw $500
```

Cash changes are logged as `CashTransaction` entries with type `CashTransactionType.Edit`.

### Buying Stocks

```csharp
portfolio.Buy("AAPL", 10, 150.00f);
```

This will:
1. Verify you have enough cash
2. Log a `HoldingTransaction` (Buy)
3. Deduct the cost from your cash balance (logged as `CashTransactionType.Transaction`)
4. Deduct the trade cost/commission if set (logged as `CashTransactionType.Expense`)

### Selling Stocks

```csharp
portfolio.Sell("AAPL", 5, 200.00f);
```

This will:
1. Verify you own enough shares to sell
2. Log a `HoldingTransaction` (Sell)
3. Credit the proceeds to your cash balance
4. Deduct the trade cost/commission if set

### Viewing Holdings

Holdings are computed on-the-fly from the transaction history. No stale state is stored.

```csharp
Holding[] holdings = portfolio.Holdings();

foreach (Holding h in holdings)
{
    Console.WriteLine($"{h.Symbol}: {h.Quantity} shares");
    Console.WriteLine($"  Cost basis per share: ${h.CostBasisPerShare:F2}");
    Console.WriteLine($"  Total position cost: ${h.CostBasisTotalPosition:F2}");
}
```

- `CostBasisPerShare` — weighted average price per share (total cost of all buys ÷ total shares bought)
- `CostBasisTotalPosition` — total cost of the current position (`CostBasisPerShare × Quantity`)

### Reviewing Transaction History

```csharp
// Holding (stock) transactions
foreach (HoldingTransaction ht in portfolio.HoldingTransactionLog)
{
    Console.WriteLine($"[{ht.TransactedAt}] {ht.OrderType} {ht.Quantity} {ht.Symbol} @ ${ht.ExecutedPrice:F2}");
}

// Cash transactions
foreach (CashTransaction ct in portfolio.CashTransactionLog)
{
    Console.WriteLine($"[{ct.TransactedAt}] {ct.ChangeType}: ${ct.CashChange:F2}");
}
```

## Full Example

```csharp
using TimHanewich.Investing;
using TimHanewich.Investing.Simulation;
using Newtonsoft.Json;

Portfolio p = new Portfolio();
p.EditCash(100_000.00f);

p.Buy("MSFT", 10, 100.00f);
p.Buy("MSFT", 20, 200.00f);
Console.WriteLine(JsonConvert.SerializeObject(p.Holdings(), Formatting.Indented));

p.Sell("MSFT", 25, 300.00f);
Console.WriteLine(JsonConvert.SerializeObject(p.Holdings(), Formatting.Indented));
```

## Class Reference

### `TimHanewich.Investing.Simulation`

| Class | Description |
| --- | --- |
| `Portfolio` | The main portfolio class. Manages cash, executes trades, computes holdings from transaction history. |
| `Holding` | A computed stock position: `Symbol`, `Quantity`, `CostBasisPerShare`, `CostBasisTotalPosition`. |
| `HoldingTransaction` | A recorded stock trade: `Symbol`, `Quantity`, `OrderType`, `ExecutedPrice`. Extends `Transaction`. |
| `CashTransaction` | A recorded cash movement: `CashChange`, `ChangeType`. Extends `Transaction`. |
| `Transaction` | Base class with `TransactedAt` (`DateTimeOffset`). |
| `TransactionType` | Enum: `Buy`, `Sell` |
| `CashTransactionType` | Enum: `Edit`, `Transaction`, `Expense` |
