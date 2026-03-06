using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace UP_4.Models;

public partial class SavukovContext : DbContext
{
    public SavukovContext()
    {
    }

    public SavukovContext(DbContextOptions<SavukovContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Company> Companys { get; set; }

    public virtual DbSet<Machine> Machines { get; set; }

    public virtual DbSet<MachinePaymentType> MachinePaymentTypes { get; set; }

    public virtual DbSet<MachineProduct> MachineProducts { get; set; }

    public virtual DbSet<Maintenance> Maintenances { get; set; }

    public virtual DbSet<Mode> Modes { get; set; }

    public virtual DbSet<Model> Models { get; set; }

    public virtual DbSet<Operator> Operators { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<PaymentType> PaymentTypes { get; set; }

    public virtual DbSet<Place> Places { get; set; }

    public virtual DbSet<Priority> Prioritys { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Sale> Sales { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<Timezone> Timezones { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=ngknn.ru;Port=5442;Database=Savukov;Username=21P;Password=123");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("C");

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("companys_pkey");

            entity.ToTable("companys", "UP_41");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<Machine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("machines_pkey");

            entity.ToTable("machines", "UP_41");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Company).HasColumnName("company");
            entity.Property(e => e.Coordinates)
                .HasColumnType("character varying")
                .HasColumnName("coordinates");
            entity.Property(e => e.CriticalThresholdTemplate).HasColumnName("critical_threshold_template");
            entity.Property(e => e.Engineer).HasColumnName("engineer");
            entity.Property(e => e.InstallDate).HasColumnName("install_date");
            entity.Property(e => e.KitOnlineId)
                .HasColumnType("character varying")
                .HasColumnName("kit_online_id");
            entity.Property(e => e.LastMaintenanceDate).HasColumnName("last_maintenance_date");
            entity.Property(e => e.Location)
                .HasColumnType("character varying")
                .HasColumnName("location");
            entity.Property(e => e.Manager).HasColumnName("manager");
            entity.Property(e => e.Model).HasColumnName("model");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.Notes)
                .HasColumnType("character varying")
                .HasColumnName("notes");
            entity.Property(e => e.NotificationTemplate).HasColumnName("notification_template");
            entity.Property(e => e.Operator).HasColumnName("operator");
            entity.Property(e => e.Place).HasColumnName("place");
            entity.Property(e => e.RfidCashCollection)
                .HasColumnType("character varying")
                .HasColumnName("rfid_cash_collection");
            entity.Property(e => e.RfidLoading)
                .HasColumnType("character varying")
                .HasColumnName("rfid_loading");
            entity.Property(e => e.RfidService)
                .HasColumnType("character varying")
                .HasColumnName("rfid_service");
            entity.Property(e => e.SerialNumber).HasColumnName("serial_number");
            entity.Property(e => e.ServicePriority).HasColumnName("service_priority");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Technician).HasColumnName("technician");
            entity.Property(e => e.Timezone).HasColumnName("timezone");
            entity.Property(e => e.TotalIncome)
                .HasPrecision(14, 2)
                .HasColumnName("total_income");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WorkMode).HasColumnName("work_mode");
            entity.Property(e => e.WorkingHours)
                .HasColumnType("character varying")
                .HasColumnName("working_hours");

            entity.HasOne(d => d.CompanyNavigation).WithMany(p => p.Machines)
                .HasForeignKey(d => d.Company)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("machines_company_fkey");

            entity.HasOne(d => d.CriticalThresholdTemplateNavigation).WithMany(p => p.MachineCriticalThresholdTemplateNavigations)
                .HasForeignKey(d => d.CriticalThresholdTemplate)
                .HasConstraintName("machines_critical_threshold_template_fkey");

            entity.HasOne(d => d.EngineerNavigation).WithMany(p => p.MachineEngineerNavigations)
                .HasForeignKey(d => d.Engineer)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("machines_engineer_fkey");

            entity.HasOne(d => d.ManagerNavigation).WithMany(p => p.MachineManagerNavigations)
                .HasForeignKey(d => d.Manager)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("machines_manager_fkey");

            entity.HasOne(d => d.ModelNavigation).WithMany(p => p.Machines)
                .HasForeignKey(d => d.Model)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("machines_model_fkey");

            entity.HasOne(d => d.NotificationTemplateNavigation).WithMany(p => p.MachineNotificationTemplateNavigations)
                .HasForeignKey(d => d.NotificationTemplate)
                .HasConstraintName("machines_notification_template_fkey");

            entity.HasOne(d => d.OperatorNavigation).WithMany(p => p.Machines)
                .HasForeignKey(d => d.Operator)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("machines_operator_fkey");

            entity.HasOne(d => d.PlaceNavigation).WithMany(p => p.Machines)
                .HasForeignKey(d => d.Place)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("machines_place_fkey");

            entity.HasOne(d => d.ServicePriorityNavigation).WithMany(p => p.Machines)
                .HasForeignKey(d => d.ServicePriority)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("machines_service_priority_fkey");

            entity.HasOne(d => d.StatusNavigation).WithMany(p => p.Machines)
                .HasForeignKey(d => d.Status)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("machines_status_fkey");

            entity.HasOne(d => d.TechnicianNavigation).WithMany(p => p.MachineTechnicianNavigations)
                .HasForeignKey(d => d.Technician)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("machines_technician_fkey");

            entity.HasOne(d => d.TimezoneNavigation).WithMany(p => p.Machines)
                .HasForeignKey(d => d.Timezone)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("machines_timezone_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.MachineUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("machines_user_id_fkey");

            entity.HasOne(d => d.WorkModeNavigation).WithMany(p => p.MachineWorkModeNavigations)
                .HasForeignKey(d => d.WorkMode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("machines_work_mode_fkey");
        });

        modelBuilder.Entity<MachinePaymentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("machine_payment_type_pkey");

            entity.ToTable("machine_payment_type", "UP_41");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdMachine).HasColumnName("id_machine");
            entity.Property(e => e.IdPaymentType).HasColumnName("id_payment_type");

            entity.HasOne(d => d.IdMachineNavigation).WithMany(p => p.MachinePaymentTypes)
                .HasForeignKey(d => d.IdMachine)
                .HasConstraintName("machine_payment_type_id_machine_fkey");

            entity.HasOne(d => d.IdPaymentTypeNavigation).WithMany(p => p.MachinePaymentTypes)
                .HasForeignKey(d => d.IdPaymentType)
                .HasConstraintName("machine_payment_type_id_payment_type_fkey");
        });

        modelBuilder.Entity<MachineProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("machine_products_pkey");

            entity.ToTable("machine_products", "UP_41");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MachineId).HasColumnName("machine_id");
            entity.Property(e => e.MinStock).HasColumnName("min_stock");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.QuantityAvailable).HasColumnName("quantity_available");

            entity.HasOne(d => d.Machine).WithMany(p => p.MachineProducts)
                .HasForeignKey(d => d.MachineId)
                .HasConstraintName("machine_products_machine_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.MachineProducts)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("machine_products_product_id_fkey");
        });

        modelBuilder.Entity<Maintenance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("maintenance_pkey");

            entity.ToTable("maintenance", "UP_41");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.IssuesFound)
                .HasColumnType("character varying")
                .HasColumnName("issues_found");
            entity.Property(e => e.ServiceDate).HasColumnName("service_date");
            entity.Property(e => e.VendingMachineId).HasColumnName("vending_machine_id");
            entity.Property(e => e.WorkDescription)
                .HasColumnType("character varying")
                .HasColumnName("work_description");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Maintenances)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("maintenance_id_user_fkey");

            entity.HasOne(d => d.VendingMachine).WithMany(p => p.Maintenances)
                .HasForeignKey(d => d.VendingMachineId)
                .HasConstraintName("maintenance_vending_machine_id_fkey");
        });

        modelBuilder.Entity<Mode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("modes_pkey");

            entity.ToTable("modes", "UP_41");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<Model>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("models_pkey");

            entity.ToTable("models", "UP_41");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<Operator>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("operators_pkey");

            entity.ToTable("operators", "UP_41");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payment_method_pkey");

            entity.ToTable("payment_method", "UP_41");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<PaymentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payment_types_pkey");

            entity.ToTable("payment_types", "UP_41");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<Place>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("places_pkey");

            entity.ToTable("places", "UP_41");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<Priority>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prioritys_pkey");

            entity.ToTable("prioritys", "UP_41");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("products_pkey");

            entity.ToTable("products", "UP_41");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Description)
                .HasColumnType("character varying")
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.SalesTrend)
                .HasPrecision(5, 2)
                .HasColumnName("sales_trend");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles", "UP_41");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sales_pkey");

            entity.ToTable("sales", "UP_41");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MachineId).HasColumnName("machine_id");
            entity.Property(e => e.PaymentMethod).HasColumnName("payment_method");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.SaleTimestamp)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("sale_timestamp");
            entity.Property(e => e.TotalPrice)
                .HasPrecision(10, 2)
                .HasColumnName("total_price");

            entity.HasOne(d => d.Machine).WithMany(p => p.Sales)
                .HasForeignKey(d => d.MachineId)
                .HasConstraintName("sales_machine_id_fkey");

            entity.HasOne(d => d.PaymentMethodNavigation).WithMany(p => p.Sales)
                .HasForeignKey(d => d.PaymentMethod)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sales_payment_method_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.Sales)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("sales_product_id_fkey");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("statuses_pkey");

            entity.ToTable("statuses", "UP_41");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<Timezone>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("timezones_pkey");

            entity.ToTable("timezones", "UP_41");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users", "UP_41");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.HasIndex(e => e.Phone, "users_phone_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Email)
                .HasColumnType("character varying")
                .HasColumnName("email");
            entity.Property(e => e.IdRole).HasColumnName("id_role");
            entity.Property(e => e.Image)
                .HasColumnType("character varying")
                .HasColumnName("image");
            entity.Property(e => e.IsEngineer).HasColumnName("is_engineer");
            entity.Property(e => e.IsManager).HasColumnName("is_manager");
            entity.Property(e => e.IsOperator).HasColumnName("is_operator");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasColumnType("character varying")
                .HasColumnName("password");
            entity.Property(e => e.Patronymic)
                .HasColumnType("character varying")
                .HasColumnName("patronymic");
            entity.Property(e => e.Phone)
                .HasColumnType("character varying")
                .HasColumnName("phone");
            entity.Property(e => e.Surname)
                .HasColumnType("character varying")
                .HasColumnName("surname");

            entity.HasOne(d => d.IdRoleNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.IdRole)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("users_id_role_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
