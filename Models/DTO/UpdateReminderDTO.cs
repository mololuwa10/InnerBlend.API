using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InnerBlend.API.Models.DTO
{
    public class UpdateReminderDTO
    {
        [Required]
        public string? ReminderMessage { get; set; }

        [Required]
        public TimeOnly ReminderTime { get; set; }
        public bool IsActive { get; set; }
    }
}
