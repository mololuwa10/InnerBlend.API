using System;
using System.Collections.Generic;
using System.Linq;
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
            var journal = await dbContext.Journals.FindAsync(journalId);
            if (journal == null)
            {
                return NotFound("Journal not found.");
            }
            
            var now = DateTime.UtcNow;
            
            // Convert tag names from the DTO into Tag objects (or create new ones)
            var tags = new List<Tag>();
            foreach (var tagName in entryDTO.Tags ?? [])
            {
                var tag = await dbContext.Tags
                    .FirstOrDefaultAsync(t => t.Name == tagName);
                if (tag == null)
                {
                    tag = new Tag { Name = tagName };
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
    }
}