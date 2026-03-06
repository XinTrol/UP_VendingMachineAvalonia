using System;
using System.Collections.Generic;

namespace UP_4.Models;

public partial class Maintenance
{
    public int Id { get; set; }

    public DateOnly ServiceDate { get; set; }

    public string? IssuesFound { get; set; }

    public string? WorkDescription { get; set; }

    public Guid VendingMachineId { get; set; }

    public Guid IdUser { get; set; }

    public virtual User IdUserNavigation { get; set; } = null!;

    public virtual Machine VendingMachine { get; set; } = null!;
}
