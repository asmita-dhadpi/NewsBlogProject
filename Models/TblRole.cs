using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewsBlogProject.Models;

[Table("tblRoles")]
[Index("RoleName", Name = "UQ__tblRoles__8A2B61601CB71349", IsUnique = true)]
public partial class TblRole
{
    [Key]
    public int RoleId { get; set; }

    [StringLength(50)]
    public string RoleName { get; set; } = null!;

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedOn { get; set; }

    [InverseProperty("Role")]
    public virtual ICollection<TblUser> TblUsers { get; set; } = new List<TblUser>();
}
