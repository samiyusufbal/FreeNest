using PhotoSauce.MagicScaler;

namespace FreeNest.Helpers
{
    public static class ImageHelper
    {
        public static bool IsValidImage(string filename)
        {
            var ext = Path.GetExtension(filename).ToLower();
            var validExt = new List<string> { ".jpg", ".jpeg", ".png", ".gif" };

            return validExt.Contains(ext);
        }

        public static bool IsValidFile(string filename)
        {
            var ext = Path.GetExtension(filename).ToLower();
            var validExt = new List<string> { ".pdf", ".jpg", ".jpeg", ".png", ".gif" };

            return validExt.Contains(ext);
        }

        public static string SaveImage(Stream inFileStream, string inFile, string outFolderPath, int width, int height)
        {
            var ext = Path.GetExtension(inFile);

            var fileName = Path.GetFileNameWithoutExtension(inFile).Replace(" ", "");
            if (fileName.Length > 90)
                fileName = fileName.Substring(0, 80);

            var settings = new ProcessImageSettings
            {
                Width = width,
                Height = height,
            };

            if (ext == ".png")
                settings.TrySetEncoderFormat(ImageMimeTypes.Png);
            else
            {
                settings.TrySetEncoderFormat(ImageMimeTypes.Jpeg);
                settings.DpiX = settings.DpiY = 80;
                ext = ".jpg";
            }

            while (File.Exists(Path.Combine(outFolderPath, fileName + ext)))
            {
                var rnd = new Random().Next(1, 99999);
                fileName = $"{fileName}_{rnd}";
            }


            var outPath = Path.Combine(outFolderPath, fileName + ext);

            using (var outStream = new FileStream(outPath, FileMode.Create))
            {
                MagicImageProcessor.ProcessImage(inFileStream, outStream, settings);
            }

            return fileName + ext;
        }

        public static string SaveImage(Stream inFileStream, string inFile, string outFolderPath, int maxSize = 1000)
        {
            var ext = Path.GetExtension(inFile).ToLower();
            var fileName = Path.GetFileNameWithoutExtension(inFile).Replace(" ", "");

            if (fileName.Length > 90)
                fileName = fileName.Substring(0, 80);

            if (ext != ".png" && ext != ".jpg" && ext != ".jpeg")
                ext = ".jpg";

            while (File.Exists(Path.Combine(outFolderPath, fileName + ext)))
            {
                var rnd = new Random().Next(1, 99999);
                fileName = $"{fileName}_{rnd}";
            }

            var outPath = Path.Combine(outFolderPath, fileName + ext);

            using (var outStream = new FileStream(outPath, FileMode.Create))
            {
                var settings = new ProcessImageSettings
                {
                    Width = maxSize,
                    Height = maxSize,
                    ResizeMode = CropScaleMode.Max,
                    HybridMode = HybridScaleMode.Off
                };

                MagicImageProcessor.ProcessImage(inFileStream, outStream, settings);
            }

            return fileName + ext;
        }

        public static string SaveImage(Stream inFileStream, string inFile, string outFolderPath)
        {
            var ext = Path.GetExtension(inFile);
            var fileName = Path.GetFileNameWithoutExtension(inFile).Replace(" ", "");
            if (fileName.Length > 90)
                fileName = fileName.Substring(0, 80);

            var settings = new ProcessImageSettings();

            if (ext == ".png")
                settings.TrySetEncoderFormat(ImageMimeTypes.Png);
            else
            {
                settings.TrySetEncoderFormat(ImageMimeTypes.Jpeg);
                settings.DpiX = settings.DpiY = 80;
                ext = ".jpg";
            }

            while (File.Exists(Path.Combine(outFolderPath, fileName + ext)))
            {
                var rnd = new Random().Next(1, 99999);
                fileName = $"{fileName}_{rnd}";
            }

            var outPath = Path.Combine(outFolderPath, fileName + ext);

            using (var outStream = new FileStream(outPath, FileMode.Create))
            {
                MagicImageProcessor.ProcessImage(inFileStream, outStream, settings);
            }

            return fileName + ext;
        }

        public static string SaveFile(Stream inFileStream, string inFile, string outFolderPath)
        {
            var ext = ".pdf";
            var fileName = Path.GetFileNameWithoutExtension(inFile).Replace(" ", "");
            if (fileName.Length > 90)
                fileName = fileName.Substring(0, 80);




            while (File.Exists(Path.Combine(outFolderPath, fileName + ext)))
            {
                var rnd = new Random().Next(1, 99999);
                fileName = $"{fileName}_{rnd}";
            }


            var outPath = Path.Combine(outFolderPath, fileName + ext);

            using (var outStream = new FileStream(outPath, FileMode.Create))
            {
                inFileStream.CopyTo(outStream);
            }

            return fileName + ext;
        }

        public static bool RemoveFile(string fileName)
        {

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
                return true;
            }

            return true;
        }
    }
}
