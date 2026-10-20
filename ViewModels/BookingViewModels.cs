using System.ComponentModel.DataAnnotations;
using SkillSwap.Models;

namespace SkillSwap.ViewModels
{
    public class BookingCreateViewModel : IValidatableObject
    {
        public int SkillOfferId { get; set; }

        [Required(ErrorMessage = "Pick a date and time")]
        [Display(Name = "Preferred date and time")]
        public DateTime? ScheduledAt { get; set; }

        [StringLength(300, ErrorMessage = "Keep the message under 300 characters")]
        [Display(Name = "Message to the teacher")]
        public string? Message { get; set; }

        // Display only
        public SkillOffer? Offer { get; set; }
        public int MyCredits { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ScheduledAt.HasValue && ScheduledAt.Value <= DateTime.Now.AddHours(1))
                yield return new ValidationResult("Pick a time at least 1 hour from now", new[] { nameof(ScheduledAt) });
        }
    }

    public class ReviewViewModel
    {
        public int BookingId { get; set; }

        [Range(1, 5, ErrorMessage = "Choose 1 to 5 stars")]
        public int Rating { get; set; }

        [StringLength(500, ErrorMessage = "Keep the review under 500 characters")]
        public string? Comment { get; set; }

        public Booking? Booking { get; set; }
    }

    public class BookingsViewModel
    {
        public List<Booking> AsLearner { get; set; } = new();
        public List<Booking> AsTeacher { get; set; } = new();
    }

    public class WalletViewModel
    {
        public int Balance { get; set; }
        public int HeldCredits { get; set; }
        public int TotalEarned { get; set; }
        public List<CreditTransaction> Transactions { get; set; } = new();
    }
}
