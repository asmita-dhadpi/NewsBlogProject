using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewsBlogProject.Models;

[Table("tblNewsCategory")]
public partial class TblNewsCategory
{
    [Key]
    public int CategoryId { get; set; }

    [StringLength(150)]
    public string CategoryName { get; set; } = null!;

    public int CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedOn { get; set; }

    public int? ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedOn { get; set; }

    public bool IsDeleted { get; set; }

    [InverseProperty("Category")]
    public virtual ICollection<TblNewsBlog> TblNewsBlogs { get; set; } = new List<TblNewsBlog>();
}
