using SkillSwap.Models;

namespace SkillSwap.ViewModels
{
    public class SkillBrowseViewModel
    {
        public List<SkillCard> Skills { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
    }

    public class SkillCard
    {
        public SkillOffer Offer { get; set; } = null!;
        public double? AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }

    public class SkillDetailsViewModel
    {
        public SkillOffer Offer { get; set; } = null!;
        public double? AverageRating { get; set; }
        public List<Review> Reviews { get; set; } = new();
        public bool IsOwner { get; set; }
        public bool IsLoggedIn { get; set; }
        public int ViewerCredits { get; set; }
    }
}
