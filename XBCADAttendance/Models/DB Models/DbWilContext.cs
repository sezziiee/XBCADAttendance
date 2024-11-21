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

    public virtual DbSet<TblModule> TblModules { get; set; }

    public virtual DbSet<TblRole> TblRoles { get; set; }

    public virtual DbSet<TblStaff> TblStaffs { get; set; }

    public virtual DbSet<TblStaffLecture> TblStaffLectures { get; set; }

    public virtual DbSet<TblStudent> TblStudents { get; set; }

    public virtual DbSet<TblStudentLecture> TblStudentLectures { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    public virtual DbSet<TblUserModules> TblUserModules { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=tcp:wilapidbserver.database.windows.net,1433;Initial Catalog=WIL-DB;Persist Security Info=False;User ID=ST10085210;Password=Treepair521;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
            entity.HasKey(e => e.UserId).HasName("PK__tblStaff__96D4AAF7F85862C2");

            entity.ToTable("tblStaff");

            entity.Property(e => e.UserId)
                .HasMaxLength(8)
                .IsFixedLength()
                .HasColumnName("UserID");
            entity.Property(e => e.RoleId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("RoleID");
            entity.Property(e => e.StaffId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("StaffID");

            entity.HasOne(d => d.Role).WithMany(p => p.TblStaffs)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__tblStaff__RoleID__76969D2E");

            entity.HasOne(d => d.User).WithOne(p => p.TblStaff)
                .HasForeignKey<TblStaff>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tblStaff__UserID__75A278F5");
        });

        modelBuilder.Entity<TblStaffLecture>(entity =>
        {
            entity.HasKey(e => e.LectureId);

            entity.ToTable("TblStaffLecture");

            entity.Property(e => e.LectureId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("LectureID");
            entity.Property(e => e.ClassroomCode).HasMaxLength(5);
            entity.Property(e => e.Finish).HasColumnName("finish");
            entity.Property(e => e.ModuleCode)
                .HasMaxLength(8)
                .IsFixedLength();
            entity.Property(e => e.Start).HasColumnName("start");
            entity.Property(e => e.UserId)
                .HasMaxLength(8)
                .IsFixedLength()
                .HasColumnName("UserID");

            entity.HasOne(d => d.ModuleCodeNavigation).WithMany(p => p.TblStaffLectures)
                .HasForeignKey(d => d.ModuleCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblStaffLecture_tblModule");

            entity.HasOne(d => d.User).WithMany(p => p.TblStaffLectures)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblStaffLecture_tblStaff");
        });

        modelBuilder.Entity<TblStudent>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__tblStude__32C4C02A7885B30D");

            entity.ToTable("tblStudent");

            entity.Property(e => e.UserId)
                .HasMaxLength(8)
                .IsFixedLength()
                .HasColumnName("UserID");
            entity.Property(e => e.StudentNo)
                .HasMaxLength(10)
                .IsFixedLength();

            entity.HasOne(d => d.User).WithOne(p => p.TblStudent)
                .HasForeignKey<TblStudent>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tblStuden__UserI__7D439ABD");
        });

        modelBuilder.Entity<TblStudentLecture>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tblStudentLecture");

            entity.Property(e => e.ClassroomCode).HasMaxLength(5);
            entity.Property(e => e.LectureId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("LectureID");
            entity.Property(e => e.ModuleCode)
                .HasMaxLength(8)
                .IsFixedLength();
            entity.Property(e => e.UserId)
                .HasMaxLength(8)
                .IsFixedLength()
                .HasColumnName("UserID");

            entity.HasOne(d => d.Lecture).WithMany()
                .HasForeignKey(d => d.LectureId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tblStudentLecture_TblStaffLecture1");

            entity.HasOne(d => d.ModuleCodeNavigation).WithMany()
                .HasForeignKey(d => d.ModuleCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tblLectur__Modul__7A672E12");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tblStudentLecture_tblStudent");
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

        modelBuilder.Entity<TblUserModules>(entity =>
        {
            // Composite Primary Key
            entity.HasKey(e => new { e.ModuleCode, e.UserId })
                  .HasName("PK_tblUserModules");

            // Table Mapping
            entity.ToTable("tblUserModules");

            // Properties
            entity.Property(e => e.ModuleCode)
                  .HasMaxLength(50)
                  .IsFixedLength()
                  .HasColumnName("ModuleCode");

            entity.Property(e => e.UserId)
                  .HasMaxLength(50) // Ensure this matches the column definition
                  .HasColumnName("UserID");

            // Relationships
            entity.HasOne(e => e.TblModule) // Navigation property to TblModule
                  .WithMany(m => m.TblUserModules) // Reverse navigation property in TblModule
                  .HasForeignKey(e => e.ModuleCode)
                  .HasConstraintName("FK_tblUserModules_tblModule");

            entity.HasOne(e => e.TblUser) // Navigation property to TblUser
                  .WithMany(u => u.TblUserModules) // Reverse navigation property in TblUser
                  .HasForeignKey(e => e.UserId)
                  .HasConstraintName("FK_tblUserModules_tblUser");
        });


        OnModelCreatingPartial(modelBuilder);
    }



    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
