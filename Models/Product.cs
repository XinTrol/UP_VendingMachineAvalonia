using System;
using System.Collections.Generic;

namespace UP_4.Models;

public partial class Product
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal SalesTrend { get; set; }

    public virtual ICollection<MachineProduct> MachineProducts { get; set; } = new List<MachineProduct>();

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
