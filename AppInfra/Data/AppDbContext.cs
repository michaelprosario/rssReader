using AppCore.Models;
using AppCore.Models.Bookmarks;
using AppCore.Models.Feeds;
using AppCore.Models.Settings;
using AppCore.Models.Tags;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppInfra.Data
{
    /// <summary>
    /// Database context for the RSS Reader application
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Feed> Feeds { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<BookmarkTag> BookmarkTags { get; set; }
        public DbSet<AppSettings> AppSettings { get; set; }
        public DbSet<ReadingHistory> ReadingHistories { get; set; }
        public DbSet<KeyboardShortcut> KeyboardShortcuts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Feed - Article relationship
            modelBuilder.Entity<Article>()
                .HasOne<Feed>()
                .WithMany(f => f.Articles)
                .HasForeignKey(a => a.FeedId)
                .OnDelete(DeleteBehavior.Cascade);

            // Article - Bookmark relationship
            modelBuilder.Entity<Bookmark>()
                .HasOne(b => b.Article)
                .WithMany()
                .HasForeignKey(b => b.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Bookmark - BookmarkTag - Tag relationship
            modelBuilder.Entity<BookmarkTag>()
                .HasOne(bt => bt.Bookmark)
                .WithMany(b => b.BookmarkTags)
                .HasForeignKey(bt => bt.BookmarkId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookmarkTag>()
                .HasOne(bt => bt.Tag)
                .WithMany(t => t.BookmarkTags)
                .HasForeignKey(bt => bt.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ReadingHistory
            modelBuilder.Entity<ReadingHistory>()
                .HasOne<Article>()
                .WithMany()
                .HasForeignKey(r => r.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
