using System;
using System.Collections.Generic;

namespace Models.Entities;

public class GroupedFixedAllocation
{
    public Guid CardHolderId { get; set; }
    public string CardHolderName { get; set; }
    public string DisplayName { get; set; }
    public decimal? TotalBalance { get; set; }
    public decimal? TotalAllocationAmount { get; set; }
    public decimal? RemainAmount { get; set; }
    public List<FixedAllocation>? Allocations { get; set; } = new List<FixedAllocation>();
}