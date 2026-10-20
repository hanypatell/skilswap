using Microsoft.AspNetCore.Mvc;
using SkillSwap.Data;
using SkillSwap.Filters;

namespace SkillSwap.ViewComponents
{
    // Navbar user area with the live credit balance
    public class UserMenuViewComponent : ViewComponent
    {
        private readonly AppDbContext _db;

        public UserMenuViewComponent(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = HttpContext.Session.GetUserId();
            var user = userId == null ? null : await _db.Users.FindAsync(userId);
            return View(user);
        }
    }
}
