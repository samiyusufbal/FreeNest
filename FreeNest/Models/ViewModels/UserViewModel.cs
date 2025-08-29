using DATA.Models;

namespace FreeNest.Models.ViewModels
{
    public class UserViewModel
    {
        public string? JsonList { get; set; }
        public List<User>? Users { get; set; } = new List<User>();
    }

    public class UserProfileDtoModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string UserPermission { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? Bio { get; set; }
        public IFormFile? AvatarFile { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
    public class UserDtoModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string UserPermission { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
