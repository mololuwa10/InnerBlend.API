using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InnerBlend.API.Models.DTO
{
    public class ReminderResponseDTO
    {
        public Guid ReminderId { get; set; }
        public string? ReminderMessage { get; set; }
        public TimeOnly ReminderTime { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
    }
}