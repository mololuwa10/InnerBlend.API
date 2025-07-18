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
        public Mood? Mood { get; set; }
        public string? Location { get; set; }
        public ICollection<JournalEntryTag>? JournalEntryTags { get; set; }
        public DateTime? DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime? DateModified { get; set; }
        public ICollection<JournalEntryImages>? Images { get; set; }
    }

    public enum Mood
    {
        VerySad,
        Sad,
        Neutral,
        Happy,
        VeryHappy,
    }
}
