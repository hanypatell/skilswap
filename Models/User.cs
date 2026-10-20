using System.ComponentModel.DataAnnotations;

namespace SkillSwap.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(60)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(120)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(60)]
        public string? Department { get; set; }

        [StringLength(300)]
        public string? Bio { get; set; }

        // Current time-credit balance (1 credit = 1 hour of teaching)
        public int Credits { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<SkillOffer> SkillOffers { get; set; } = new List<SkillOffer>();
    }
}
