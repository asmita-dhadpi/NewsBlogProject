using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewsBlogProject.Models;

[Table("tblNewsBlogStatus")]
[Index("StatusName", Name = "UQ__tblNewsB__05E7698A782F8B44", IsUnique = true)]
public partial class TblNewsBlogStatus
{
    [Key]
    public int NewsBlogStatusId { get; set; }

    [StringLength(50)]
    public string StatusName { get; set; } = null!;

    public bool IsActive { get; set; }

    [InverseProperty("NewsBlogStatus")]
    public virtual ICollection<TblNewsBlog> TblNewsBlogs { get; set; } = new List<TblNewsBlog>();
}
