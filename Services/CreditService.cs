using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Models;

namespace SkillSwap.Services
{
    // All credit movements go through here so balances and the ledger never disagree.
    // Flow: request -> learner's credits are held; decline/cancel -> refunded;
    // complete -> teacher earns them.
    public class CreditService
    {
        public const int SignupBonus = 3;

        private readonly AppDbContext _db;

        public CreditService(AppDbContext db)
        {
            _db = db;
        }

        public void AddTransaction(User user, int amount, TransactionType type, string description, Booking? booking = null)
        {
            user.Credits += amount;
            _db.CreditTransactions.Add(new CreditTransaction
            {
                User = user,
                Amount = amount,
                Type = type,
                Description = description,
                Booking = booking
            });
        }

        public async Task<(Booking? booking, string? error)> RequestSessionAsync(int learnerId, SkillOffer offer, DateTime scheduledAt, string? message)
        {
            if (offer.TeacherId == learnerId)
                return (null, "You cannot book your own skill.");
            if (!offer.IsActive)
                return (null, "This skill is not accepting requests right now.");

            await using var tx = await _db.Database.BeginTransactionAsync();

            var learner = await _db.Users.FirstAsync(u => u.Id == learnerId);
            if (learner.Credits < offer.DurationHours)
                return (null, $"You need {offer.DurationHours} credit(s) but have {learner.Credits}. Teach a session to earn more.");

            var booking = new Booking
            {
                SkillOfferId = offer.Id,
                LearnerId = learnerId,
                TeacherId = offer.TeacherId,
                ScheduledAt = scheduledAt,
                Hours = offer.DurationHours,
                Message = message
            };
            _db.Bookings.Add(booking);
            AddTransaction(learner, -offer.DurationHours, TransactionType.Hold, $"Held for \"{offer.Title}\"", booking);

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return (booking, null);
        }

        public async Task RefundAsync(Booking booking, BookingStatus newStatus, string reason)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();

            var learner = await _db.Users.FirstAsync(u => u.Id == booking.LearnerId);
            booking.Status = newStatus;
            AddTransaction(learner, booking.Hours, TransactionType.Refund, reason, booking);

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }

        public async Task CompleteAsync(Booking booking)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();

            var teacher = await _db.Users.FirstAsync(u => u.Id == booking.TeacherId);
            booking.Status = BookingStatus.Completed;
            booking.CompletedAt = DateTime.Now;
            AddTransaction(teacher, booking.Hours, TransactionType.Earned,
                $"Taught \"{booking.SkillOffer?.Title}\"", booking);

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
    }
}
