using System.Text.Json;

namespace FreeNest.Helpers
{
    public class IconHelper
    {
        private readonly IWebHostEnvironment _env;
        private readonly List<string> _icons;

        public IconHelper(IWebHostEnvironment env)
        {
            _env = env;
            _icons = LoadIcons();
        }

        public List<string> GetIcons() => _icons;

        private List<string> LoadIcons()
        {
            var path = Path.Combine(_env.WebRootPath, "json", "fontawesome-icons.json");
            if (!File.Exists(path)) return new List<string>();

            var json = File.ReadAllText(path);
            var iconsData = JsonSerializer.Deserialize<Dictionary<string, IconData>>(json);
            if (iconsData == null) return new List<string>();

            // JSON'daki free listesindeki tüm stilleri alıyoruz
            return iconsData
                .Select(kvp =>
                {
                    var key = kvp.Key;
                    var style = kvp.Value.free.FirstOrDefault() ?? "solid"; // default solid
                    return $"{GetPrefix(style)} fa-{key}";
                })
                .OrderBy(i => i)
                .ToList();
        }

        private static string GetPrefix(string style) => style switch
        {
            "brands" => "fab",
            "solid" => "fas",
            "regular" => "far",
            _ => "fas"
        };

        private class IconData
        {
            public List<string> free { get; set; }
        }
    }
}
