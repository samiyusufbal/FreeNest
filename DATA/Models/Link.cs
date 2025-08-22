namespace DATA.Models
{
    public class Link
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; }

        public User User { get; set; }
        public ICollection<LinkClick> LinkClicks { get; set; }
    }
}
