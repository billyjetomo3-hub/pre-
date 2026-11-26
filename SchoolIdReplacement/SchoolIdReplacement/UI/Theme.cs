using System.Drawing;
using SchoolIdReplacement.Models;

namespace SchoolIdReplacement.UI
{
    internal static class Theme
    {
        public static readonly Color Background = Color.FromArgb(243, 245, 249);
        public static readonly Color Surface = Color.White;
        public static readonly Color Sidebar = Color.FromArgb(15, 23, 42);
        public static readonly Color SidebarHover = Color.FromArgb(30, 41, 59);
        public static readonly Color SidebarText = Color.FromArgb(148, 163, 184);

        public static readonly Color Accent = Color.FromArgb(79, 70, 229);
        public static readonly Color AccentHover = Color.FromArgb(67, 56, 202);
        public static readonly Color AccentSoft = Color.FromArgb(238, 242, 255);

        public static readonly Color Text = Color.FromArgb(30, 41, 59);
        public static readonly Color Muted = Color.FromArgb(100, 116, 139);
        public static readonly Color Border = Color.FromArgb(226, 232, 240);
        public static readonly Color Field = Color.FromArgb(248, 250, 252);

        public static readonly Color Success = Color.FromArgb(21, 128, 61);
        public static readonly Color SuccessSoft = Color.FromArgb(240, 253, 244);
        public static readonly Color Warning = Color.FromArgb(180, 83, 9);
        public static readonly Color WarningSoft = Color.FromArgb(255, 251, 235);
        public static readonly Color Danger = Color.FromArgb(185, 28, 28);
        public static readonly Color DangerSoft = Color.FromArgb(254, 242, 242);
        public static readonly Color Info = Color.FromArgb(3, 105, 161);
        public static readonly Color InfoSoft = Color.FromArgb(240, 249, 255);

        public static readonly Font H1 = new Font("Segoe UI Semibold", 18f, FontStyle.Bold);
        public static readonly Font H2 = new Font("Segoe UI Semibold", 14f, FontStyle.Bold);
        public static readonly Font H3 = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold);
        public static readonly Font Body = new Font("Segoe UI", 9.5f);
        public static readonly Font BodyStrong = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
        public static readonly Font Small = new Font("Segoe UI", 8.5f);
        public static readonly Font Tiny = new Font("Segoe UI", 8f);
        public static readonly Font Kpi = new Font("Segoe UI Semibold", 26f, FontStyle.Bold);
        public static readonly Font KpiSmall = new Font("Segoe UI Semibold", 18f, FontStyle.Bold);
        public static readonly Font Button = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
        public static readonly Font Input = new Font("Segoe UI", 10.5f);
        public static readonly Font Caption = new Font("Segoe UI Semibold", 8f, FontStyle.Bold);
        public static readonly Font Micro = new Font("Segoe UI Semibold", 7.75f, FontStyle.Bold);

        public static Color StatusColor(string status)
        {
            switch (status)
            {
                case RequestStatus.Pending:
                    return Warning;
                case RequestStatus.Approved:
                    return Info;
                case RequestStatus.ReadyForPickup:
                    return Accent;
                case RequestStatus.Released:
                    return Success;
                case RequestStatus.Rejected:
                    return Danger;
                case RequestStatus.Cancelled:
                    return Muted;
                default:
                    return Muted;
            }
        }

        public static Color StatusBackground(string status)
        {
            switch (status)
            {
                case RequestStatus.Pending:
                    return WarningSoft;
                case RequestStatus.Approved:
                    return InfoSoft;
                case RequestStatus.ReadyForPickup:
                    return AccentSoft;
                case RequestStatus.Released:
                    return SuccessSoft;
                case RequestStatus.Rejected:
                    return DangerSoft;
                case RequestStatus.Cancelled:
                    return Field;
                default:
                    return Field;
            }
        }
    }
}
