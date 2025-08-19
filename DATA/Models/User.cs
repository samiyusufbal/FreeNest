namespace DATA.Models
{
    public class User
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
        public string ShowAnalytics { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<Link> Links { get; set; }
    }
}
