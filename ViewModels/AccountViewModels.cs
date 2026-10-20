using System.ComponentModel.DataAnnotations;

namespace SkillSwap.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Enter your name")]
        [StringLength(60, MinimumLength = 2)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter your email")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [StringLength(60)]
        public string? Department { get; set; }

        [Required(ErrorMessage = "Choose a password")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Use at least one letter and one number")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm your password")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Enter your email")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter your password")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember my email")]
        public bool RememberEmail { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Enter your name")]
        [StringLength(60, MinimumLength = 2)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(60)]
        public string? Department { get; set; }

        [StringLength(300, ErrorMessage = "Keep your bio under 300 characters")]
        public string? Bio { get; set; }

        public string Email { get; set; } = string.Empty;
    }
}
