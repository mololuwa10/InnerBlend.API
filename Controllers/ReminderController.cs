using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using InnerBlend.API.Data;
using InnerBlend.API.Models.DTO;
using InnerBlend.API.Models.Journal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InnerBlend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReminderController(ApplicationDbContext context) : ControllerBase
    {
        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        
        // POST /api/reminder
        [HttpPost]
        public async Task<ActionResult<Reminder>> CreateReminder([FromBody] CreateReminderDTO reminderDTO) 
        {
            var reminder = new Reminder 
            {
                ReminderId = Guid.NewGuid(),
                UserId = GetUserId(),
                ReminderMessage = reminderDTO.ReminderMessage,
                ReminderTime = reminderDTO.ReminderTime,
                IsActive = true,
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };
        
            await context.Reminders.AddAsync(reminder);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetReminders), new { id = reminder.ReminderId }, reminder);
        }
        // GET: /api/reminder
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reminder>>> GetReminders() 
        {
            var userId = GetUserId();
            var reminders = await context.Reminders
                .Where(r => r.UserId == userId)
                .ToListAsync();
                
            return Ok(reminders);
        }
        
        // DELETE: /api/reminder/{reminderId}
        [HttpDelete("{reminderId}")]
        public async Task<IActionResult> DeleteReminder(Guid reminderId) 
        {
            var userId = GetUserId();
            var reminder = await context.Reminders.FirstOrDefaultAsync(r => r.ReminderId == reminderId && r.UserId == userId);
            
            if (reminder == null) 
            {
                return NotFound();
            }
            
            context.Reminders.Remove(reminder);
            await context.SaveChangesAsync();
            
            return NoContent();
        }
    }
}