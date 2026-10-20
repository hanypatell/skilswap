using System.ComponentModel.DataAnnotations;

namespace SkillSwap.Models
{
    public enum BookingStatus { Pending, Accepted, Declined, Cancelled, Completed }

    public class Booking
    {
        public int Id { get; set; }

        public int SkillOfferId { get; set; }
        public SkillOffer? SkillOffer { get; set; }

        public int LearnerId { get; set; }
        public User? Learner { get; set; }

        public int TeacherId { get; set; }
        public User? Teacher { get; set; }

        public DateTime ScheduledAt { get; set; }

        // Credits held from the learner when the request is made
        public int Hours { get; set; }

        [StringLength(300)]
        public string? Message { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? CompletedAt { get; set; }

        public Review? Review { get; set; }
    }
}
