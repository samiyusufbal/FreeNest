namespace DATA.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string UserPermission { get; set; } // e.g., "admin", "user"
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        public string PasswordHash { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string Theme { get; set; } = "system";
        public string? CustomUrl { get; set; }
        public string Language { get; set; } = "en";
        public bool ShowAnalytics { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }

        public ICollection<Link> Links { get; set; } = new List<Link>();
    }
}
