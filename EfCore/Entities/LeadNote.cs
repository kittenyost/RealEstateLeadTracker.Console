using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateLeadTracker.Console.EfCore.Entities
{
    [Table("LeadNotes")]
    public partial class LeadNote
    {
        [Key]
        public int LeadNoteId { get; set; }

        public int LeadId { get; set; }

        [StringLength(200)]
        public string Note { get; set; } = null!;

        [Column(TypeName = "datetime")]
        public DateTime CreatedOn { get; set; }

        public virtual Lead Lead { get; set; } = null!;
    }
}