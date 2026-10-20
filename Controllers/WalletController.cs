using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Filters;
using SkillSwap.Models;
using SkillSwap.ViewModels;

namespace SkillSwap.Controllers
{
    [RequireLogin]
    public class WalletController : Controller
    {
        private readonly AppDbContext _db;

        public WalletController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetUserId()!.Value;
            var user = await _db.Users.FindAsync(userId);

            return View(new WalletViewModel
            {
                Balance = user!.Credits,
                HeldCredits = await _db.Bookings
                    .Where(b => b.LearnerId == userId && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Accepted))
                    .SumAsync(b => (int?)b.Hours) ?? 0,
                TotalEarned = await _db.CreditTransactions
                    .Where(t => t.UserId == userId && t.Type == TransactionType.Earned)
                    .SumAsync(t => (int?)t.Amount) ?? 0,
                Transactions = await _db.CreditTransactions
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.CreatedAt).ThenByDescending(t => t.Id)
                    .ToListAsync()
            });
        }
    }
}
