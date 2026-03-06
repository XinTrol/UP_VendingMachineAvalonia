using System;
using System.Collections.Generic;

namespace UP_4.Models;

public partial class Mode
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Machine> MachineCriticalThresholdTemplateNavigations { get; set; } = new List<Machine>();

    public virtual ICollection<Machine> MachineNotificationTemplateNavigations { get; set; } = new List<Machine>();

    public virtual ICollection<Machine> MachineWorkModeNavigations { get; set; } = new List<Machine>();
}
