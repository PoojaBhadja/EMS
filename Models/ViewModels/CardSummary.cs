using System;
using System.Collections.Generic;

namespace Models.Entities;

public class CardSummary
{
    public decimal? TotalBalance { get; set; }
    public decimal? TotalAllocationAmount { get; set; }
    public decimal? RemainAmount { get; set; }
    public List<GroupedFixedAllocation> GroupedFixedAllocations { get; set; } = new List<GroupedFixedAllocation>();
}