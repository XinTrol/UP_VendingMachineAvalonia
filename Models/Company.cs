using System;
using System.Collections.Generic;

namespace UP_4.Models;

public partial class Company
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? ParentCompanyId { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? ContactPerson { get; set; }

    public DateOnly CreatedDate { get; set; }

    public string Inn { get; set; } = null!;

    public virtual Company? ParentCompany { get; set; }
    
    public virtual ICollection<Company> ChildCompanies { get; set; } = new List<Company>();

    public virtual ICollection<Machine> Machines { get; set; } = new List<Machine>();
}