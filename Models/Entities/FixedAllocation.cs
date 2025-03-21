using System;
using System.Collections.Generic;

namespace Models.Entities;

public partial class FixedAllocation
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public Guid CardHolderId { get; set; }

    public decimal Amount { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? IsActive { get; set; }

    public virtual CardHolder CardHolder { get; set; } = null!;
}
