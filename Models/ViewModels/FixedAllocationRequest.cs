using System;
using System.Collections.Generic;

namespace Models.Entities;

public partial class FixedAllocationRequest
{
    public string Name { get; set; }

    public Guid CardHolderId { get; set; }

    public decimal Amount { get; set; }
}
