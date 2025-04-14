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
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<JournalEntry>()
                .Property(e => e.Tags)
                .HasColumnType("text[]");

            modelBuilder.Entity<Journals>()
                .HasMany(j => j.JournalEntries)
                .WithOne(e => e.Journal)
                .HasForeignKey(e => e.JournalId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Journals>()
                .HasOne(j => j.User)
                .WithMany()
                .HasForeignKey(j => j.UserId);
        }
    }
}