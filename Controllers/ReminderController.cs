using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using InnerBlend.API.Data;
using InnerBlend.API.Models.DTO;
using InnerBlend.API.Models.Journal;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize]
        public async Task<ActionResult<Reminder>> CreateReminder([FromBody] CreateReminderDTO reminderDTO) 
        {
            var reminder = new Reminder 
            {
                ReminderId = Guid.NewGuid(),
                UserId = GetUserId(),
                ReminderMessage = reminderDTO.ReminderMessage,
                ReminderTime = reminderDTO.ReminderTime,
                IsActive = reminderDTO.IsActive,
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };
        
            await context.Reminders.AddAsync(reminder);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetReminders), new { id = reminder.ReminderId }, reminder);
        }
        // GET: /api/reminder
        [HttpGet]
        [Authorize]
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
        [Authorize]
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
        
        [HttpPut("{reminderId}")]
        [Authorize]
        public async Task<IActionResult> UpdateReminder(Guid reminderId, [FromBody] UpdateReminderDTO updateReminderDTO) 
        {
            var userId = GetUserId();
            var reminder = await context.Reminders.FindAsync(reminderId);
            
            if (reminder == null || reminder.UserId != userId) 
            {
                return NotFound(new { message = "Reminder not found or access denied." });
            }
            
            reminder.ReminderMessage = updateReminderDTO.ReminderMessage;
            reminder.ReminderTime = updateReminderDTO.ReminderTime;
            reminder.IsActive = updateReminderDTO.IsActive;
            reminder.DateModified = DateTime.UtcNow;
            
            await context.SaveChangesAsync();

            var response = new ReminderResponseDTO
            {
                ReminderId = reminder.ReminderId,
                ReminderMessage = reminder.ReminderMessage,
                ReminderTime = reminder.ReminderTime,
                IsActive = reminder.IsActive,
                DateCreated = reminder.DateCreated,
                DateModified = reminder.DateModified
            };
            
            return Ok(response);
        }
    }
}