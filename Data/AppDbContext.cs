using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StackOverFlowClone.Models.Entities;
using StackOverFlowClone.Models.Role;

namespace StackOverFlowClone.Data
{
    public class AppDbContext : IdentityDbContext<User, Role, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Bookmark>()
        .HasIndex(b => new { b.UserId, b.QuestionId })
        .IsUnique();

            modelBuilder.Entity<Bookmark>()
                    .HasOne(b => b.Question)
                    .WithMany(q => q.Bookmarks)
                    .HasForeignKey(b => b.QuestionId)
                    .OnDelete(DeleteBehavior.Restrict); // أو DeleteBehavior.NoAction

            modelBuilder.Entity<Bookmark>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookmarks)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade); // أو Restrict إذا كان هناك تضارب آخر

            modelBuilder.Entity<QuestionTag>()
                     .HasKey(qt => new { qt.QuestionId, qt.TagId });

            modelBuilder.Entity<QuestionTag>()
                .HasOne(qt => qt.Question)
                .WithMany(q => q.QuestionTags)
                .HasForeignKey(qt => qt.QuestionId);

            modelBuilder.Entity<QuestionTag>()
                .HasOne(qt => qt.Tag)
                .WithMany(t => t.QuestionTags)
                .HasForeignKey(qt => qt.TagId);


            // Answer - Question relationship
            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Answer - User relationship
            modelBuilder.Entity<Answer>()
                .HasOne(a => a.User)
                .WithMany(u => u.Answers)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Question - User relationship
            modelBuilder.Entity<Question>()
                .HasOne(q => q.User)
                .WithMany(u => u.Questions)
                .HasForeignKey(q => q.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment - User relationship
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Vote - User relationship
            modelBuilder.Entity<Vote>()
                .HasOne(v => v.User)
                .WithMany(u => u.Votes)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment - Question (optional polymorphic)
            /* modelBuilder.Entity<Comment>()
                 .HasOne(c => c.Question)
                 .WithMany(q => q.Comments)
                 .HasForeignKey(c => c.QuestionId)
                 .OnDelete(DeleteBehavior.Restrict)
                 .IsRequired(false);

             // Comment - Answer (optional polymorphic)
             modelBuilder.Entity<Comment>()
                 .HasOne(c => c.Answer)
                 .WithMany(a => a.Comments)
                 .HasForeignKey(c => c.AnswerId)
                 .OnDelete(DeleteBehavior.Restrict)
                 .IsRequired(false);*/
            // Configure User-Comment relationship
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany() // Or .WithMany(u => u.Comments) if User has navigation
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // No foreign key constraints to Question or Answer for polymorphic design


            modelBuilder.Entity<Vote>()
                          .HasOne(v => v.Question)
                          .WithMany(q => q.Votes)
                          .HasForeignKey(v => v.QuestionId)
                          .OnDelete(DeleteBehavior.Restrict)
                          .IsRequired(false);

            modelBuilder.Entity<Vote>()
                .HasOne(v => v.Answer)
                .WithMany(a => a.Votes)
                .HasForeignKey(v => v.AnswerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<QuestionTag> QuestionTags { get; set; } // Add this line to include QuestionTag entity
        public DbSet<Bookmark> Bookmarks { get; set; } // Add this line to include Bookmark entity


    }
}
