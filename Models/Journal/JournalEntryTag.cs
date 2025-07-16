using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InnerBlend.API.Models.Journal
{
    public class JournalEntryTag
    {
        public int JournalEntryId { get; set; }
        public JournalEntry? JournalEntry { get; set; }

        public int TagId { get; set; }
        public Tag? Tag { get; set; }
    }
}
