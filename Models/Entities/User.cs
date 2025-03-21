using System;
using System.Collections.Generic;

namespace Models.Entities;

public partial class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public Guid? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<CardHolder> CardHolderCreatedByNavigations { get; set; } = new List<CardHolder>();

    public virtual ICollection<CardHolder> CardHolderUpdatedByNavigations { get; set; } = new List<CardHolder>();

    public virtual ICollection<Transaction> TransactionCreatedByNavigations { get; set; } = new List<Transaction>();

    public virtual ICollection<Transaction> TransactionUpdatedByNavigations { get; set; } = new List<Transaction>();
}
