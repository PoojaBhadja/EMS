using System;
using System.Collections.Generic;

namespace Models.Entities;

public partial class CardHolder
{
    public Guid Id { get; set; }

    public string CardHolderName { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public decimal Balance { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? IsActive { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<FixedAllocation> FixedAllocations { get; set; } = new List<FixedAllocation>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual User? UpdatedByNavigation { get; set; }
}
