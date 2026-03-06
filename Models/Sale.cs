using System;
using System.Collections.Generic;

namespace UP_4.Models;

public partial class Sale
{
    public int Id { get; set; }

    public DateTime SaleTimestamp { get; set; }

    public Guid ProductId { get; set; }

    public Guid MachineId { get; set; }

    public decimal TotalPrice { get; set; }

    public int Quantity { get; set; }

    public int PaymentMethod { get; set; }

    public virtual Machine Machine { get; set; } = null!;

    public virtual PaymentMethod PaymentMethodNavigation { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
