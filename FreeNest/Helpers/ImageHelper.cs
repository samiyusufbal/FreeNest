using PhotoSauce.MagicScaler;

namespace FreeNest.Helpers
{
    public static class ImageHelper
    {
        private static readonly HashSet<string> _validImageExts = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif" };
        private static readonly HashSet<string> _validFileExts = new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".jpg", ".jpeg", ".png", ".gif" };

        public static bool IsValidImage(string filename) =>
            _validImageExts.Contains(Path.GetExtension(filename));

        public static bool IsValidFile(string filename) =>
            _validFileExts.Contains(Path.GetExtension(filename));

        public static string SaveImage(Stream inStream, string inFile, string outFolder, int width, int height) =>
            SaveInternal(inStream, inFile, outFolder, width, height, CropScaleMode.Crop);

        public static string SaveImage(Stream inStream, string inFile, string outFolder, int maxSize = 1000) =>
            SaveInternal(inStream, inFile, outFolder, maxSize, maxSize, CropScaleMode.Max);

        public static string SaveImage(Stream inStream, string inFile, string outFolder) =>
            SaveInternal(inStream, inFile, outFolder, null, null);

        public static string SaveFile(Stream inStream, string inFile, string outFolder)
        {
            var fileName = SanitizeFileName(inFile);
            const string ext = ".pdf";
            var outPath = GetUniqueFilePath(outFolder, fileName, ext);

            using var outStream = new FileStream(outPath, FileMode.Create);
            inStream.CopyTo(outStream);

            return Path.GetFileName(outPath);
        }

        public static bool RemoveFile(string filePath)
        {
            if (!File.Exists(filePath)) return false;

            File.Delete(filePath);
            return true;
        }

        // 🔽 Private Helpers
        private static string SaveInternal(Stream inStream, string inFile, string outFolder, int? width, int? height, CropScaleMode? mode = null)
        {
            var ext = Path.GetExtension(inFile).ToLower();
            var fileName = SanitizeFileName(inFile);

            if (!_validImageExts.Contains(ext))
                ext = ".jpg"; // fallback

            var outPath = GetUniqueFilePath(outFolder, fileName, ext);

            var settings = new ProcessImageSettings
            {
                Width = width ?? 0,
                Height = height ?? 0,
                ResizeMode = mode ?? CropScaleMode.Max,
                HybridMode = HybridScaleMode.Off
            };

            if (ext == ".png")
                settings.TrySetEncoderFormat(ImageMimeTypes.Png);
            else
            {
                settings.TrySetEncoderFormat(ImageMimeTypes.Jpeg);
                settings.DpiX = settings.DpiY = 80;
            }

            using var outStream = new FileStream(outPath, FileMode.Create);
            MagicImageProcessor.ProcessImage(inStream, outStream, settings);

            return Path.GetFileName(outPath);
        }


        private static string SanitizeFileName(string inFile)
        {
            var fileName = Path.GetFileNameWithoutExtension(inFile).Replace(" ", "");
            return fileName.Length > 90 ? fileName[..80] : fileName;
        }

        private static string GetUniqueFilePath(string folder, string fileName, string ext)
        {
            var outPath = Path.Combine(folder, fileName + ext);

            var rnd = new Random();
            while (File.Exists(outPath))
            {
                fileName = $"{fileName}_{rnd.Next(1, 99999)}";
                outPath = Path.Combine(folder, fileName + ext);
            }

            return outPath;
        }
    }
}