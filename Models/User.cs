using System;
using System.Collections.Generic;

namespace UP_4.Models;

public partial class User
{
    public Guid Id { get; set; }

    public string? Surname { get; set; }

    public string? Name { get; set; }

    public string? Patronymic { get; set; }

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public bool? IsManager { get; set; }

    public bool? IsEngineer { get; set; }

    public bool? IsOperator { get; set; }

    public int IdRole { get; set; }

    public string? Image { get; set; }

    public string Password { get; set; } = null!;

    public virtual Role IdRoleNavigation { get; set; } = null!;

    public virtual ICollection<Machine> MachineEngineerNavigations { get; set; } = new List<Machine>();

    public virtual ICollection<Machine> MachineManagerNavigations { get; set; } = new List<Machine>();

    public virtual ICollection<Machine> MachineTechnicianNavigations { get; set; } = new List<Machine>();

    public virtual ICollection<Machine> MachineUsers { get; set; } = new List<Machine>();

    public virtual ICollection<Maintenance> Maintenances { get; set; } = new List<Maintenance>();

    public string ShortName => $"{Surname} {GetInitial(Name)}. {GetInitial(Patronymic)}.";

    private string GetInitial(string name)
    {
        return string.IsNullOrEmpty(name) ? "" : name[0].ToString().ToUpper();
    }
}
