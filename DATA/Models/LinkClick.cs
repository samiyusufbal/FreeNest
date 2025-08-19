namespace DATA.Models
{
    public class LinkClick
    {
        public int Id { get; set; }
        public int LinkId { get; set; }
        public DateTime ClickedAt { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }

        public Link Link { get; set; }
    }
}
