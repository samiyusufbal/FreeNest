using System.Text.Json;

namespace FreeNest.Helpers
{
    public class IconHelper
    {
        private readonly IWebHostEnvironment _env;
        private List<string> _icons;

        public IconHelper(IWebHostEnvironment env)
        {
            _env = env;
            _icons = LoadIcons();
        }

        public List<string> GetIcons()
        {
            return _icons;
        }

        private List<string> LoadIcons()
        {
            var path = Path.Combine(_env.WebRootPath, "json", "fontawesome-icons.json");
            var json = System.IO.File.ReadAllText(path);
            var iconsData = JsonSerializer.Deserialize<Dictionary<string, IconData>>(json);

            return iconsData?
                .Where(x => x.Value.free.Contains("brands"))
                .Select(x => $"fab fa-{x.Key}")
                .OrderBy(x => x)
                .ToList() ?? new List<string>();
        }

        private class IconData
        {
            public List<string> free { get; set; }
        }
    }
}