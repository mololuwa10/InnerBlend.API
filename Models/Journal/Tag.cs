using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InnerBlend.API.Models.Journal
{
    public class Tag
    {
        [Key]
        public int TagId { get; set; }
        public string? Name { get; set; }
        
        public ICollection<JournalEntryTag>? JournalEntries { get; set; }
    }
}