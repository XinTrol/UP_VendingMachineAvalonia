using Avalonia.Controls;
using System;
using System.Collections.Generic;
using Tmds.DBus.Protocol;

namespace UP_4.Models;

public partial class Machine
{
    public Guid Id { get; set; }

    public long? SerialNumber { get; set; }

    public string Name { get; set; } = null!;

    public Guid UserId { get; set; }

    public Guid Manager { get; set; }

    public Guid Engineer { get; set; }

    public Guid Technician { get; set; }

    public string RfidCashCollection { get; set; } = null!;

    public string RfidLoading { get; set; } = null!;

    public string RfidService { get; set; } = null!;

    public int Model { get; set; }

    public int Company { get; set; }

    public int WorkMode { get; set; }

    public int Status { get; set; }

    public int ServicePriority { get; set; }

    public int Place { get; set; }

    public int Operator { get; set; }

    public int Timezone { get; set; }

    public int? CriticalThresholdTemplate { get; set; }

    public int? NotificationTemplate { get; set; }

    public string? Location { get; set; }

    public string? Coordinates { get; set; }

    public string? Notes { get; set; }

    public string WorkingHours { get; set; } = null!;

    public string KitOnlineId { get; set; } = null!;

    public decimal TotalIncome { get; set; }

    public DateOnly InstallDate { get; set; }

    public DateOnly LastMaintenanceDate { get; set; }

    public virtual Company CompanyNavigation { get; set; } = null!;

    public virtual Mode? CriticalThresholdTemplateNavigation { get; set; }

    public virtual User EngineerNavigation { get; set; } = null!;

    public virtual ICollection<MachinePaymentType> MachinePaymentTypes { get; set; } = new List<MachinePaymentType>();

    public virtual ICollection<MachineProduct> MachineProducts { get; set; } = new List<MachineProduct>();

    public virtual ICollection<Maintenance> Maintenances { get; set; } = new List<Maintenance>();

    public virtual User ManagerNavigation { get; set; } = null!;

    public virtual Model ModelNavigation { get; set; } = null!;

    public virtual Mode? NotificationTemplateNavigation { get; set; }

    public virtual Operator OperatorNavigation { get; set; } = null!;

    public virtual Place PlaceNavigation { get; set; } = null!;

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public virtual Priority ServicePriorityNavigation { get; set; } = null!;

    public virtual Status StatusNavigation { get; set; } = null!;

    public virtual User TechnicianNavigation { get; set; } = null!;

    public virtual Timezone TimezoneNavigation { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual Mode WorkModeNavigation { get; set; } = null!;

    public string FullAddress => $"{Location}, {PlaceNavigation.Name}";
}
