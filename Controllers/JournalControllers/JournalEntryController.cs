using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using InnerBlend.API.Data;
using InnerBlend.API.Models.DTO;
using InnerBlend.API.Models.Journal;
using InnerBlend.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InnerBlend.API.Controllers.JournalControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JournalEntryController(ApplicationDbContext dbContext, BlobStorageServices _blobStorageServices) : ControllerBase
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
                Mood = journalEntry.Mood.ToString(),
                Location = journalEntry.Location,
                DateCreated = journalEntry.DateCreated,
                DateModified = journalEntry.DateModified,
                ImageUrls = journalEntry.Images?.Select(i => i.ImageUrl!).ToList() ?? []
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
                    Mood = e.Mood.ToString(),
                    Location = e.Location,
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

            Mood? parsedMood = null;
            if (!string.IsNullOrWhiteSpace(entryDTO.Mood))
            {
                var normalized = entryDTO.Mood.Replace(" ", "", StringComparison.OrdinalIgnoreCase);
                if (Enum.TryParse(normalized, true, out Mood moodEnum))
                {
                    parsedMood = moodEnum;
                }
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
                Mood = parsedMood,
                Location = entryDTO.Location,
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
                Location = entry.Location,
                Mood = entry.Mood?.ToString(),
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
            if (Enum.TryParse(entryDTO.Mood, out Mood updatedMood))
            {
                entry.Mood = updatedMood;
            }
            entry.Location = entryDTO.Location;


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

        // DELETE: api/journalentry/entryId
        [HttpDelete("{entryId}")]
        [Authorize]
        public async Task<IActionResult> DeleteJournalEntry(int entryId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

            // Fetch entry with tags included
            var entry = await dbContext.JournalEntries
                .Include(e => e.JournalEntryTags)
                .ThenInclude(jt => jt.Tag)
                .FirstOrDefaultAsync(e => e.JournalEntryId == entryId && e.Journal != null && e.Journal.UserId == userId);

            if (entry == null)
            {
                return NotFound("Journal entry not found.");
            }

            // Remove tag relationships (without deleting the tags themselves)
            if (entry.JournalEntryTags != null && entry.JournalEntryTags.Any())
            {
                dbContext.JournalEntryTags.RemoveRange(entry.JournalEntryTags);
            }

            dbContext.JournalEntries.Remove(entry);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }

        // MOVE JOURNAL ENTRY TO DIFFERENT JOURNAL
        [HttpPut("move-entry")] // PUT: api/journalentry/entryId/journal/journalId
        [Authorize]
        public async Task<IActionResult> MoveJournalEntry([FromBody] MoveEntryDTO moveEntryDTO)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null) return Unauthorized("User not authenticated");

            var entry = await dbContext.JournalEntries
                    .Include(e => e.Journal)
                    .FirstOrDefaultAsync(e => e.JournalEntryId == moveEntryDTO.EntryId && e.Journal != null && e.Journal.UserId == userId);

            if (entry == null) return NotFound("Journal entry not found");

            var newJournal = await dbContext.Journals
                    .FirstOrDefaultAsync(j => j.JournalId == moveEntryDTO.JournalId && j.UserId == userId);

            if (newJournal == null) return NotFound("Journal not found");

            entry.JournalId = newJournal.JournalId;
            entry.DateModified = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();

            return Ok("Entry moved successfully");
        }


        [HttpPost("{entryId}/images")]
        [Authorize]
        public async Task<IActionResult> UploadImages(int entryId, List<IFormFile> files)
        {
            if (files == null || !files.Any()) return BadRequest("No files provided");

            var journalEntry = await dbContext.JournalEntries.FindAsync(entryId);
            if (journalEntry == null) return NotFound("Journal Entry not found");

            var imageEntities = new List<JournalEntryImages>();

            foreach (var file in files)
            {
                if (file.Length > 5 * 1024 * 1024) 
                { 
                    return BadRequest("File too large"); 
                };
                
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
                
                if (!allowedTypes.Contains(file.ContentType)) 
                {
                    return BadRequest($"Unsupported file type: {file.ContentType}"); 
                }
                
                var compressedStream = await _blobStorageServices.CompressAndResizeImageAsync(file); // compressed image
                
                var imageUrl = await _blobStorageServices.UploadAsync(compressedStream, file.FileName);
                
                var image = new JournalEntryImages
                {
                    JournalEntryId = entryId,
                    ImageUrl = imageUrl
                };

                imageEntities.Add(image);
            }

            dbContext.AddRange(imageEntities);
            await dbContext.SaveChangesAsync();

            return Ok(new { message = "Images uploaded successfully", images = imageEntities });
        }

        [HttpDelete("{entryId}/images")]
        public async Task<IActionResult> DeleteImages(int entryId, [FromBody] List<string> imageUrls)
        {
            var images = dbContext.Set<JournalEntryImages>()
                .Where(i => i.JournalEntryId == entryId && i.ImageUrl != null && imageUrls.Contains(i.ImageUrl))
                .ToList();

            foreach (var image in images)
            {
                if (image.ImageUrl != null) await _blobStorageServices.DeleteAsync(image.ImageUrl);
            }

            dbContext.RemoveRange(images);
            await dbContext.SaveChangesAsync();

            return Ok("Deleted Successfully");
        }

        [HttpPut("{entryId}/images")]
        public async Task<IActionResult> ReplaceImage(int entryId, [FromQuery] string oldImageUrl, IFormFile newFile)
        {
            if (newFile == null || string.IsNullOrWhiteSpace(oldImageUrl)) return BadRequest("No file provided");
            
            if (newFile.Length > 5 * 1024 * 1024) return BadRequest("File too large");
            
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            
            if (!allowedTypes.Contains(newFile.ContentType)) return BadRequest($"Unsupported file type: {newFile.ContentType}");

            var image = await dbContext.Set<JournalEntryImages>()
                .FirstOrDefaultAsync(i => i.JournalEntryId == entryId && i.ImageUrl == oldImageUrl);

            if (image == null) return NotFound("Image not found");
            
            // Delete old blob
            await _blobStorageServices.DeleteAsync(oldImageUrl);
            
            // Compress and upload new image
            var compressedStream = await _blobStorageServices.CompressAndResizeImageAsync(newFile);
            var newImageUrl = await _blobStorageServices.UploadAsync(compressedStream, newFile.FileName);

            image.ImageUrl = newImageUrl;
            await dbContext.SaveChangesAsync();

            return Ok(new { message = "Image replaced successfully", newImageUrl });
        }
    }
}