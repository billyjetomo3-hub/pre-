using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SchoolIdReplacement.Models;
using SchoolIdReplacement.UI;
using SchoolIdReplacement.Views;

namespace SchoolIdReplacement.Forms
{
    public class MainForm : Form
    {
        private readonly Session _session;
        private readonly List<NavButton> _navButtons = new List<NavButton>();

        private Panel _contentHost;
        private Panel _navHost;
        private Label _pageTitle;
        private Label _pageSubtitle;

        public MainForm(Session session)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            _session = session;
            InitializeComponent();
            BuildNavigation();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            Text = "School ID Replacement System";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1180, 720);
            ClientSize = new Size(1320, 800);
            BackColor = Theme.Background;
            Font = Theme.Body;

            // ------------------------------------------------------------ Sidebar
            var sidebar = new TableLayoutPanel
            {
                Dock = DockStyle.Left,
                Width = 252,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Theme.Sidebar,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            sidebar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 98f));
            sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
            sidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 132f));

            // Brand
            var brand = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Sidebar, Margin = new Padding(0) };
            brand.Controls.Add(new Avatar
            {
                Location = new Point(24, 26),
                Size = new Size(46, 46),
                Initials = "ID",
                FillColor = Theme.Accent,
                TextColor = Color.White
            });

            var brandTitle = Ui.TextBlock("ID Replacement", new Font("Segoe UI Semibold", 12f, FontStyle.Bold), Color.White);
            brandTitle.Location = new Point(82, 28);
            brand.Controls.Add(brandTitle);

            var brandSubtitle = Ui.TextBlock("Student Services Office", Theme.Tiny, Theme.SidebarText);
            brandSubtitle.Location = new Point(83, 52);
            brand.Controls.Add(brandSubtitle);

            sidebar.Controls.Add(brand, 0, 0);

            // Menu caption
            var menuCaption = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Sidebar, Margin = new Padding(0) };
            var menuLabel = Ui.TextBlock("MAIN MENU", new Font("Segoe UI Semibold", 7.5f, FontStyle.Bold), Color.FromArgb(100, 116, 139));
            menuLabel.Location = new Point(28, 8);
            menuCaption.Controls.Add(menuLabel);

            sidebar.Controls.Add(menuCaption, 0, 1);

            // Navigation
            _navHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Sidebar,
                AutoScroll = true,
                Margin = new Padding(0)
            };
            sidebar.Controls.Add(_navHost, 0, 2);

            // Signed in user
            var footer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(20, 28, 44),
                Margin = new Padding(0)
            };

            footer.Controls.Add(new Avatar
            {
                Location = new Point(24, 18),
                Size = new Size(42, 42),
                Initials = _session.Initials,
                FillColor = Color.FromArgb(51, 65, 85),
                TextColor = Color.White
            });

            var nameLabel = Ui.Line(_session.FullName, Theme.BodyStrong, Color.White);
            nameLabel.Location = new Point(78, 20);
            nameLabel.Size = new Size(158, 20);
            footer.Controls.Add(nameLabel);

            var roleLabel = Ui.Line(
                _session.Role + (string.IsNullOrEmpty(_session.StudentNo) ? string.Empty : " - " + _session.StudentNo),
                Theme.Tiny,
                Theme.SidebarText);
            roleLabel.Location = new Point(79, 40);
            roleLabel.Size = new Size(158, 18);
            footer.Controls.Add(roleLabel);

            var signOut = new RoundedButton
            {
                Text = "Sign out",
                Location = new Point(24, 74),
                Size = new Size(204, 38),
                FillColor = Color.FromArgb(30, 41, 59),
                HoverColor = Color.FromArgb(51, 65, 85),
                PressedColor = Color.FromArgb(71, 85, 105),
                ForeColor = Color.White,
                CornerRadius = 8,
                Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold)
            };
            signOut.Click += SignOutClicked;
            footer.Controls.Add(signOut);

            sidebar.Controls.Add(footer, 0, 3);

            // --------------------------------------------------------------- Main
            var main = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background, Margin = new Padding(0) };

            _contentHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Background,
                Padding = new Padding(30, 0, 30, 26),
                Margin = new Padding(0)
            };

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 96,
                BackColor = Theme.Background,
                Margin = new Padding(0)
            };

            _pageTitle = Ui.TextBlock("Dashboard", Theme.H2, Theme.Text);
            _pageTitle.Location = new Point(30, 26);
            header.Controls.Add(_pageTitle);

            _pageSubtitle = Ui.TextBlock(string.Empty, Theme.Small, Theme.Muted);
            _pageSubtitle.Location = new Point(31, 54);
            header.Controls.Add(_pageSubtitle);

            var today = new Label
            {
                Text = DateTime.Now.ToString("dddd, MMMM d, yyyy"),
                Font = Theme.Small,
                ForeColor = Theme.Muted,
                AutoSize = false,
                Width = 300,
                Dock = DockStyle.Right,
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 0, 30, 0)
            };
            header.Controls.Add(today);

            main.Controls.Add(_contentHost);
            main.Controls.Add(header);

            Controls.Add(main);
            Controls.Add(sidebar);

            ResumeLayout(false);
        }

        private void BuildNavigation()
        {
            var items = new List<NavItem>();

            items.Add(new NavItem(
                "Dashboard",
                "Dashboard",
                "Overview of ID replacement activity",
                delegate { return new DashboardView(_session); }));

            if (_session.IsAdministrator)
            {
                items.Add(new NavItem(
                    "Requests",
                    "ID Requests",
                    "Review, approve and release replacement requests",
                    delegate { return new RequestsView(_session); }));

                items.Add(new NavItem(
                    "Students",
                    "Students",
                    "Student records, accounts and ID card previews",
                    delegate { return new StudentsView(_session); }));

                items.Add(new NavItem(
                    "Reports",
                    "Reports",
                    "Replacement statistics and payment summaries",
                    delegate { return new ReportsView(); }));
            }
            else
            {
                items.Add(new NavItem(
                    "NewRequest",
                    "New Request",
                    "File a request for a replacement student ID",
                    delegate { return new NewRequestView(_session); }));

                items.Add(new NavItem(
                    "MyRequests",
                    "My Requests",
                    "Track the status of your replacement requests",
                    delegate { return new MyRequestsView(_session); }));
            }

            items.Add(new NavItem(
                "Settings",
                "Settings",
                "Account password and system information",
                delegate { return new SettingsView(_session); }));

            _navHost.SuspendLayout();
            _navButtons.Clear();

            var width = _navHost.ClientSize.Width > 0 ? _navHost.ClientSize.Width : 252;

            for (var index = 0; index < items.Count; index++)
            {
                var item = items[index];

                var button = new NavButton
                {
                    Text = item.Title,
                    Location = new Point(0, 4 + index * 50),
                    Size = new Size(width, 44)
                };

                var captured = item;
                button.Click += delegate { Navigate(captured, button); };

                _navHost.Controls.Add(button);
                _navButtons.Add(button);
            }

            _navHost.Resize += delegate
            {
                var available = _navHost.ClientSize.Width;

                if (available <= 0)
                {
                    return;
                }

                foreach (var navButton in _navButtons)
                {
                    navButton.Width = available;
                }
            };

            _navHost.ResumeLayout();

            if (_navButtons.Count > 0)
            {
                Navigate(items[0], _navButtons[0]);
            }
        }

        private void Navigate(NavItem item, NavButton button)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                foreach (var navButton in _navButtons)
                {
                    navButton.Active = navButton == button;
                }

                _pageTitle.Text = item.Title;
                _pageSubtitle.Text = item.Subtitle;

                var existing = _contentHost.Controls.Count > 0 ? _contentHost.Controls[0] : null;

                _contentHost.SuspendLayout();
                _contentHost.Controls.Clear();

                var view = item.Factory();
                view.Dock = DockStyle.Fill;
                _contentHost.Controls.Add(view);

                if (existing != null)
                {
                    existing.Dispose();
                }

                _contentHost.ResumeLayout();
            }
            catch (Exception ex)
            {
                Ui.ShowError(this, "The screen could not be loaded.\r\n\r\n" + ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void SignOutClicked(object sender, EventArgs e)
        {
            if (!Ui.Confirm(this, "Sign out of the system?"))
            {
                return;
            }

            Session.SignOutRequested = true;
            Close();
        }

        private class NavItem
        {
            public NavItem(string key, string title, string subtitle, Func<Control> factory)
            {
                Key = key;
                Title = title;
                Subtitle = subtitle;
                Factory = factory;
            }

            public string Key { get; private set; }

            public string Title { get; private set; }

            public string Subtitle { get; private set; }

            public Func<Control> Factory { get; private set; }
        }
    }
}
