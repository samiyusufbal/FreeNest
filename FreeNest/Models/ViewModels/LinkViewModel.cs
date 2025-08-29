using DATA.Models;

namespace FreeNest.Models.ViewModels
{
    public class LinkViewModel
    {
        public string JsonList { get; set; }
        public User? User { get; set; }
        public List<Link>? Links { get; set; }
    }
    public class LinkDtoModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }
        public bool IsSocial { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public User? User { get; set; }
    }
}
