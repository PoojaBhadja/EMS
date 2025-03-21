using System;
using System.Collections.Generic;

namespace Models.Entities;

public partial class Transaction
{
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }

    public Guid SubCategoryId { get; set; }

    public Guid CardHolderId { get; set; }

    public decimal Amount { get; set; }

    public int TransactionType { get; set; }

    public DateTime TransactionDate { get; set; }

    public string? Description { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsPaid { get; set; }

    public virtual CardHolder CardHolder { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual SubCategory SubCategory { get; set; } = null!;

    public virtual User? UpdatedByNavigation { get; set; }
}
