namespace FreeNest.Models
{
    public class UserLoginModel
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public bool RememberMe { get; set; }
        public string? ErrorMsg { get; set; }
    }

    public class UserSessionModel
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? PPhoto { get; set; }
    }
}
