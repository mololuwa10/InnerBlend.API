using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace InnerBlend.API.Models.Journal
{
    public class Tag
    {
        [Key]
        public int TagId { get; set; }
        public string? Name { get; set; }

        [ForeignKey("User")]
        public string? UserId { get; set; }
        public User? User { get; set; }

        public ICollection<JournalEntryTag>? JournalEntries { get; set; }
    }
}
