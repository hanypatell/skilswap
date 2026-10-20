using Microsoft.AspNetCore.Identity;
using SkillSwap.Models;
using SkillSwap.Services;

namespace SkillSwap.Data
{
    // Fills an empty database with demo members, skills and past sessions
    public static class DbSeeder
    {
        public const string DemoPassword = "Demo@123";

        public static void Seed(AppDbContext db)
        {
            if (db.Users.Any()) return;

            var hasher = new PasswordHasher<User>();
            var credits = new CreditService(db);

            User NewUser(string name, string email, string dept, string bio)
            {
                var u = new User { FullName = name, Email = email, Department = dept, Bio = bio, CreatedAt = DateTime.Now.AddDays(-30) };
                u.PasswordHash = hasher.HashPassword(u, DemoPassword);
                db.Users.Add(u);
                credits.AddTransaction(u, CreditService.SignupBonus, TransactionType.SignupBonus, "Welcome bonus");
                return u;
            }

            var aarav = NewUser("Aarav Mehta", "aarav@skillswap.test", "Computer Engineering", "Guitarist for 6 years. Want to get better at UI design.");
            var isha = NewUser("Isha Rao", "isha@skillswap.test", "Information Technology", "Competitive programmer. Learning guitar on the side.");
            var kabir = NewUser("Kabir Nair", "kabir@skillswap.test", "Mechanical Engineering", "Badminton state-level player. Struggling with DSA.");
            var meera = NewUser("Meera Joshi", "meera@skillswap.test", "Computer Science", "Figma and Spanish nerd. Happy to help with portfolios.");

            SkillOffer Offer(User t, int cat, string title, string desc, SkillLevel lvl, SessionMode mode, int hrs)
            {
                var o = new SkillOffer { Teacher = t, CategoryId = cat, Title = title, Description = desc, Level = lvl, Mode = mode, DurationHours = hrs, CreatedAt = DateTime.Now.AddDays(-25) };
                db.SkillOffers.Add(o);
                return o;
            }

            var guitar = Offer(aarav, 2, "Acoustic guitar basics", "Learn the open chords, strumming patterns and your first two songs. Bring a guitar if you have one, I can lend mine otherwise.", SkillLevel.Beginner, SessionMode.InPerson, 1);
            Offer(aarav, 1, "C# and ASP.NET Core MVC", "Controllers, views, models and Entity Framework explained with a small project. Good for anyone stuck on practicals.", SkillLevel.Intermediate, SessionMode.Online, 2);
            var dsa = Offer(isha, 1, "DSA for placements", "Arrays, two pointers, sliding window and binary search with LeetCode problems solved live. We pick problems based on your level.", SkillLevel.Intermediate, SessionMode.Online, 2);
            Offer(isha, 5, "Discrete maths doubt solving", "Sets, relations, graphs and proofs. Bring your doubts from class or the last exam paper and we will work through them.", SkillLevel.Beginner, SessionMode.InPerson, 1);
            Offer(kabir, 6, "Badminton footwork and smash", "Court movement, grip and a proper smash. Meet at the sports complex, rackets available.", SkillLevel.Beginner, SessionMode.InPerson, 1);
            Offer(kabir, 5, "AutoCAD 2D drawing", "Draw and dimension a machine part from scratch, with keyboard shortcuts that save hours.", SkillLevel.Beginner, SessionMode.Online, 1);
            var figma = Offer(meera, 4, "Figma from zero", "Frames, auto layout, components and prototyping. By the end you will design a clean app screen on your own.", SkillLevel.Beginner, SessionMode.Online, 1);
            var spanish = Offer(meera, 3, "Conversational Spanish", "Greetings, ordering food and small talk. Lots of speaking practice, very little grammar.", SkillLevel.Beginner, SessionMode.Online, 1);

            void Completed(SkillOffer offer, User learner, int daysAgo, int rating, string comment)
            {
                var b = new Booking { SkillOffer = offer, Learner = learner, Teacher = offer.Teacher, Hours = offer.DurationHours, ScheduledAt = DateTime.Now.AddDays(-daysAgo), CreatedAt = DateTime.Now.AddDays(-daysAgo - 2), Status = BookingStatus.Completed, CompletedAt = DateTime.Now.AddDays(-daysAgo) };
                db.Bookings.Add(b);
                credits.AddTransaction(learner, -b.Hours, TransactionType.Hold, $"Held for \"{offer.Title}\"", b);
                credits.AddTransaction(offer.Teacher!, b.Hours, TransactionType.Earned, $"Taught \"{offer.Title}\"", b);
                db.Reviews.Add(new Review { Booking = b, Reviewer = learner, Teacher = offer.Teacher, Rating = rating, Comment = comment, CreatedAt = DateTime.Now.AddDays(-daysAgo) });
            }

            Completed(guitar, isha, 12, 5, "Super patient. I can play two songs now!");
            Completed(dsa, kabir, 9, 5, "Finally understood sliding window. Great problem picks.");
            Completed(guitar, meera, 6, 4, "Fun session, would have liked a bit more strumming practice.");
            Completed(figma, aarav, 3, 5, "Auto layout finally makes sense. Very clear.");

            var pending = new Booking { SkillOffer = spanish, Learner = kabir, Teacher = meera, Hours = 1, ScheduledAt = DateTime.Now.AddDays(2).Date.AddHours(17), Message = "Going to Spain next summer, want the basics!" };
            db.Bookings.Add(pending);
            credits.AddTransaction(kabir, -1, TransactionType.Hold, $"Held for \"{spanish.Title}\"", pending);

            db.SaveChanges();
        }
    }
}
