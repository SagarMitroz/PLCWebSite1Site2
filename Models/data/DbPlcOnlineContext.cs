using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Water_Filtration.Models.data;

public partial class DbPlcOnlineContext : DbContext
{
    public DbPlcOnlineContext()
    {
    }

    public DbPlcOnlineContext(DbContextOptions<DbPlcOnlineContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Connectionstatus> Connectionstatuses { get; set; }

    public virtual DbSet<Filtrationcyclesummary> Filtrationcyclesummaries { get; set; }

    public virtual DbSet<Filtrationsummary> Filtrationsummaries { get; set; }

    public virtual DbSet<Flowmaster> Flowmasters { get; set; }

    public virtual DbSet<Plcalarm> Plcalarms { get; set; }

    public virtual DbSet<Plcaudit> Plcaudits { get; set; }

    public virtual DbSet<Plcauditlog> Plcauditlogs { get; set; }

    public virtual DbSet<Plctag> Plctags { get; set; }

    public virtual DbSet<Plcvalue> Plcvalues { get; set; }

    public virtual DbSet<PlcvalueBackup> PlcvalueBackups { get; set; }

    public virtual DbSet<Pressuremaster> Pressuremasters { get; set; }

    public virtual DbSet<Site> Sites { get; set; }

    public virtual DbSet<Siteflowaccess> Siteflowaccesses { get; set; }

    public virtual DbSet<Sitepressureaccess> Sitepressureaccesses { get; set; }

    public virtual DbSet<Sitetankaccess> Sitetankaccesses { get; set; }

    public virtual DbSet<Sitetempaccess> Sitetempaccesses { get; set; }

    public virtual DbSet<Tankmaster> Tankmasters { get; set; }

    public virtual DbSet<Temperaturemaster> Temperaturemasters { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Userrole> Userroles { get; set; }

    public virtual DbSet<Waterusagesummary> Waterusagesummaries { get; set; }

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Connectionstatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("connectionstatus");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.LastRecordTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<Filtrationcyclesummary>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("filtrationcyclesummary");

            entity.Property(e => e.Backwash).HasColumnName("backwash");
            entity.Property(e => e.Cip).HasColumnName("CIP");
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.FItrationrunning).HasColumnName("fItrationrunning");
            entity.Property(e => e.FiltrationRuntime).HasColumnName("filtrationRuntime");
            entity.Property(e => e.Standby).HasColumnName("standby");
            entity.Property(e => e.SummaryDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<Filtrationsummary>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("filtrationsummary");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.EventEndTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.EventStartTime).HasColumnType("datetime");
            entity.Property(e => e.FiltrationEvent).HasMaxLength(100);
            entity.Property(e => e.IsDelete)
                .HasDefaultValueSql("'0'")
                .HasColumnName("isDelete");
        });

        modelBuilder.Entity<Flowmaster>(entity =>
        {
            entity.HasKey(e => e.Fid).HasName("PRIMARY");

            entity.ToTable("flowmaster");

            entity.Property(e => e.Fid).HasColumnName("FId");
            entity.Property(e => e.CreatedOn).HasColumnType("timestamp");
            entity.Property(e => e.FlowName).HasMaxLength(45);
            entity.Property(e => e.IsDelete).HasMaxLength(45);
        });

        modelBuilder.Entity<Plcalarm>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("plcalarm");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.IsDelete)
                .HasDefaultValueSql("b'0'")
                .HasColumnType("bit(1)");
        });

        modelBuilder.Entity<Plcaudit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("plcaudit");

            entity.Property(e => e.ChangedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.IsDelete)
                .HasDefaultValueSql("b'0'")
                .HasColumnType("bit(1)");
        });

        modelBuilder.Entity<Plcauditlog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("plcauditlog");

            entity.Property(e => e.ChangedBy).HasMaxLength(100);
            entity.Property(e => e.ChangedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.IsDelete)
                .HasDefaultValueSql("b'0'")
                .HasColumnType("bit(1)");
            entity.Property(e => e.NewValue).HasMaxLength(255);
            entity.Property(e => e.OldValue).HasMaxLength(255);
        });

        modelBuilder.Entity<Plctag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("plctag");

            entity.Property(e => e.DataType).HasMaxLength(20);
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("b'1'")
                .HasColumnType("bit(1)");
            entity.Property(e => e.IsImportant).HasDefaultValueSql("'0'");
            entity.Property(e => e.Offset).HasMaxLength(50);
            entity.Property(e => e.TagName).HasMaxLength(50);
        });

        modelBuilder.Entity<Plcvalue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("plcvalue");

            entity.Property(e => e.ActTimeBackflush).HasColumnName("ACT_TIME_BACKFLUSH");
            entity.Property(e => e.ActTimeFiltration).HasColumnName("ACT_TIME_FILTRATION");
            entity.Property(e => e.BackflushCount).HasColumnName("BACKFLUSH_COUNT");
            entity.Property(e => e.Cls601).HasColumnName("CLS_601");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.FiltrationRunHrs).HasColumnName("FILTRATION_RUN_HRS");
            entity.Property(e => e.Fs101).HasColumnName("FS_101");
            entity.Property(e => e.Fs102).HasColumnName("FS_102");
            entity.Property(e => e.Fs301).HasColumnName("FS_301");
            entity.Property(e => e.Fs302).HasColumnName("FS_302");
            entity.Property(e => e.Fs303).HasColumnName("FS_303");
            entity.Property(e => e.Fs601).HasColumnName("FS_601");
            entity.Property(e => e.Phs601).HasColumnName("PHS_601");
            entity.Property(e => e.Ps301).HasColumnName("PS_301");
            entity.Property(e => e.Ps302).HasColumnName("PS_302");
            entity.Property(e => e.Ps303).HasColumnName("PS_303");
            entity.Property(e => e.Qcs601).HasColumnName("QCS_601");
            entity.Property(e => e.Qs301).HasColumnName("QS_301");
            entity.Property(e => e.SelectedCycle).HasColumnName("SELECTED_CYCLE");
            entity.Property(e => e.SetTimeBackflush).HasColumnName("SET_TIME_BACKFLUSH");
            entity.Property(e => e.SetTimeFiltration).HasColumnName("SET_TIME_FILTRATION");
            entity.Property(e => e.Spare3).HasColumnName("SPARE_3");
            entity.Property(e => e.Spare4).HasColumnName("SPARE_4");
            entity.Property(e => e.Spare5).HasColumnName("SPARE_5");
            entity.Property(e => e.StepNo).HasColumnName("STEP_NO");
            entity.Property(e => e.Tl01).HasColumnName("TL_01");
            entity.Property(e => e.Tl03).HasColumnName("TL_03");
            entity.Property(e => e.Tl04).HasColumnName("TL_04");
            entity.Property(e => e.Tl05).HasColumnName("TL_05");
            entity.Property(e => e.Tl06).HasColumnName("TL_06");
            entity.Property(e => e.Ts301).HasColumnName("TS_301");
            entity.Property(e => e.Ts302).HasColumnName("TS_302");
        });

        modelBuilder.Entity<PlcvalueBackup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("plcvalue_backup");

            entity.Property(e => e.ActTimeBackflush).HasColumnName("ACT_TIME_BACKFLUSH");
            entity.Property(e => e.ActTimeFiltration).HasColumnName("ACT_TIME_FILTRATION");
            entity.Property(e => e.BackflushCount).HasColumnName("BACKFLUSH_COUNT");
            entity.Property(e => e.Cls601).HasColumnName("CLS_601");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.FiltrationRunHrs).HasColumnName("FILTRATION_RUN_HRS");
            entity.Property(e => e.Fs101).HasColumnName("FS_101");
            entity.Property(e => e.Fs102).HasColumnName("FS_102");
            entity.Property(e => e.Fs301).HasColumnName("FS_301");
            entity.Property(e => e.Fs302).HasColumnName("FS_302");
            entity.Property(e => e.Fs303).HasColumnName("FS_303");
            entity.Property(e => e.Fs601).HasColumnName("FS_601");
            entity.Property(e => e.Phs601).HasColumnName("PHS_601");
            entity.Property(e => e.Ps301).HasColumnName("PS_301");
            entity.Property(e => e.Ps302).HasColumnName("PS_302");
            entity.Property(e => e.Ps303).HasColumnName("PS_303");
            entity.Property(e => e.Qcs601).HasColumnName("QCS_601");
            entity.Property(e => e.Qs301).HasColumnName("QS_301");
            entity.Property(e => e.SelectedCycle).HasColumnName("SELECTED_CYCLE");
            entity.Property(e => e.SetTimeBackflush).HasColumnName("SET_TIME_BACKFLUSH");
            entity.Property(e => e.SetTimeFiltration).HasColumnName("SET_TIME_FILTRATION");
            entity.Property(e => e.Spare3).HasColumnName("SPARE_3");
            entity.Property(e => e.Spare4).HasColumnName("SPARE_4");
            entity.Property(e => e.Spare5).HasColumnName("SPARE_5");
            entity.Property(e => e.StepNo).HasColumnName("STEP_NO");
            entity.Property(e => e.Tl01).HasColumnName("TL_01");
            entity.Property(e => e.Tl03).HasColumnName("TL_03");
            entity.Property(e => e.Tl04).HasColumnName("TL_04");
            entity.Property(e => e.Tl05).HasColumnName("TL_05");
            entity.Property(e => e.Tl06).HasColumnName("TL_06");
            entity.Property(e => e.Ts301).HasColumnName("TS_301");
            entity.Property(e => e.Ts302).HasColumnName("TS_302");
        });

        modelBuilder.Entity<Pressuremaster>(entity =>
        {
            entity.HasKey(e => e.Pid).HasName("PRIMARY");

            entity.ToTable("pressuremaster");

            entity.Property(e => e.Pid).HasColumnName("pid");
            entity.Property(e => e.CreatedOn).HasColumnType("timestamp");
            entity.Property(e => e.PressureName).HasMaxLength(45);
        });

        modelBuilder.Entity<Site>(entity =>
        {
            entity.HasKey(e => e.SiteId).HasName("PRIMARY");

            entity.ToTable("site");

            entity.Property(e => e.CreatedOn).HasColumnType("timestamp");
            entity.Property(e => e.PlcSignalDate).HasMaxLength(250);
            entity.Property(e => e.SiteName).HasMaxLength(200);
        });

        modelBuilder.Entity<Siteflowaccess>(entity =>
        {
            entity.HasKey(e => e.SiteFlowAccessId).HasName("PRIMARY");

            entity.ToTable("siteflowaccess");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.HasAccess).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<Sitepressureaccess>(entity =>
        {
            entity.HasKey(e => e.SitepressureId).HasName("PRIMARY");

            entity.ToTable("sitepressureaccess");

            entity.Property(e => e.SitepressureId).HasColumnName("sitepressureId");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
        });

        modelBuilder.Entity<Sitetankaccess>(entity =>
        {
            entity.HasKey(e => e.SitetankaccessId).HasName("PRIMARY");

            entity.ToTable("sitetankaccess");

            entity.Property(e => e.SitetankaccessId).HasColumnName("sitetankaccessId");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.IsCwtank).HasColumnName("IsCWTank");
        });

        modelBuilder.Entity<Sitetempaccess>(entity =>
        {
            entity.HasKey(e => e.SitetempId).HasName("PRIMARY");

            entity.ToTable("sitetempaccess");

            entity.Property(e => e.SitetempId).HasColumnName("sitetempId");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
        });

        modelBuilder.Entity<Tankmaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tankmaster");

            entity.Property(e => e.CreatedOn).HasColumnType("timestamp");
            entity.Property(e => e.IsDelete).HasDefaultValueSql("'0'");
            entity.Property(e => e.TankName).HasMaxLength(45);
        });

        modelBuilder.Entity<Temperaturemaster>(entity =>
        {
            entity.HasKey(e => e.Tid).HasName("PRIMARY");

            entity.ToTable("temperaturemaster");

            entity.Property(e => e.Tid).HasColumnName("tid");
            entity.Property(e => e.CreatedOn).HasColumnType("timestamp");
            entity.Property(e => e.Temperature).HasMaxLength(45);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("user");

            entity.Property(e => e.Contact).HasMaxLength(100);
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(45);
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.UserName).HasMaxLength(45);
        });

        modelBuilder.Entity<Userrole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("userrole");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Role).HasMaxLength(45);
        });

        modelBuilder.Entity<Waterusagesummary>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("waterusagesummary");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("createdOn");
            entity.Property(e => e.Cronjobsendtime)
                .HasColumnType("datetime")
                .HasColumnName("cronjobsendtime");
            entity.Property(e => e.Cronjobstarttime)
                .HasColumnType("datetime")
                .HasColumnName("cronjobstarttime");
            entity.Property(e => e.Fs101)
                .HasDefaultValueSql("'0'")
                .HasColumnName("FS_101");
            entity.Property(e => e.Fs102)
                .HasDefaultValueSql("'0'")
                .HasColumnName("FS_102");
            entity.Property(e => e.Fs301)
                .HasDefaultValueSql("'0'")
                .HasColumnName("FS_301");
            entity.Property(e => e.Fs302)
                .HasDefaultValueSql("'0'")
                .HasColumnName("FS_302");
            entity.Property(e => e.Fs303)
                .HasDefaultValueSql("'0'")
                .HasColumnName("FS_303");
            entity.Property(e => e.Fs601)
                .HasDefaultValueSql("'0'")
                .HasColumnName("FS_601");
            entity.Property(e => e.IsDelete).HasColumnName("isDelete");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
