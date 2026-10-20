using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Filters;
using SkillSwap.Models;
using SkillSwap.Services;
using SkillSwap.ViewModels;

namespace SkillSwap.Controllers
{
    [RequireLogin]
    public class BookingsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly CreditService _credits;

        public BookingsController(AppDbContext db, CreditService credits)
        {
            _db = db;
            _credits = credits;
        }

        private int CurrentUserId => HttpContext.Session.GetUserId()!.Value;

        // Sessions I'm learning in and sessions I'm teaching
        public async Task<IActionResult> Index()
        {
            var all = await _db.Bookings
                .Include(b => b.SkillOffer).Include(b => b.Learner).Include(b => b.Teacher).Include(b => b.Review)
                .Where(b => b.LearnerId == CurrentUserId || b.TeacherId == CurrentUserId)
                .OrderBy(b => b.Status == BookingStatus.Pending ? 0 : b.Status == BookingStatus.Accepted ? 1 : 2)
                .ThenByDescending(b => b.ScheduledAt)
                .ToListAsync();

            return View(new BookingsViewModel
            {
                AsLearner = all.Where(b => b.LearnerId == CurrentUserId).ToList(),
                AsTeacher = all.Where(b => b.TeacherId == CurrentUserId).ToList()
            });
        }

        [HttpGet]
        public async Task<IActionResult> Create(int skillId)
        {
            var offer = await _db.SkillOffers.Include(s => s.Teacher).FirstOrDefaultAsync(s => s.Id == skillId);
            if (offer == null) return NotFound();
            if (offer.TeacherId == CurrentUserId)
            {
                TempData["Error"] = "You cannot book your own skill.";
                return RedirectToAction("Details", "Skills", new { id = skillId });
            }

            var me = await _db.Users.FindAsync(CurrentUserId);
            return View(new BookingCreateViewModel
            {
                SkillOfferId = skillId,
                Offer = offer,
                MyCredits = me!.Credits,
                ScheduledAt = DateTime.Today.AddDays(1).AddHours(17)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingCreateViewModel model)
        {
            var offer = await _db.SkillOffers.Include(s => s.Teacher).FirstOrDefaultAsync(s => s.Id == model.SkillOfferId);
            if (offer == null) return NotFound();

            if (ModelState.IsValid)
            {
                var (booking, error) = await _credits.RequestSessionAsync(CurrentUserId, offer, model.ScheduledAt!.Value, model.Message?.Trim());
                if (booking != null)
                {
                    TempData["Success"] = $"Request sent to {offer.Teacher!.FullName}. {booking.Hours} credit(s) are on hold until the session is done.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, error!);
            }

            model.Offer = offer;
            model.MyCredits = (await _db.Users.FindAsync(CurrentUserId))!.Credits;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int id)
        {
            var booking = await FindAsync(id);
            if (booking == null || booking.TeacherId != CurrentUserId || booking.Status != BookingStatus.Pending)
                return BadAction();

            booking.Status = BookingStatus.Accepted;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Session with {booking.Learner!.FullName} accepted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decline(int id)
        {
            var booking = await FindAsync(id);
            if (booking == null || booking.TeacherId != CurrentUserId || booking.Status != BookingStatus.Pending)
                return BadAction();

            await _credits.RefundAsync(booking, BookingStatus.Declined, $"Refund: \"{booking.SkillOffer!.Title}\" was declined");
            TempData["Success"] = "Request declined. The learner's credits were refunded.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await FindAsync(id);
            if (booking == null || booking.LearnerId != CurrentUserId
                || (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Accepted))
                return BadAction();

            await _credits.RefundAsync(booking, BookingStatus.Cancelled, $"Refund: you cancelled \"{booking.SkillOffer!.Title}\"");
            TempData["Success"] = "Session cancelled and your credits were refunded.";
            return RedirectToAction(nameof(Index));
        }

        // The learner confirms the session happened, which pays the teacher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var booking = await FindAsync(id);
            if (booking == null || booking.LearnerId != CurrentUserId || booking.Status != BookingStatus.Accepted)
                return BadAction();

            await _credits.CompleteAsync(booking);
            TempData["Success"] = $"Marked as done. {booking.Teacher!.FullName} earned {booking.Hours} credit(s). Leave a review!";
            return RedirectToAction(nameof(Review), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Review(int id)
        {
            var booking = await FindAsync(id);
            if (booking == null || booking.LearnerId != CurrentUserId || booking.Status != BookingStatus.Completed || booking.Review != null)
                return BadAction();

            return View(new ReviewViewModel { BookingId = id, Booking = booking, Rating = 5 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(ReviewViewModel model)
        {
            var booking = await FindAsync(model.BookingId);
            if (booking == null || booking.LearnerId != CurrentUserId || booking.Status != BookingStatus.Completed || booking.Review != null)
                return BadAction();

            if (!ModelState.IsValid)
            {
                model.Booking = booking;
                return View(model);
            }

            _db.Reviews.Add(new Review
            {
                BookingId = booking.Id,
                ReviewerId = CurrentUserId,
                TeacherId = booking.TeacherId,
                Rating = model.Rating,
                Comment = model.Comment?.Trim()
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = "Thanks for the review!";
            return RedirectToAction(nameof(Index));
        }

        private Task<Booking?> FindAsync(int id) =>
            _db.Bookings
                .Include(b => b.SkillOffer).Include(b => b.Learner).Include(b => b.Teacher).Include(b => b.Review)
                .FirstOrDefaultAsync(b => b.Id == id);

        private IActionResult BadAction()
        {
            TempData["Error"] = "That action isn't available for this session.";
            return RedirectToAction(nameof(Index));
        }
    }
}
