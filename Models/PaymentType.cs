using System;
using System.Collections.Generic;

namespace UP_4.Models;

public partial class PaymentType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<MachinePaymentType> MachinePaymentTypes { get; set; } = new List<MachinePaymentType>();
}
