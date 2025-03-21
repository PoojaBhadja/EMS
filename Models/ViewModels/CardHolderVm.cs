using System;
using System.Collections.Generic;

namespace Models.Entities;

public partial class CardHolderInput
{

    public string CardHolderName { get; set; } = null!;

    public string DisplayName { get; set; } = null!;
    public decimal Balance { get; set; }

    public bool IsActive { get; set; }
}
