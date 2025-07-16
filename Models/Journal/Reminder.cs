using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace InnerBlend.API.Models.Journal
{
    public class Reminder
    {
        [Key]
        public Guid ReminderId { get; set; } = Guid.NewGuid();

        [Required]
        public string? UserId { get; set; }

        [Required]
        public string? ReminderMessage { get; set; }

        [Required]
        public TimeOnly ReminderTime { get; set; } // e.g 9:00 AM

        public bool IsActive { get; set; }
        public DateTime? DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime? DateModified { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
