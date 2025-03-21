using System;
using System.Collections.Generic;

namespace Models.Entities;

public partial class FixedCutOff
{
    public int Id { get; set; }

    public Guid Guid { get; set; }

    public string Name { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }
}
