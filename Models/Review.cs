using System.ComponentModel.DataAnnotations;

namespace SkillSwap.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        public int ReviewerId { get; set; }
        public User? Reviewer { get; set; }

        public int TeacherId { get; set; }
        public User? Teacher { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(500)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
