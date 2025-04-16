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
    public class JournalEntryController(ApplicationDbContext dbContext) : ControllerBase
    {
        // GET: api/journalentry/entryId
        // Each entry has a unique ID
        [HttpGet("{entryId}")]
        [Authorize]
        public async Task<ActionResult<JournalEntry>> GetJournalEntry(int entryId)
        {
            var journalEntry = await dbContext.JournalEntries
                .Include(e => e.JournalEntryTags!)
                    .ThenInclude(jt => jt.Tag)
                .FirstOrDefaultAsync(e => e.JournalEntryId == entryId);
                
            if (journalEntry == null) 
            {
                return NotFound("Journal entry not found.");
            }
            
            var entryDTO = new JournalEntryDTO
            {
                JournalEntryId = journalEntry.JournalEntryId,
                JournalId = journalEntry.JournalId,
                Title = journalEntry.Title,
                Content = journalEntry.Content,
                Tags = journalEntry.JournalEntryTags != null 
                        ? journalEntry.JournalEntryTags
                            .Where(jt => jt.Tag != null && jt.Tag.Name != null)
                            .Select(jt => jt.Tag!.Name!)
                            .ToList()
                        : [],
                DateCreated = journalEntry.DateCreated,
                DateModified = journalEntry.DateModified
            };

            return Ok(entryDTO);
        }
        
        // GET: api/journalentry/journalId
        // Each entry has to be tied to a particular journal
        // so that getting all entries for a journal is possible
        [HttpGet("journal/{journalId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<JournalEntry>>> GetEntriesByJournal(int journalId) 
        {
            var journal = await dbContext.Journals.FindAsync(journalId);
            if (journal == null)
            {
                return NotFound("Journal not found.");
            }
            
            var entries = await dbContext.JournalEntries
                .Where(e => e.JournalId == journalId)
                .Select(e => new JournalEntryDTO
                {
                    JournalEntryId = e.JournalEntryId,
                    JournalId = e.JournalId,
                    Title = e.Title,
                    Content = e.Content,
                    Tags = e.JournalEntryTags != null 
                        ? e.JournalEntryTags
                            .Where(jt => jt.Tag != null && jt.Tag.Name != null)
                            .Select(jt => jt.Tag!.Name!)
                            .ToList()
                        : new List<string>(),
                    DateCreated = e.DateCreated,
                    DateModified = e.DateModified
                }).ToListAsync();

            return Ok(entries);
        }
        
        // POST: api/journalentry/journalId
        [HttpPost("journal/{journalId}")] 
        [Authorize]
        public async Task<ActionResult<JournalEntry>> CreateJournalEntry(int journalId, [FromBody] JournalEntryDTO entryDTO) 
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            
            var journal = await dbContext.Journals.FindAsync(journalId);
            
            if (journal == null)
            {
                return NotFound("Journal not found.");
            }
            
            if (string.IsNullOrWhiteSpace(entryDTO.Title) || string.IsNullOrWhiteSpace(entryDTO.Content))
            {
                return BadRequest("Title and content are required.");
            }
       
            var now = DateTime.UtcNow;
            
            // Convert tag names from the DTO into Tag objects (or create new ones)
            var tags = new List<Tag>();
            foreach (var tagName in entryDTO.Tags ?? [])
            {
                var trimmedName = tagName.Trim().ToLower();

                var tag = await dbContext.Tags
                    .FirstOrDefaultAsync(t => t.Name != null && t.Name.ToLower() == trimmedName && t.UserId == userId);

                if (tag == null)
                {
                    tag = new Tag { Name = trimmedName, UserId = userId };
                    dbContext.Tags.Add(tag);
                }

                tags.Add(tag);
            }
            
            var entry = new JournalEntry
            {
                JournalId = journalId,
                Title = entryDTO.Title,
                Content = entryDTO.Content,
                JournalEntryTags = tags.Select(tag => new JournalEntryTag
                {
                    Tag = tag
                }).ToList(),
                DateCreated = now,
                DateModified = now
            };

            await dbContext.JournalEntries.AddAsync(entry);
            await dbContext.SaveChangesAsync();
            
            var resultDTO = new JournalEntryDTO
            {
                JournalEntryId = entry.JournalEntryId,
                JournalId = entry.JournalId,
                Title = entry.Title,
                Content = entry.Content,
                Tags = entry.JournalEntryTags?
                    .Where(jt => jt.Tag != null && jt.Tag.Name != null)
                    .Select(jt => jt.Tag!.Name!)
                    .ToList() ?? [],
                DateCreated = entry.DateCreated,
                DateModified = entry.DateModified
            };
            
            return CreatedAtAction(nameof(GetJournalEntry), new { entryId = entry.JournalEntryId }, resultDTO);
        }
        
        // PUT: api/journalentry/entryId
        [HttpPut("{entryId}")] 
        [Authorize]
        public async Task<IActionResult> UpdateJournalEntry(int entryId, [FromBody] JournalEntryDTO entryDTO) 
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            
            #pragma warning disable CS8620 
            var entry = await dbContext.JournalEntries
                .Include(e => e.JournalEntryTags)
                .ThenInclude(jt => jt.Tag)
                .FirstOrDefaultAsync(e => e.JournalEntryId == entryId);

            if (entry == null) 
            {
                return NotFound("Journal entry not found.");
            }
            
            entry.Title = entryDTO.Title;
            entry.Content = entryDTO.Content;
            entry.DateModified = DateTime.UtcNow;
            
            // HANDLE TAGS
            var newTags = entryDTO.Tags?.Select(t => t.Trim().ToLower()).Distinct().ToList() ?? [];
            
            // Get existing tags from DB
            var existingTags = await dbContext.Tags
                .Where(t => newTags.Contains(t.Name!.ToLower()) && t.UserId == userId)
                .ToListAsync();
                
            // Tags to add (not in DB yet)
            var tagsToAdd = newTags
                .Where(t => !existingTags.Any(et => et.Name!.ToLower() == t))
                .Select(t => new Tag { Name = t, UserId = userId })
                .ToList();
                
            // Add new tags to DB
            dbContext.Tags.AddRange(tagsToAdd);
            await dbContext.SaveChangesAsync();
            
            var allTags = existingTags.Concat(tagsToAdd).ToList();
            
            // Remove old tag links
            if (entry.JournalEntryTags != null)
            {
                dbContext.JournalEntryTags.RemoveRange(entry.JournalEntryTags);
            }

            // Add updated tag links
            entry.JournalEntryTags = allTags
                .Select(t => new JournalEntryTag
                {
                    JournalEntryId = entryId,
                    TagId = t.TagId
                }).ToList();

            await dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}