using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Data;
using SkillSwap.Filters;
using SkillSwap.Models;
using SkillSwap.Services;
using SkillSwap.ViewModels;

namespace SkillSwap.Controllers
{
    public class AccountController : Controller
    {
        private const string RememberEmailCookie = "SkillSwap.RememberEmail";

        private readonly AppDbContext _db;
        private readonly IPasswordHasher<User> _hasher;
        private readonly CreditService _credits;

        public AccountController(AppDbContext db, IPasswordHasher<User> hasher, CreditService credits)
        {
            _db = db;
            _hasher = hasher;
            _credits = credits;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetUserId() != null) return RedirectToAction("Index", "Home");
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var email = model.Email.Trim().ToLower();
            if (ModelState.IsValid && await _db.Users.AnyAsync(u => u.Email == email))
                ModelState.AddModelError(nameof(model.Email), "An account with this email already exists");

            if (!ModelState.IsValid) return View(model);

            var user = new User
            {
                FullName = model.FullName.Trim(),
                Email = email,
                Department = model.Department?.Trim()
            };
            user.PasswordHash = _hasher.HashPassword(user, model.Password);
            _db.Users.Add(user);
            _credits.AddTransaction(user, CreditService.SignupBonus, TransactionType.SignupBonus, "Welcome bonus");
            await _db.SaveChangesAsync();

            SignIn(user);
            TempData["Success"] = $"Welcome to SkillSwap, {user.FullName}! You got {CreditService.SignupBonus} free credits to start learning.";
            return RedirectToAction("Index", "Skills");
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (HttpContext.Session.GetUserId() != null) return RedirectToAction("Index", "Home");

            // Cookie: prefill the email if the user asked us to remember it
            var remembered = Request.Cookies[RememberEmailCookie];
            return View(new LoginViewModel
            {
                Email = remembered ?? string.Empty,
                RememberEmail = remembered != null,
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var email = model.Email.Trim().ToLower();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || _hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Email or password is incorrect");
                return View(model);
            }

            if (model.RememberEmail)
                Response.Cookies.Append(RememberEmailCookie, email, new CookieOptions { Expires = DateTimeOffset.Now.AddDays(30), HttpOnly = true, IsEssential = true });
            else
                Response.Cookies.Delete(RememberEmailCookie);

            SignIn(user);
            TempData["Success"] = $"Welcome back, {user.FullName.Split(' ')[0]}!";

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "You have been logged out.";
            return RedirectToAction("Index", "Home");
        }

        [RequireLogin]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _db.Users.FindAsync(HttpContext.Session.GetUserId());
            if (user == null) return RedirectToAction(nameof(Login));

            return View(new ProfileViewModel
            {
                FullName = user.FullName,
                Department = user.Department,
                Bio = user.Bio,
                Email = user.Email
            });
        }

        [RequireLogin]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _db.Users.FindAsync(HttpContext.Session.GetUserId());
            if (user == null) return RedirectToAction(nameof(Login));

            model.Email = user.Email;
            if (!ModelState.IsValid) return View(model);

            user.FullName = model.FullName.Trim();
            user.Department = model.Department?.Trim();
            user.Bio = model.Bio?.Trim();
            await _db.SaveChangesAsync();

            HttpContext.Session.SetString(SessionKeys.UserName, user.FullName);
            TempData["Success"] = "Profile updated.";
            return RedirectToAction(nameof(Profile));
        }

        private void SignIn(User user)
        {
            HttpContext.Session.SetInt32(SessionKeys.UserId, user.Id);
            HttpContext.Session.SetString(SessionKeys.UserName, user.FullName);
        }
    }
}
