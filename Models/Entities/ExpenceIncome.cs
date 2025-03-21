using System;
using System.Collections.Generic;

namespace Models.Entities;

public partial class ExpenceIncome
{
    public int Id { get; set; }

    public Guid Guid { get; set; }

    public string? Description { get; set; }

    public int CategoryId { get; set; }

    public int AccountDetailId { get; set; }

    public decimal Amount { get; set; }

    public int TransactionType { get; set; }

    public DateTime SpentDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool IsDeleted { get; set; }

    public virtual AccountDetail AccountDetail { get; set; } = null!;
}
