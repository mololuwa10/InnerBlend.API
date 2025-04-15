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
            var journalEntry = await dbContext.JournalEntries.FindAsync(entryId);
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
                Tags = journalEntry.Tags,
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
                    Tags = e.Tags,
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
            
            var entry = new JournalEntry
            {
                JournalId = journalId,
                Title = entryDTO.Title,
                Content = entryDTO.Content,
                Tags = entryDTO.Tags,
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
                Tags = entry.Tags,
                DateCreated = entry.DateCreated,
                DateModified = entry.DateModified
            };
            
            return CreatedAtAction(nameof(GetJournalEntry), new { entryId = entry.JournalEntryId }, resultDTO);
        }
    }
}