using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InnerBlend.API.Models.Journal
{
    public class JournalEntry
    {
        [Key]
        public int? JournalEntryId { get; set; }
        public int? JournalId { get; set; }
        public Journals? Journal { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public ICollection<string>? Tags { get; set; }
        public DateTime? DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime? DateModified { get; set; }
    }
}