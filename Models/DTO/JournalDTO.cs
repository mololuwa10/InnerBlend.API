using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InnerBlend.API.Models.Journal;

namespace InnerBlend.API.Models.DTO
{
    public class JournalDTO
    {
        public int JournalId { get; set; }
        public string? JournalTitle { get; set; }
        public string? JournalDescription { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public string? UserId { get; set; }
        public ICollection<JournalEntry>? JournalEntries { get; set; }
    }
}
