using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace XBCADAttendance.Models;

public partial class DbWilContext : DbContext
{
    public DbWilContext()
    {
    }

    public DbWilContext(DbContextOptions<DbWilContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TblLecture> TblLectures { get; set; }

    public virtual DbSet<TblModule> TblModules { get; set; }

    public virtual DbSet<TblRole> TblRoles { get; set; }

    public virtual DbSet<TblStaff> TblStaffs { get; set; }

    public virtual DbSet<TblStudent> TblStudents { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.

        => optionsBuilder.UseSqlServer("Server=localhost;Database=dbWIL;Trusted_Connection=True;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblLecture>(entity =>
        {
            entity.HasKey(e => e.LectureId).HasName("PK__tblLectu__B739F69FA304CBA1");

            entity.ToTable("tblLecture");

            entity.Property(e => e.LectureId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("LectureID");
            entity.Property(e => e.ClassroomCode)
                .HasMaxLength(5)
                .IsFixedLength();
            entity.Property(e => e.ModuleCode)
                .HasMaxLength(8)
                .IsFixedLength();
            entity.Property(e => e.UserId)
                .HasMaxLength(8)
                .IsFixedLength()
                .HasColumnName("UserID");

            entity.HasOne(d => d.ModuleCodeNavigation).WithMany(p => p.TblLectures)
                .HasForeignKey(d => d.ModuleCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tblLectur__Modul__7A672E12");

            entity.HasOne(d => d.User).WithMany(p => p.TblLectures)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tblLectur__UserI__797309D9");
        });

        modelBuilder.Entity<TblModule>(entity =>
        {
            entity.HasKey(e => e.ModuleCode).HasName("PK__tblModul__EB27D43206A5E44A");

            entity.ToTable("tblModule");

            entity.Property(e => e.ModuleCode)
                .HasMaxLength(8)
                .IsFixedLength();
            entity.Property(e => e.ModuleName)
                .HasMaxLength(50)
                .IsFixedLength();
        });

        modelBuilder.Entity<TblRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__tblRole__8AFACE3A27DAE5EE");

            entity.ToTable("tblRole");

            entity.Property(e => e.RoleId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("RoleID");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .IsFixedLength();
        });

        modelBuilder.Entity<TblStaff>(entity =>
        {
            entity.HasKey(e => e.StaffId).HasName("PK__tblStaff__96D4AAF7F85862C2");

            entity.ToTable("tblStaff");

            entity.Property(e => e.StaffId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("StaffID");
            entity.Property(e => e.RoleId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("RoleID");
            entity.Property(e => e.UserId)
                .HasMaxLength(8)
                .IsFixedLength()
                .HasColumnName("UserID");

            entity.HasOne(d => d.Role).WithMany(p => p.TblStaffs)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__tblStaff__RoleID__76969D2E");

            entity.HasOne(d => d.User).WithMany(p => p.TblStaffs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tblStaff__UserID__75A278F5");
        });

        modelBuilder.Entity<TblStudent>(entity =>
        {
            entity.HasKey(e => e.StudentNo).HasName("PK__tblStude__32C4C02A7885B30D");

            entity.ToTable("tblStudent");

            entity.Property(e => e.StudentNo)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.UserId)
                .HasMaxLength(8)
                .IsFixedLength()
                .HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.TblStudents)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tblStuden__UserI__7D439ABD");
        });

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__tblUser__1788CCAC6390EC4C");

            entity.ToTable("tblUser");

            entity.Property(e => e.UserId)
                .HasMaxLength(8)
                .IsFixedLength()
                .HasColumnName("UserID");
            entity.Property(e => e.Password).HasMaxLength(200);
            entity.Property(e => e.UserName)
                .HasMaxLength(20)
                .IsFixedLength();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
