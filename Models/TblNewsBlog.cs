using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewsBlogProject.Models;

[Table("tblNewsBlog")]
public partial class TblNewsBlog
{
    [Key]
    public int NewsBlogId { get; set; }

    public int CategoryId { get; set; }

    [StringLength(300)]
    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public int CreatedByUserId { get; set; }

    public int? ApprovedByUserId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedOn { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ApprovedOn { get; set; }

    public int NewsBlogStatusId { get; set; }

    public int? ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedOn { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("ApprovedByUserId")]
    [InverseProperty("TblNewsBlogApprovedByUsers")]
    public virtual TblUser? ApprovedByUser { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("TblNewsBlogs")]
    public virtual TblNewsCategory Category { get; set; } = null!;

    [ForeignKey("CreatedByUserId")]
    [InverseProperty("TblNewsBlogCreatedByUsers")]
    public virtual TblUser CreatedByUser { get; set; } = null!;

    [ForeignKey("NewsBlogStatusId")]
    [InverseProperty("TblNewsBlogs")]
    public virtual TblNewsBlogStatus NewsBlogStatus { get; set; } = null!;

    [InverseProperty("NewsBlog")]
    public virtual ICollection<TblComment> TblComments { get; set; } = new List<TblComment>();
}
