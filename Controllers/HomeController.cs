using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Models;
using SkillSwap.ViewModels;

namespace SkillSwap.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var latest = await _db.SkillOffers
                .Include(s => s.Teacher).Include(s => s.Category)
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.CreatedAt)
                .Take(3)
                .ToListAsync();

            var model = new HomeViewModel
            {
                MemberCount = await _db.Users.CountAsync(),
                SkillCount = await _db.SkillOffers.CountAsync(s => s.IsActive),
                HoursExchanged = await _db.Bookings.Where(b => b.Status == BookingStatus.Completed).SumAsync(b => (int?)b.Hours) ?? 0,
                LatestSkills = await SkillsController.ToCardsAsync(_db, latest),
                TopTeachers = (await BuildLeaderboardAsync()).Take(5).ToList()
            };
            return View(model);
        }

        public async Task<IActionResult> Leaderboard()
        {
            return View(await BuildLeaderboardAsync());
        }

        private async Task<List<LeaderboardEntry>> BuildLeaderboardAsync()
        {
            var taught = await _db.Bookings
                .Where(b => b.Status == BookingStatus.Completed)
                .GroupBy(b => b.TeacherId)
                .Select(g => new { TeacherId = g.Key, Hours = g.Sum(b => b.Hours), Sessions = g.Count() })
                .ToListAsync();

            var ratings = await _db.Reviews
                .GroupBy(r => r.TeacherId)
                .Select(g => new { TeacherId = g.Key, Avg = g.Average(r => r.Rating) })
                .ToDictionaryAsync(x => x.TeacherId, x => x.Avg);

            var ids = taught.Select(t => t.TeacherId).ToList();
            var users = await _db.Users.Where(u => ids.Contains(u.Id)).ToDictionaryAsync(u => u.Id);

            return taught
                .OrderByDescending(t => t.Hours)
                .ThenByDescending(t => ratings.GetValueOrDefault(t.TeacherId))
                .Select((t, i) => new LeaderboardEntry
                {
                    Rank = i + 1,
                    Name = users[t.TeacherId].FullName,
                    Department = users[t.TeacherId].Department,
                    HoursTaught = t.Hours,
                    SessionsTaught = t.Sessions,
                    AverageRating = ratings.TryGetValue(t.TeacherId, out var avg) ? avg : null
                })
                .ToList();
        }

        public IActionResult HowItWorks()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
