using Microsoft.EntityFrameworkCore;
using SkillSwap.Models;

namespace SkillSwap.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<SkillOffer> SkillOffers => Set<SkillOffer>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<CreditTransaction> CreditTransactions => Set<CreditTransaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Category>().HasIndex(c => c.Name).IsUnique();

            // Store enums as readable text instead of numbers
            modelBuilder.Entity<SkillOffer>().Property(s => s.Level).HasConversion<string>().HasMaxLength(20);
            modelBuilder.Entity<SkillOffer>().Property(s => s.Mode).HasConversion<string>().HasMaxLength(20);
            modelBuilder.Entity<Booking>().Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
            modelBuilder.Entity<CreditTransaction>().Property(t => t.Type).HasConversion<string>().HasMaxLength(20);

            modelBuilder.Entity<SkillOffer>()
                .HasOne(s => s.Teacher).WithMany(u => u.SkillOffers)
                .HasForeignKey(s => s.TeacherId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SkillOffer>()
                .HasOne(s => s.Category).WithMany(c => c.SkillOffers)
                .HasForeignKey(s => s.CategoryId).OnDelete(DeleteBehavior.Restrict);

            // A booking points at two users, so both relationships are spelled out
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Learner).WithMany()
                .HasForeignKey(b => b.LearnerId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Teacher).WithMany()
                .HasForeignKey(b => b.TeacherId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.SkillOffer).WithMany(s => s.Bookings)
                .HasForeignKey(b => b.SkillOfferId).OnDelete(DeleteBehavior.Restrict);

            // At most one review per booking
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Booking).WithOne(b => b.Review)
                .HasForeignKey<Review>(r => r.BookingId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Review>().HasIndex(r => r.BookingId).IsUnique();

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Reviewer).WithMany()
                .HasForeignKey(r => r.ReviewerId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Teacher).WithMany()
                .HasForeignKey(r => r.TeacherId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CreditTransaction>()
                .HasOne(t => t.User).WithMany()
                .HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CreditTransaction>()
                .HasOne(t => t.Booking).WithMany()
                .HasForeignKey(t => t.BookingId).OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Programming", Icon = "💻" },
                new Category { Id = 2, Name = "Music", Icon = "🎸" },
                new Category { Id = 3, Name = "Languages", Icon = "🗣️" },
                new Category { Id = 4, Name = "Design", Icon = "🎨" },
                new Category { Id = 5, Name = "Academics", Icon = "📚" },
                new Category { Id = 6, Name = "Sports & Fitness", Icon = "🏸" },
                new Category { Id = 7, Name = "Other", Icon = "✨" });
        }
    }
}
