using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InnerBlend.API.Models;
using InnerBlend.API.Models.Journal;
using Microsoft.EntityFrameworkCore;

namespace InnerBlend.API.Data
{
    public class ApplicationDbContext (DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Journals> Journals { get; set; }
        public DbSet<JournalEntry>  JournalEntries { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<JournalEntryTag> JournalEntryTags { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Relationships for JournalEntries
            modelBuilder.Entity<JournalEntryTag>()
                .HasKey(jt => new {jt.JournalEntryId, jt.TagId});

            modelBuilder.Entity<JournalEntryTag>()
                .HasOne(jt => jt.JournalEntry)
                .WithMany(je => je.JournalEntryTags)
                .HasForeignKey(jt => jt.JournalEntryId);
                
            modelBuilder.Entity<JournalEntryTag>()
                .HasOne(jt => jt.Tag)
                .WithMany(t => t.JournalEntries)
                .HasForeignKey(jt => jt.TagId);
                
            // Relationships for Journals    
            modelBuilder.Entity<Journals>()
                .HasMany(j => j.JournalEntries)
                .WithOne(e => e.Journal)
                .HasForeignKey(e => e.JournalId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Journals>()
                .HasOne(j => j.User)
                .WithMany()
                .HasForeignKey(j => j.UserId);
            
            // Relationships for Tags
            modelBuilder.Entity<Tag>()
                .HasIndex(t => new { t.Name, t.UserId })
                .IsUnique();
        }
    }
}