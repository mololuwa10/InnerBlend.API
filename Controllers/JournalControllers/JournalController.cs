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

namespace InnerBlend.API.Controllers.JournalControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JournalController(ApplicationDbContext dbContext) : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<JournalDTO>>> GetJournals() 
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) 
            {
                return Unauthorized("You are not logged in");
            }

            #pragma warning disable CS8602
            var journalItems = await dbContext?
                .Journals?.Where(t => t.UserId == userId)
                .Select(t => new JournalDTO
                {
                    JournalId = t.JournalId,
                    JournalTitle = t.JournalTitle,
                    JournalDescription = t.JournalDescription,
                    DateCreated = t.DateCreated,
                    DateModified = t.DateModified,
                    UserId = t.UserId,
                    JournalEntries = t.JournalEntries!.Select(e => new JournalEntry
                    {
                        JournalEntryId = e.JournalEntryId,
                        JournalId = e.JournalId,
                        Title = e.Title,
                        Content = e.Content,
                        DateCreated = e.DateCreated,
                        DateModified = e.DateModified,
                        JournalEntryTags = e.JournalEntryTags!.Select(jet => new JournalEntryTag
                        {
                            JournalEntryId = jet.JournalEntryId,
                            TagId = jet.TagId,
                            Tag = new Tag
                            {
                                TagId = jet.Tag.TagId,
                                Name = jet.Tag.Name,
                                UserId = jet.Tag.UserId
                            }
                        }).ToList()
                    }).ToList()
                })
                .ToListAsync();


            return Ok(journalItems);
        }
        
        [HttpGet("{journalId}")]
        [Authorize]
		public async Task<ActionResult<Journals>> GetJournals(int journalId)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if(string.IsNullOrEmpty(userId)) 
			{
			    return Unauthorized("You are not logged in");
			}
			
			var journalItem = await dbContext?
                .Journals?.Where(t => t.UserId == userId && t.JournalId == journalId)
                .Select(t => new JournalDTO
                {
                    JournalId = t.JournalId,
                    JournalTitle = t.JournalTitle,
                    JournalDescription = t.JournalDescription,
                    DateCreated = t.DateCreated,
                    DateModified = t.DateModified,
                    UserId = t.UserId,
                    JournalEntries = t.JournalEntries!.Select(e => new JournalEntry
                    {
                        JournalEntryId = e.JournalEntryId,
                        JournalId = e.JournalId,
                        Title = e.Title,
                        Content = e.Content,
                        DateCreated = e.DateCreated,
                        DateModified = e.DateModified,
                        JournalEntryTags = e.JournalEntryTags!.Select(jet => new JournalEntryTag
                        {
                            JournalEntryId = jet.JournalEntryId,
                            TagId = jet.TagId,
                            Tag = new Tag
                            {
                                TagId = jet.Tag.TagId,
                                Name = jet.Tag.Name,
                                UserId = jet.Tag.UserId
                            }
                        }).ToList()
                    }).ToList()
                })
                .FirstOrDefaultAsync();

			if (journalItem == null)
			{
				return NotFound();
			}

			return Ok(journalItem);
		}
		
		[HttpPost]
        [Authorize]
		public async Task<ActionResult<Journals>> CreateJournal([FromBody] JournalDTO journalDTO) 
		{
		    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if(string.IsNullOrEmpty(userId)) 
			{
			    return Unauthorized("You are not logged in");
			}
			
			if (journalDTO == null || string.IsNullOrEmpty(journalDTO.JournalTitle))
            {
                return BadRequest("Journal title is required.");
            }
            
            var now = DateTime.UtcNow;
            
            var newJournal = new Journals
            {
                JournalTitle = journalDTO.JournalTitle,
                JournalDescription = journalDTO.JournalDescription,
                UserId = userId,
                DateCreated = now,
                DateModified = now
            };

            await dbContext.Journals.AddAsync(newJournal);
            await dbContext.SaveChangesAsync();
            
            newJournal.User = await dbContext.Users.FindAsync(userId);

            // Return the created journal as confirmation
            return CreatedAtAction(nameof(GetJournals), new { id = newJournal.JournalId }, newJournal);
		}
		
        [HttpPut("{journalId}")]
        [Authorize]
        public async Task<ActionResult<Journals>> UpdateJournal(int journalId, [FromBody] JournalDTO journalDTO) 
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("You are not logged in");
            }
            
            var journal = await dbContext.Journals
                .FirstOrDefaultAsync(j => j.JournalId == journalId && j.UserId == userId);

            if (journal == null)
            {
                return NotFound("Journal not found.");
            }

            journal.JournalTitle = journalDTO.JournalTitle ?? journal.JournalTitle;
            journal.JournalDescription = journalDTO.JournalDescription ?? journal.JournalDescription;
            journal.DateModified = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();

            return NoContent();
        }
        
        [HttpDelete("{journalId}")]
        [Authorize]
        public async Task<IActionResult> DeleteJournal(int journalId) 
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("You are not logged in");
            }
            
            var journal = await dbContext.Journals
                .FirstOrDefaultAsync(j => j.JournalId == journalId && j.UserId == userId);

            if (journal == null) 
            {
                return NotFound("Journal not found.");
            }
            
            dbContext.Journals.Remove(journal);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}