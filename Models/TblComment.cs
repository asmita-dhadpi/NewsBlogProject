using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewsBlogProject.Models;

[Table("tblComments")]
public partial class TblComment
{
    [Key]
    public int CommentId { get; set; }

    public int NewsBlogId { get; set; }

    public int UserId { get; set; }

    [StringLength(1000)]
    public string CommentText { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime CreatedOn { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey("NewsBlogId")]
    [InverseProperty("TblComments")]
    public virtual TblNewsBlog NewsBlog { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("TblComments")]
    public virtual TblUser User { get; set; } = null!;
}
