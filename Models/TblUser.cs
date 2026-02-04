using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewsBlogProject.Models;

[Table("tblUsers")]
public partial class TblUser
{
    [Key]
    public int UserId { get; set; }

    public int RoleId { get; set; }

    [StringLength(100)]
    public string FirstName { get; set; } = null!;

    [StringLength(100)]
    public string? LastName { get; set; }

    [StringLength(150)]
    public string Email { get; set; } = null!;

    [StringLength(500)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(200)]
    public string PasswordSalt { get; set; } = null!;

    [Column("DOB")]
    public DateOnly? Dob { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; }

    public int? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedOn { get; set; }

    public int? ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedOn { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("RoleId")]
    [InverseProperty("TblUsers")]
    public virtual TblRole Role { get; set; } = null!;

    [InverseProperty("User")]
    public virtual ICollection<TblAuditLog> TblAuditLogs { get; set; } = new List<TblAuditLog>();

    [InverseProperty("User")]
    public virtual ICollection<TblComment> TblComments { get; set; } = new List<TblComment>();

    [InverseProperty("ApprovedByUser")]
    public virtual ICollection<TblNewsBlog> TblNewsBlogApprovedByUsers { get; set; } = new List<TblNewsBlog>();

    [InverseProperty("CreatedByUser")]
    public virtual ICollection<TblNewsBlog> TblNewsBlogCreatedByUsers { get; set; } = new List<TblNewsBlog>();
}
