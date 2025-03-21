using System;
using System.Collections.Generic;

namespace Models.Entities;

public partial class AccountDetail
{
    public int Id { get; set; }

    public Guid Guid { get; set; }

    public string CardHolderName { get; set; } = null!;

    public int BankId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Bank Bank { get; set; } = null!;

    public virtual ICollection<ExpenceIncome> ExpenceIncomes { get; set; } = new List<ExpenceIncome>();
}
