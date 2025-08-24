using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
namespace FreeNest.Models
{
    public class UserRegisterModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Username can only contain letters, numbers, dots, hyphens and underscores")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match")]
        public string ConfirmPassword { get; set; }

        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Phone Number")]
        public string? GSM { get; set; }

        [Display(Name = "Bio")]
        [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
        public string? Bio { get; set; }

        [Display(Name = "Avatar")]
        public IFormFile? AvatarFile { get; set; }

        [Display(Name = "Theme")]
        public string Theme { get; set; } = "system";

        [Display(Name = "Language")]
        public string Language { get; set; } = "en";

        [Display(Name = "Show Analytics")]
        public bool ShowAnalytics { get; set; } = false;

        [Display(Name = "Custom URL")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? CustomUrl { get; set; }

        // SelectList properties for dropdowns
        public SelectList ThemeOptions { get; set; }
        public SelectList LanguageOptions { get; set; }

        public UserRegisterModel()
        {
            ThemeOptions = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Value = "system", Text = "System" },
                new SelectListItem { Value = "light", Text = "Light" },
                new SelectListItem { Value = "dark", Text = "Dark" }
            }, "Value", "Text");

            LanguageOptions = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Value = "en", Text = "English" },
                new SelectListItem { Value = "tr", Text = "Türkçe" }
            }, "Value", "Text");
        }
    }
}
