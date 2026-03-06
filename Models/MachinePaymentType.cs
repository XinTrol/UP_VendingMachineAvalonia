using System;
using System.Collections.Generic;

namespace UP_4.Models;

public partial class MachinePaymentType
{
    public int Id { get; set; }

    public Guid IdMachine { get; set; }

    public int IdPaymentType { get; set; }

    public virtual Machine IdMachineNavigation { get; set; } = null!;

    public virtual PaymentType IdPaymentTypeNavigation { get; set; } = null!;
}
