using System;
using System.Collections.Generic;

namespace Models.Entities;

public partial class Bank
{
    public int Id { get; set; }

    public string BankName { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual ICollection<AccountDetail> AccountDetails { get; set; } = new List<AccountDetail>();
}
