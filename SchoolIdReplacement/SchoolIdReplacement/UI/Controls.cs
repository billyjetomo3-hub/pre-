using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace SchoolIdReplacement.UI
{
    internal static class Draw
    {
        public static GraphicsPath RoundRect(Rectangle rectangle, int radius)
        {
            var path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(rectangle);
                return path;
            }

            var diameter = Math.Min(Math.Min(radius * 2, rectangle.Width), rectangle.Height);
            path.AddArc(rectangle.X, rectangle.Y, diameter, diameter, 180f, 90f);
            path.AddArc(rectangle.Right - diameter, rectangle.Y, diameter, diameter, 270f, 90f);
            path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0f, 90f);
            path.AddArc(rectangle.X, rectangle.Bottom - diameter, diameter, diameter, 90f, 90f);
            path.CloseFigure();
            return path;
        }
    }

    internal class RoundedButton : Button
    {
        private bool _hover;
        private bool _pressed;

        public RoundedButton()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);

            CornerRadius = 8;
            FillColor = Theme.Accent;
            HoverColor = Theme.AccentHover;
            PressedColor = Theme.AccentHover;
            BorderColor = Color.Empty;
            BorderWidth = 0;
            ForeColor = Color.White;
            Font = Theme.Button;
            Cursor = Cursors.Hand;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false;
            Size = new Size(140, 40);
        }

        public int CornerRadius { get; set; }

        public Color FillColor { get; set; }

        public Color HoverColor { get; set; }

        public Color PressedColor { get; set; }

        public Color BorderColor { get; set; }

        public int BorderWidth { get; set; }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            _pressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            _pressed = true;
            Invalidate();
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            _pressed = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var graphics = pevent.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            var parentColor = Parent != null ? Parent.BackColor : Theme.Background;
            using (var background = new SolidBrush(parentColor))
            {
                graphics.FillRectangle(background, ClientRectangle);
            }

            var color = !Enabled
                ? Color.FromArgb(203, 213, 225)
                : (_pressed ? PressedColor : (_hover ? HoverColor : FillColor));

            var rectangle = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = Draw.RoundRect(rectangle, CornerRadius))
            using (var fill = new SolidBrush(color))
            {
                graphics.FillPath(fill, path);

                if (BorderWidth > 0 && BorderColor != Color.Empty && BorderColor != Color.Transparent)
                {
                    using (var pen = new Pen(BorderColor, BorderWidth))
                    {
                        graphics.DrawPath(pen, path);
                    }
                }
            }

            TextRenderer.DrawText(
                graphics,
                Text,
                Font,
                rectangle,
                Enabled ? ForeColor : Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }

    internal class NavButton : Button
    {
        private bool _hover;
        private bool _active;

        public NavButton()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);

            Font = new Font("Segoe UI", 10f);
            ForeColor = Theme.SidebarText;
            Cursor = Cursors.Hand;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            TextAlign = ContentAlignment.MiddleLeft;
            Size = new Size(240, 44);
        }

        public bool Active
        {
            get { return _active; }
            set
            {
                _active = value;
                Invalidate();
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var graphics = pevent.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            using (var background = new SolidBrush(Theme.Sidebar))
            {
                graphics.FillRectangle(background, ClientRectangle);
            }

            if (_active || _hover)
            {
                var rectangle = new Rectangle(12, 3, Width - 24, Height - 7);
                using (var path = Draw.RoundRect(rectangle, 8))
                using (var fill = new SolidBrush(_active ? Theme.SidebarHover : Color.FromArgb(24, 32, 48)))
                {
                    graphics.FillPath(fill, path);
                }
            }

            if (_active)
            {
                using (var accent = new SolidBrush(Theme.Accent))
                {
                    graphics.FillRectangle(accent, 0, 12, 4, Height - 24);
                }
            }

            TextRenderer.DrawText(
                graphics,
                Text,
                Font,
                new Rectangle(24, 0, Width - 36, Height),
                _active ? Color.White : Theme.SidebarText,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }

    internal class CardPanel : Panel
    {
        public CardPanel()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);

            CornerRadius = 14;
            BackColor = Theme.Surface;
            Padding = new Padding(20);
            ShowBorder = true;
        }

        public int CornerRadius { get; set; }

        public bool ShowBorder { get; set; }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Painted here (not in OnPaint) so transparent child labels pick up the card surface.
            var graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (var background = new SolidBrush(Parent != null ? Parent.BackColor : Theme.Background))
            {
                graphics.FillRectangle(background, ClientRectangle);
            }

            var rectangle = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = Draw.RoundRect(rectangle, CornerRadius))
            using (var fill = new SolidBrush(BackColor))
            {
                graphics.FillPath(fill, path);

                if (ShowBorder)
                {
                    using (var pen = new Pen(Theme.Border, 1f))
                    {
                        graphics.DrawPath(pen, path);
                    }
                }
            }
        }
    }

    internal class Avatar : Control
    {
        public Avatar()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);

            Size = new Size(42, 42);
            FillColor = Theme.Accent;
            TextColor = Color.White;
        }

        public string Initials { get; set; }

        public Color FillColor { get; set; }

        public Color TextColor { get; set; }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var graphics = pevent.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            using (var background = new SolidBrush(Parent != null ? Parent.BackColor : Theme.Sidebar))
            {
                graphics.FillRectangle(background, ClientRectangle);
            }

            using (var fill = new SolidBrush(FillColor))
            {
                graphics.FillEllipse(fill, 0, 0, Width - 1, Height - 1);
            }

            var text = string.IsNullOrEmpty(Initials) ? "?" : Initials;
            using (var font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold))
            {
                TextRenderer.DrawText(
                    graphics,
                    text,
                    font,
                    new Rectangle(0, 0, Width, Height),
                    TextColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }
    }
}
