using Models.Entities;
using System;
using System.Collections.Generic;

namespace Models.ViewModels;

public partial class DashboardVm
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpence { get; set; }
    public decimal TotalInvestment { get; set; }
    public List<ExpenceBrekdownVm> ExpenceBreakdowns { get; set; }
    public List<IncomeBrekdownVm> IncomeBreakdowns { get; set; }
    public List<Transaction> Transactions { get; set; }
    public List<CashFlowVm> CashFlowVm { get; set; }
}

public partial class ExpenceBrekdownVm
{
    public string Name { get; set; }
    public decimal Amount { get; set; }
}

public partial class IncomeBrekdownVm
{
    public string Name { get; set; }
    public decimal Amount { get; set; }
}


public partial class CashFlowVm
{
    public string Month { get; set; }
    public decimal IncomeAmount { get; set; }
    public decimal ExpenceAmount { get; set; }
    public decimal InvestedAmount { get; set; }
}