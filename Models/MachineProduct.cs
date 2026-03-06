using System;
using System.Collections.Generic;

namespace UP_4.Models;

public partial class MachineProduct
{
    public int Id { get; set; }

    public Guid MachineId { get; set; }

    public Guid ProductId { get; set; }

    public decimal Price { get; set; }

    public int MinStock { get; set; }

    public int QuantityAvailable { get; set; }

    public virtual Machine Machine { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
