using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InnerBlend.API.Models.Journal
{
    public class Journals
    {
        [Key]
        public int JournalId { get; set; }
        public string? UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public string? JournalTitle { get; set; }
        public string? JournalDescription { get; set; }
        public ICollection<JournalEntry>? JournalEntries { get; set; }
        public DateTime? DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime? DateModified { get; set; }
    }
}
