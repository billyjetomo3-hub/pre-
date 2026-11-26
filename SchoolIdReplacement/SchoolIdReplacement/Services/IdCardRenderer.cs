using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using SchoolIdReplacement.Models;

namespace SchoolIdReplacement.Services
{
    /// <summary>
    /// Draws the student ID card. The same routine is used for the on-screen preview and for printing.
    /// </summary>
    public static class IdCardRenderer
    {
        public const float CardWidthInches = 3.375f;
        public const float CardHeightInches = 2.125f;

        // Design surface in pixels; everything is scaled to the target rectangle.
        private const float DesignWidth = 675f;
        private const float DesignHeight = 425f;

        public static Bitmap Render(Student student, Image photo)
        {
            var width = (int)Math.Round(DesignWidth);
            var height = (int)Math.Round(DesignHeight);
            var bitmap = new Bitmap(width * 2, height * 2);
            bitmap.SetResolution(192f, 192f);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.ScaleTransform(2f, 2f);
                Draw(graphics, new RectangleF(0f, 0f, DesignWidth, DesignHeight), student, photo, true);
            }

            return bitmap;
        }

        public static void Draw(Graphics graphics, RectangleF bounds, Student student, Image photo, bool roundedCorners)
        {
            var state = graphics.Save();
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            graphics.TranslateTransform(bounds.X, bounds.Y);
            graphics.ScaleTransform(bounds.Width / DesignWidth, bounds.Height / DesignHeight);

            var card = new RectangleF(0f, 0f, DesignWidth, DesignHeight);
            using (var cardPath = RoundRect(card, roundedCorners ? 26f : 0f))
            using (var white = new SolidBrush(Color.White))
            {
                graphics.FillPath(white, cardPath);
            }

            // Header band
            using (var header = new SolidBrush(Color.FromArgb(15, 23, 42)))
            {
                graphics.FillRectangle(header, 0f, 0f, DesignWidth, 84f);
            }
            using (var accent = new SolidBrush(Color.FromArgb(79, 70, 229)))
            {
                graphics.FillRectangle(accent, 0f, 0f, 12f, 84f);
            }

            using (var schoolFont = new Font("Segoe UI Semibold", 21f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var bannerFont = new Font("Segoe UI", 12f, FontStyle.Regular, GraphicsUnit.Pixel))
            using (var lightBrush = new SolidBrush(Color.White))
            using (var mutedLight = new SolidBrush(Color.FromArgb(148, 163, 184)))
            {
                graphics.DrawString(SchoolProfile.Name, schoolFont, lightBrush, 32f, 16f);
                graphics.DrawString("OFFICIAL STUDENT IDENTIFICATION CARD", bannerFont, mutedLight, 34f, 48f);
            }

            // Photo frame
            var photoFrame = new RectangleF(30f, 108f, 168f, 196f);
            using (var framePath = RoundRect(photoFrame, 14f))
            using (var frameBrush = new SolidBrush(Color.FromArgb(241, 245, 249)))
            {
                graphics.FillPath(frameBrush, framePath);
                graphics.SetClip(framePath);

                if (photo != null)
                {
                    DrawImageCover(graphics, photo, photoFrame);
                }
                else if (student != null)
                {
                    using (var placeholderFont = new Font("Segoe UI Semibold", 62f, FontStyle.Bold, GraphicsUnit.Pixel))
                    using (var placeholderBrush = new SolidBrush(Color.FromArgb(148, 163, 184)))
                    {
                        var initials = student.Initials;
                        var size = graphics.MeasureString(initials, placeholderFont);
                        graphics.DrawString(initials, placeholderFont, placeholderBrush,
                            photoFrame.X + (photoFrame.Width - size.Width) / 2f,
                            photoFrame.Y + (photoFrame.Height - size.Height) / 2f);
                    }
                }

                graphics.ResetClip();
                using (var pen = new Pen(Color.FromArgb(226, 232, 240), 2f))
                {
                    graphics.DrawPath(pen, framePath);
                }
            }

            // Details
            var name = student == null ? "Student Name" : student.FullName;
            var number = student == null ? "0000-0000" : student.StudentNo;
            var course = student == null ? "Course" : student.Course;
            var yearSection = student == null ? "Year and Section" : (student.YearLevel ?? string.Empty) + "  " + (student.Section ?? string.Empty);

            var left = 224f;

            using (var labelFont = new Font("Segoe UI", 11f, FontStyle.Regular, GraphicsUnit.Pixel))
            using (var nameFont = new Font("Segoe UI Semibold", 25f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var numberFont = new Font("Consolas", 19f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var bodyFont = new Font("Segoe UI", 14f, FontStyle.Regular, GraphicsUnit.Pixel))
            using (var labelBrush = new SolidBrush(Color.FromArgb(100, 116, 139)))
            using (var textBrush = new SolidBrush(Color.FromArgb(15, 23, 42)))
            using (var accentBrush = new SolidBrush(Color.FromArgb(79, 70, 229)))
            {
                graphics.DrawString("STUDENT NAME", labelFont, labelBrush, left, 108f);
                graphics.DrawString(Trim(name, 26), nameFont, textBrush, left - 2f, 122f);

                graphics.DrawString("STUDENT NUMBER", labelFont, labelBrush, left, 166f);
                graphics.DrawString(number, numberFont, accentBrush, left, 180f);

                graphics.DrawString("PROGRAM", labelFont, labelBrush, left, 220f);
                graphics.DrawString(Trim(course, 34), bodyFont, textBrush, left, 234f);

                graphics.DrawString("YEAR / SECTION", labelFont, labelBrush, left, 264f);
                graphics.DrawString(Trim(yearSection.Replace("  ", " - "), 34), bodyFont, textBrush, left, 278f);
            }

            // Footer
            using (var footer = new SolidBrush(Color.FromArgb(248, 250, 252)))
            {
                graphics.FillRectangle(footer, 0f, 330f, DesignWidth, 95f);
            }
            using (var borderPen = new Pen(Color.FromArgb(226, 232, 240), 2f))
            {
                graphics.DrawLine(borderPen, 0f, 330f, DesignWidth, 330f);
            }
            using (var signaturePen = new Pen(Color.FromArgb(148, 163, 184), 2f))
            {
                graphics.DrawLine(signaturePen, 400f, 392f, 620f, 392f);
            }
            using (var footerFont = new Font("Segoe UI", 12f, FontStyle.Regular, GraphicsUnit.Pixel))
            using (var footerBold = new Font("Segoe UI Semibold", 12f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var footerBrush = new SolidBrush(Color.FromArgb(100, 116, 139)))
            using (var darkBrush = new SolidBrush(Color.FromArgb(30, 41, 59)))
            {
                graphics.DrawString(SchoolProfile.CardFooter, footerFont, footerBrush, 30f, 350f);
                graphics.DrawString(SchoolProfile.Address, footerFont, footerBrush, 30f, 370f);
                graphics.DrawString("Registrar", footerBold, darkBrush, 512f, 396f);
            }

            if (roundedCorners)
            {
                using (var cardPath = RoundRect(new RectangleF(1f, 1f, DesignWidth - 2f, DesignHeight - 2f), 26f))
                using (var border = new Pen(Color.FromArgb(203, 213, 225), 2f))
                {
                    graphics.DrawPath(border, cardPath);
                }
            }

            graphics.Restore(state);
        }

        private static void DrawImageCover(Graphics graphics, Image image, RectangleF target)
        {
            var scale = Math.Max(target.Width / image.Width, target.Height / image.Height);
            var width = image.Width * scale;
            var height = image.Height * scale;
            var x = target.X + (target.Width - width) / 2f;
            var y = target.Y + (target.Height - height) / 2f;
            graphics.DrawImage(image, x, y, width, height);
        }

        private static string Trim(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Length <= maxLength ? value : value.Substring(0, maxLength - 1) + "...";
        }

        private static GraphicsPath RoundRect(RectangleF rectangle, float radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0f)
            {
                path.AddRectangle(rectangle);
                return path;
            }

            var diameter = radius * 2f;
            path.AddArc(rectangle.X, rectangle.Y, diameter, diameter, 180f, 90f);
            path.AddArc(rectangle.Right - diameter, rectangle.Y, diameter, diameter, 270f, 90f);
            path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0f, 90f);
            path.AddArc(rectangle.X, rectangle.Bottom - diameter, diameter, diameter, 90f, 90f);
            path.CloseFigure();
            return path;
        }
    }
}
