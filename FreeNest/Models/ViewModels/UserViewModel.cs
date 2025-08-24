using Microsoft.AspNetCore.Mvc.Rendering;

namespace FreeNest.Models.ViewModels
{
    public class UserViewModel
    {
        public string JsonList { get; set; }
    }

    public class UserProfileDtoModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string? PasswordHash { get; set; }
        public string? Bio { get; set; }
        public IFormFile? AvatarFile { get; set; }
        public string Theme { get; set; } = "system"; // system, light, dark
        public string? CustomUrl { get; set; }
        public string Language { get; set; } = "en"; // en, tr
        public bool ShowAnalytics { get; set; } = false;
        public SelectList ThemeOptions { get; set; }
        public SelectList LanguageOptions { get; set; }

        public UserProfileDtoModel()
        {
            ThemeOptions = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Value = "system", Text = "System" },
                new SelectListItem { Value = "light", Text = "Light" },
                new SelectListItem { Value = "dark", Text = "Dark" }
            }, "Value", "Text");

            LanguageOptions = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Value = "en-us", Text = "English (en-us)" },
                new SelectListItem { Value = "tr-tr", Text = "Türkçe" }
            }, "Value", "Text");
        }
    }
    public class UserDtoModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string UserPermission { get; set; } // e.g., "admin", "user"
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; } // Indicates if the user's email is verified
        public string PasswordHash { get; set; }
        public string ProfileName { get; set; }
        public string Bio { get; set; }
        public string AvatarUrl { get; set; }
        public int Theme { get; set; } // 0: Light, 1: Dark, 2: System
        public string CustomUrl { get; set; } // e.g., "mycustomprofile"
        public string Language { get; set; } // e.g., "en-US", "fr-FR"
        public bool ShowAnalytics { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
