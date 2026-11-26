using System;
using System.Drawing;
using System.IO;

namespace SchoolIdReplacement.Services
{
    /// <summary>
    /// Stores student photos in %LOCALAPPDATA%\SchoolIdReplacement\Photos (and resolves relative paths)
    /// so student photos never break when moved between computers.
    /// </summary>
    public static class PhotoStorage
    {
        public static string Folder
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SchoolIdReplacement",
                    "Photos");
            }
        }

        public static string Import(string sourcePath, string studentNo)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            {
                throw new InvalidOperationException("Please choose an existing image file.");
            }

            var extension = Path.GetExtension(sourcePath);
            if (extension == null)
            {
                extension = ".jpg";
            }
            extension = extension.ToLowerInvariant();

            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".bmp")
            {
                throw new InvalidOperationException("Only JPG, PNG or BMP images are supported.");
            }

            Directory.CreateDirectory(Folder);

            var safeNumber = string.IsNullOrWhiteSpace(studentNo) ? "student" : studentNo.Trim();
            foreach (var invalid in Path.GetInvalidFileNameChars())
            {
                safeNumber = safeNumber.Replace(invalid.ToString(), string.Empty);
            }

            var fileName = safeNumber + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + extension;
            var target = Path.Combine(Folder, fileName);

            File.Copy(sourcePath, target, true);
            return fileName;
        }

        public static string ResolvePath(string storedPath)
        {
            if (string.IsNullOrWhiteSpace(storedPath))
            {
                return null;
            }

            if (File.Exists(storedPath))
            {
                return storedPath;
            }

            var fileName = Path.GetFileName(storedPath);
            if (!string.IsNullOrEmpty(fileName))
            {
                var localCandidate = Path.Combine(Folder, fileName);
                if (File.Exists(localCandidate))
                {
                    return localCandidate;
                }

                var roamingCandidate = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "SchoolIdReplacement",
                    "Photos",
                    fileName);
                if (File.Exists(roamingCandidate))
                {
                    return roamingCandidate;
                }
            }

            return null;
        }

        public static Image Load(string path)
        {
            var resolved = ResolvePath(path);
            if (string.IsNullOrWhiteSpace(resolved) || !File.Exists(resolved))
            {
                return null;
            }

            try
            {
                using (var stream = new FileStream(resolved, FileMode.Open, FileAccess.Read))
                using (var image = Image.FromStream(stream))
                {
                    return new Bitmap(image);
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
