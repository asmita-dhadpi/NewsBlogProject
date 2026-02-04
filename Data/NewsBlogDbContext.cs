using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using NewsBlogProject.Models;

namespace NewsBlogProject.Data;

public partial class NewsBlogDbContext : DbContext
{
    public NewsBlogDbContext()
    {
    }

    public NewsBlogDbContext(DbContextOptions<NewsBlogDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TblAuditLog> TblAuditLogs { get; set; }

    public virtual DbSet<TblComment> TblComments { get; set; }

    public virtual DbSet<TblNewsBlog> TblNewsBlogs { get; set; }

    public virtual DbSet<TblNewsBlogStatus> TblNewsBlogStatuses { get; set; }

    public virtual DbSet<TblNewsCategory> TblNewsCategories { get; set; }

    public virtual DbSet<TblRole> TblRoles { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblAuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId).HasName("PK__tblAudit__EB5F6CBD67502BFF");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.User).WithMany(p => p.TblAuditLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tblAuditLog_User");
        });

        modelBuilder.Entity<TblComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__tblComme__C3B4DFCA761B7C33");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.NewsBlog).WithMany(p => p.TblComments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tblComments_Blog");

            entity.HasOne(d => d.User).WithMany(p => p.TblComments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tblComments_User");
        });

        modelBuilder.Entity<TblNewsBlog>(entity =>
        {
            entity.HasKey(e => e.NewsBlogId).HasName("PK__tblNewsB__BDA902492E6AF968");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ApprovedByUser).WithMany(p => p.TblNewsBlogApprovedByUsers).HasConstraintName("FK_tblNewsBlog_ApprovedBy");

            entity.HasOne(d => d.Category).WithMany(p => p.TblNewsBlogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tblNewsBlog_Category");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.TblNewsBlogCreatedByUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tblNewsBlog_CreatedBy");

            entity.HasOne(d => d.NewsBlogStatus).WithMany(p => p.TblNewsBlogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tblNewsBlog_Status");
        });

        modelBuilder.Entity<TblNewsBlogStatus>(entity =>
        {
            entity.HasKey(e => e.NewsBlogStatusId).HasName("PK__tblNewsB__E860D8DFDB4323D4");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<TblNewsCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__tblNewsC__19093A0B7B611E20");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<TblRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__tblRoles__8AFACE1A5917D2E1");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__tblUsers__1788CC4CEDA5AB92");

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Role).WithMany(p => p.TblUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tblUsers_Roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
