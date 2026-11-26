using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using SchoolIdReplacement.Models;
using SchoolIdReplacement.Services;
using SchoolIdReplacement.UI;

namespace SchoolIdReplacement.Views
{
    public class DashboardView : UserControl
    {
        private readonly Session _session;

        private TableLayoutPanel _statsPanel;
        private Panel _gridHost;
        private Label _gridTitle;
        private Label _gridHint;

        public DashboardView(Session session)
        {
            _session = session;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            BackColor = Theme.Background;
            Dock = DockStyle.Fill;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Theme.Background,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 132f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            _statsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            for (var index = 0; index < 4; index++)
            {
                _statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            }
            _statsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.Controls.Add(_statsPanel, 0, 0);

            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 18, 0, 0),
                Padding = new Padding(0)
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66f));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34f));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // ------------------------------------------------------ Recent requests
            var listCard = new CardPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 14, 0),
                Padding = new Padding(20)
            };

            _gridHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.Surface,
                Margin = new Padding(0)
            };
            listCard.Controls.Add(_gridHost);

            var listHeader = Ui.Stack();
            var listTitle = Ui.Stack();
            _gridTitle = Ui.Line("Recent requests", Theme.H3, Theme.Text);
            Ui.Row(listTitle, _gridTitle, 24);
            _gridHint = Ui.Line("Latest activity across the registrar's queue", Theme.Tiny, Theme.Muted);
            Ui.Row(listTitle, _gridHint, 20);
            Ui.Row(listHeader, listTitle, 44);
            Ui.Gap(listHeader, 10);

            listCard.Controls.Add(listHeader);

            body.Controls.Add(listCard, 0, 0);

            // ------------------------------------------------------------- Guide
            var guideCard = new CardPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(14, 0, 0, 0),
                Padding = new Padding(20)
            };

            var guide = Ui.Stack();
            Ui.Row(guide, Ui.Line("How a replacement works", Theme.H3, Theme.Text), 26);
            Ui.Gap(guide, 10);

            var steps = new[]
            {
                "File the request with the reason",
                "Registrar reviews and approves",
                "Pay the assessed fee at the cashier",
                "Card is printed and marked ready",
                "Claim the ID and confirm release"
            };

            for (var index = 0; index < steps.Length; index++)
            {
                Ui.Row(guide, StepRow(index + 1, steps[index]), 40);
            }

            Ui.Gap(guide, 8);
            Ui.Row(guide, new Panel { BackColor = Theme.Border }, 1);
            Ui.Gap(guide, 10);

            var support = Ui.Block("Questions? " + SchoolProfile.Contact, Theme.Tiny, Theme.Muted);
            Ui.Row(guide, support, 34);

            guideCard.Controls.Add(Ui.ScrollHost(guide));

            body.Controls.Add(guideCard, 1, 0);

            root.Controls.Add(body, 0, 1);
            Controls.Add(root);

            ResumeLayout(false);
        }

        private static Control StepRow(int number, string text)
        {
            var row = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34f));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            row.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var badge = new Label
            {
                Text = number.ToString(),
                Font = Theme.Caption,
                ForeColor = Theme.Accent,
                BackColor = Theme.AccentSoft,
                AutoSize = false,
                Size = new Size(24, 24),
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 2, 0, 0)
            };
            row.Controls.Add(badge, 0, 0);

            var label = Ui.Block(text, Theme.Small, Theme.Text);
            label.Padding = new Padding(0, 3, 0, 0);
            row.Controls.Add(label, 1, 0);

            return row;
        }

        private void LoadData()
        {
            _statsPanel.SuspendLayout();
            _statsPanel.Controls.Clear();

            if (_session.IsAdministrator)
            {
                var stats = RequestService.GetStatistics();

                AddStatCard(0, "PENDING REQUESTS", stats.Pending.ToString(), "Waiting for review", Theme.Warning, Theme.Kpi);
                AddStatCard(1, "AWAITING PAYMENT", stats.UnpaidApproved.ToString(), "Approved, unpaid", Theme.Info, Theme.Kpi);
                AddStatCard(2, "READY FOR PICKUP", stats.ReadyForPickup.ToString(), "Cards printed", Theme.Accent, Theme.Kpi);
                AddStatCard(
                    3,
                    "RELEASED THIS MONTH",
                    stats.ReleasedThisMonth.ToString(),
                    "Collected: PHP " + stats.CollectedThisMonth.ToString("#,##0.00"),
                    Theme.Success,
                    Theme.Kpi);

                BindGrid(RequestService.ListAll("All", null));
                _gridTitle.Text = "Recent requests";
                _gridHint.Text = "Latest activity across the registrar's queue";
            }
            else
            {
                var requests = RequestService.ListForStudent(_session.StudentId ?? 0);

                var open = 0;
                var ready = 0;
                var released = 0;
                var unpaid = 0m;

                foreach (DataRow row in requests.Rows)
                {
                    var status = Convert.ToString(row["Status"]);

                    if (status == RequestStatus.Pending || status == RequestStatus.Approved || status == RequestStatus.ReadyForPickup)
                    {
                        open++;
                    }

                    if (status == RequestStatus.ReadyForPickup)
                    {
                        ready++;
                    }

                    if (status == RequestStatus.Released)
                    {
                        released++;
                    }

                    if (Convert.ToString(row["Payment"]) == PaymentStatus.Unpaid && status != RequestStatus.Cancelled && status != RequestStatus.Rejected)
                    {
                        unpaid += Convert.ToDecimal(row["Fee"]);
                    }
                }

                AddStatCard(0, "OPEN REQUESTS", open.ToString(), "In progress", Theme.Warning, Theme.Kpi);
                AddStatCard(1, "READY FOR PICKUP", ready.ToString(), "Claim at the registrar", Theme.Accent, Theme.Kpi);
                AddStatCard(2, "RELEASED", released.ToString(), "Completed replacements", Theme.Success, Theme.Kpi);
                AddStatCard(3, "BALANCE DUE", "PHP " + unpaid.ToString("#,##0.00"), "Unpaid assessments", Theme.Info, Theme.KpiSmall);

                BindGrid(RequestService.RecentForStudent(_session.StudentId ?? 0, 12));
                _gridTitle.Text = "My recent requests";
                _gridHint.Text = "Your latest replacement requests and their status";
            }

            _statsPanel.ResumeLayout();
        }

        private void AddStatCard(int column, string title, string value, string hint, Color accent, Font valueFont)
        {
            var card = new CardPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, column < 3 ? 14 : 0, 0),
                Padding = new Padding(20, 16, 20, 14)
            };

            var valueLabel = Ui.Line(value, valueFont, Theme.Text);
            card.Controls.Add(valueLabel);

            var hintLabel = Ui.Line(hint, Theme.Tiny, accent);
            hintLabel.Dock = DockStyle.Bottom;
            hintLabel.Height = 22;
            card.Controls.Add(hintLabel);

            var titleLabel = Ui.Line(title, Theme.Micro, Theme.Muted);
            titleLabel.Dock = DockStyle.Top;
            titleLabel.Height = 18;
            card.Controls.Add(titleLabel);

            _statsPanel.Controls.Add(card, column, 0);
        }

        private void BindGrid(DataTable table)
        {
            _gridHost.SuspendLayout();

            foreach (Control control in _gridHost.Controls)
            {
                control.Dispose();
            }
            _gridHost.Controls.Clear();

            var grid = Ui.DataGrid();
            grid.DataSource = table;

            var idColumn = grid.Columns["Id"];
            if (idColumn != null)
            {
                idColumn.Visible = false;
            }

            var feeColumn = grid.Columns["Fee"];
            if (feeColumn != null)
            {
                feeColumn.DefaultCellStyle.Format = "#,##0.00";
                feeColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            _gridHost.Controls.Add(grid);
            _gridHost.ResumeLayout();
        }
    }
}
