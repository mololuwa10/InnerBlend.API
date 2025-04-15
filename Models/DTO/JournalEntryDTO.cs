using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InnerBlend.API.Models.DTO
{
    public class JournalEntryDTO
    {
        public int? JournalEntryId { get; set; }
        public int? JournalId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public ICollection<string>? Tags { get; set; }
        public string? DateCreated { get; set; }
        public string? DateModified { get; set; }
    }
}