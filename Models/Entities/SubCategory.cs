using System;
using System.Collections.Generic;

namespace Models.Entities;

public partial class SubCategory
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public Guid CategoryId { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? IsActive { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
