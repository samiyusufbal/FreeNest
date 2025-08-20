namespace FreeNest.Models.ViewModels
{
    public class LinkClickViewModel
    {
        public string JsonList { get; set; }
    }

    public class LinkClickDtoModel
    {
        public int Id { get; set; }
        public int LinkId { get; set; }
        public DateTime ClickedAt { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
    }
}
