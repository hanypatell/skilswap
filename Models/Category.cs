using System.ComponentModel.DataAnnotations;

namespace SkillSwap.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(40)]
        public string Name { get; set; } = string.Empty;

        [StringLength(10)]
        public string Icon { get; set; } = string.Empty;

        public ICollection<SkillOffer> SkillOffers { get; set; } = new List<SkillOffer>();
    }
}
