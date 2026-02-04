using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewsBlogProject.Models;

[Table("tblAuditLog")]
public partial class TblAuditLog
{
    [Key]
    public int AuditLogId { get; set; }

    public int UserId { get; set; }

    [StringLength(200)]
    public string Action { get; set; } = null!;

    [StringLength(100)]
    public string TableName { get; set; } = null!;

    public int RecordId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedOn { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("TblAuditLogs")]
    public virtual TblUser User { get; set; } = null!;
}
