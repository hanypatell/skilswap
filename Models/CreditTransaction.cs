using System.ComponentModel.DataAnnotations;

namespace SkillSwap.Models
{
    public enum TransactionType { SignupBonus, Hold, Refund, Earned }

    // One row per credit movement, so a user's balance can always be explained
    public class CreditTransaction
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        // Positive adds credits, negative removes them
        public int Amount { get; set; }

        public TransactionType Type { get; set; }

        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        public int? BookingId { get; set; }
        public Booking? Booking { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
