using SkillSwap.Models;

namespace SkillSwap.ViewModels
{
    public class HomeViewModel
    {
        public int MemberCount { get; set; }
        public int SkillCount { get; set; }
        public int HoursExchanged { get; set; }
        public List<SkillCard> LatestSkills { get; set; } = new();
        public List<LeaderboardEntry> TopTeachers { get; set; } = new();
    }

    public class LeaderboardEntry
    {
        public int Rank { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Department { get; set; }
        public int HoursTaught { get; set; }
        public int SessionsTaught { get; set; }
        public double? AverageRating { get; set; }
    }
}
