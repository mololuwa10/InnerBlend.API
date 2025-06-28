using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InnerBlend.API.Models.DTO
{
    public class JournalEntryCreateRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Mood { get; set; }
        public string? Location { get; set; }
        public List<string>? Tags { get; set; }
        // public List<IFormFile>? Files { get; set; }
    }
}