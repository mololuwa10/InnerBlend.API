using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InnerBlend.API.Models.Journal
{
    public class JournalEntryImages
    {
        [Key]
        public int EntryImageId { get; set; }
        
        public int JournalEntryId { get; set; }
        public JournalEntry? JournalEntry { get; set; }
        
        [Required]
        public string? ImageUrl { get; set; }
        public DateTime DateUploaded { get; set; } = DateTime.UtcNow;
    }
}