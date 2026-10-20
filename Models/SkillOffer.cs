using System.ComponentModel.DataAnnotations;

namespace SkillSwap.Models
{
    public enum SkillLevel { Beginner, Intermediate, Advanced }

    public enum SessionMode { Online, [Display(Name = "In person")] InPerson }

    public class SkillOffer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Give your skill a title")]
        [StringLength(80, MinimumLength = 4, ErrorMessage = "Title must be 4 to 80 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Describe what you will teach")]
        [StringLength(1000, MinimumLength = 20, ErrorMessage = "Description must be 20 to 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Level")]
        public SkillLevel Level { get; set; }

        [Display(Name = "Mode")]
        public SessionMode Mode { get; set; }

        [Display(Name = "Session length (hours)")]
        [Range(1, 3, ErrorMessage = "A session must be 1 to 3 hours")]
        public int DurationHours { get; set; } = 1;

        [Display(Name = "Accepting requests")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Category")]
        [Range(1, int.MaxValue, ErrorMessage = "Pick a category")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public int TeacherId { get; set; }
        public User? Teacher { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
