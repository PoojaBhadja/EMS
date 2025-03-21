using System;
using System.Collections.Generic;

namespace Models.ViewModels;

public partial class AccountExpenceIncomeVm
{
    public int AccountDetailId { get; set; }

    public string DisplayAccountName { get; set; }
    public string CardHolderName { get; set; }
    public string BankName { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal TotalBalance { get; set; }
    public decimal UsedAmount { get; set; }

}
