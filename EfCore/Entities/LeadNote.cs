using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RealEstateLeadTracker.Console.EfCore.Entities;

public partial class LeadNote
{
    [Key]
    public int LeadNoteId { get; set; }

    public int LeadId { get; set; }

    [StringLength(200)]
    public string Note { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime CreatedOn { get; set; }
}
