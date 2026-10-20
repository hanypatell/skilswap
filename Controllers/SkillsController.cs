using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Filters;
using SkillSwap.Models;
using SkillSwap.ViewModels;

namespace SkillSwap.Controllers
{
    public class SkillsController : Controller
    {
        private readonly AppDbContext _db;

        public SkillsController(AppDbContext db)
        {
            _db = db;
        }

        // Browse all active skills with search and category filter
        public async Task<IActionResult> Index(string? search, int? categoryId)
        {
            var query = _db.SkillOffers
                .Include(s => s.Teacher).Include(s => s.Category)
                .Where(s => s.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(s => s.Title.Contains(term) || s.Description.Contains(term) || s.Teacher!.FullName.Contains(term));
            }
            if (categoryId.HasValue)
                query = query.Where(s => s.CategoryId == categoryId);

            var offers = await query.OrderByDescending(s => s.CreatedAt).ToListAsync();

            return View(new SkillBrowseViewModel
            {
                Skills = await ToCardsAsync(_db, offers),
                Categories = await _db.Categories.OrderBy(c => c.Id).ToListAsync(),
                Search = search,
                CategoryId = categoryId
            });
        }

        public async Task<IActionResult> Details(int id)
        {
            var offer = await _db.SkillOffers
                .Include(s => s.Teacher).Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (offer == null) return NotFound();

            var reviews = await _db.Reviews
                .Include(r => r.Reviewer)
                .Where(r => r.Booking!.SkillOfferId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var userId = HttpContext.Session.GetUserId();
            var viewer = userId == null ? null : await _db.Users.FindAsync(userId);

            return View(new SkillDetailsViewModel
            {
                Offer = offer,
                Reviews = reviews,
                AverageRating = reviews.Count > 0 ? reviews.Average(r => r.Rating) : null,
                IsOwner = userId == offer.TeacherId,
                IsLoggedIn = userId != null,
                ViewerCredits = viewer?.Credits ?? 0
            });
        }

        [RequireLogin]
        public async Task<IActionResult> Mine()
        {
            var userId = HttpContext.Session.GetUserId();
            var offers = await _db.SkillOffers
                .Include(s => s.Teacher).Include(s => s.Category)
                .Where(s => s.TeacherId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
            return View(await ToCardsAsync(_db, offers));
        }

        [RequireLogin]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadCategoriesAsync();
            return View(new SkillOffer());
        }

        [RequireLogin]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Level,Mode,DurationHours,CategoryId")] SkillOffer offer)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return View(offer);
            }

            offer.TeacherId = HttpContext.Session.GetUserId()!.Value;
            offer.IsActive = true;
            _db.SkillOffers.Add(offer);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Your skill is live. Others can now book sessions with you.";
            return RedirectToAction(nameof(Details), new { id = offer.Id });
        }

        [RequireLogin]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var offer = await FindOwnedAsync(id);
            if (offer == null) return NotFound();

            await LoadCategoriesAsync();
            return View(offer);
        }

        [RequireLogin]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Title,Description,Level,Mode,DurationHours,CategoryId,IsActive")] SkillOffer input)
        {
            var offer = await FindOwnedAsync(id);
            if (offer == null) return NotFound();

            if (!ModelState.IsValid)
            {
                input.Id = id;
                await LoadCategoriesAsync();
                return View(input);
            }

            offer.Title = input.Title;
            offer.Description = input.Description;
            offer.Level = input.Level;
            offer.Mode = input.Mode;
            offer.DurationHours = input.DurationHours;
            offer.CategoryId = input.CategoryId;
            offer.IsActive = input.IsActive;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Skill updated.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [RequireLogin]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var offer = await FindOwnedAsync(id);
            if (offer == null) return NotFound();

            ViewBag.HasBookings = await _db.Bookings.AnyAsync(b => b.SkillOfferId == id);
            return View(offer);
        }

        [RequireLogin]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var offer = await FindOwnedAsync(id);
            if (offer == null) return NotFound();

            // Keep history intact: skills with bookings can only be paused
            if (await _db.Bookings.AnyAsync(b => b.SkillOfferId == id))
            {
                TempData["Error"] = "This skill has session history, so it can't be deleted. It has been paused instead.";
                offer.IsActive = false;
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Mine));
            }

            _db.SkillOffers.Remove(offer);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Skill deleted.";
            return RedirectToAction(nameof(Mine));
        }

        private async Task<SkillOffer?> FindOwnedAsync(int id)
        {
            var userId = HttpContext.Session.GetUserId();
            return await _db.SkillOffers.Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == id && s.TeacherId == userId);
        }

        private async Task LoadCategoriesAsync()
        {
            ViewBag.Categories = new SelectList(await _db.Categories.OrderBy(c => c.Id).ToListAsync(), "Id", "Name");
        }

        // Attach each teacher's average rating for the skill cards
        public static async Task<List<SkillCard>> ToCardsAsync(AppDbContext db, List<SkillOffer> offers)
        {
            var ids = offers.Select(o => o.Id).ToList();
            var stats = await db.Reviews
                .Where(r => ids.Contains(r.Booking!.SkillOfferId))
                .GroupBy(r => r.Booking!.SkillOfferId)
                .Select(g => new { OfferId = g.Key, Avg = g.Average(r => r.Rating), Count = g.Count() })
                .ToDictionaryAsync(x => x.OfferId);

            return offers.Select(o => new SkillCard
            {
                Offer = o,
                AverageRating = stats.TryGetValue(o.Id, out var s) ? s.Avg : null,
                ReviewCount = stats.TryGetValue(o.Id, out var c) ? c.Count : 0
            }).ToList();
        }
    }
}
