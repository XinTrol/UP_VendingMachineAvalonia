using System;
using System.Collections.Generic;

namespace UP_4.Models;

public partial class Status
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Machine> Machines { get; set; } = new List<Machine>();
}
