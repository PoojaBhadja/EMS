using Models.Entities;
using System;
using System.Collections.Generic;

namespace Models.ViewModels;

public partial class TransactionRequest
{
    public Guid CategoryId { get; set; }

    public Guid SubCategoryId { get; set; }

    public Guid CardHolderId { get; set; }

    public decimal Amount { get; set; }

    public int TransactionType { get; set; }

    public DateTime TransactionDate { get; set; }

    public string? Description { get; set; }
    public bool IsPaid { get; set; }
}