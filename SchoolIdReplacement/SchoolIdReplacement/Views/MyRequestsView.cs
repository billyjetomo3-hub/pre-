using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using SchoolIdReplacement.Forms;
using SchoolIdReplacement.Models;
using SchoolIdReplacement.Services;
using SchoolIdReplacement.UI;

namespace SchoolIdReplacement.Views
{
    public class MyRequestsView : UserControl
    {
        private readonly Session _session;

        private DataGridView _grid;
        private Label _requestNoLabel;
        private Label _typeLabel;
        private Label _filedLabel;
        private Label _releasedLabel;
        private Label _paymentLabel;
        private Label _feeLabel;
        private Label _orLabel;
        private Label _remarksLabel;
        private Label _statusBadge;
        private RoundedButton _cancelButton;
        private RoundedButton _cardButton;

        public MyRequestsView(Session session)
        {
            _session = session;
            InitializeComponent();
            LoadRequests();
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
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 250f));

            // ---------------------------------------------------------- List card
            var listCard = new CardPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 14),
                Padding = new Padding(20)
            };

            _grid = Ui.DataGrid();
            _grid.SelectionChanged += SelectionChanged;
            _grid.CellDoubleClick += delegate { ViewCardClicked(null, EventArgs.Empty); };
            listCard.Controls.Add(_grid);

            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 48,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 396f));
            header.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var titles = Ui.Stack();
            Ui.Row(titles, Ui.Line("My replacement requests", Theme.H3, Theme.Text), 24);
            Ui.Row(titles, Ui.Line("Select a request to see the registrar's notes and payment details.", Theme.Tiny, Theme.Muted), 20);
            header.Controls.Add(titles, 0, 0);

            var actions = Ui.ActionBar();
            actions.Padding = new Padding(0, 7, 0, 0);

            _cardButton = Ui.PrimaryButton("View ID card");
            _cardButton.Size = new Size(128, 34);
            _cardButton.Margin = new Padding(8, 0, 0, 0);
            _cardButton.Click += ViewCardClicked;
            actions.Controls.Add(_cardButton);

            _cancelButton = Ui.SecondaryButton("Cancel request");
            _cancelButton.Size = new Size(134, 34);
            _cancelButton.Margin = new Padding(8, 0, 0, 0);
            _cancelButton.Click += CancelClicked;
            actions.Controls.Add(_cancelButton);

            var refresh = Ui.SecondaryButton("Refresh");
            refresh.Size = new Size(96, 34);
            refresh.Margin = new Padding(8, 0, 0, 0);
            refresh.Click += delegate { LoadRequests(); };
            actions.Controls.Add(refresh);

            header.Controls.Add(actions, 1, 0);
            listCard.Controls.Add(header);

            root.Controls.Add(listCard, 0, 0);

            // -------------------------------------------------------- Detail card
            var detailCard = new CardPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 14, 0, 0),
                Padding = new Padding(20)
            };

            var detail = Ui.Stack();

            var detailHeader = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            detailHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            detailHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170f));
            detailHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            _requestNoLabel = Ui.Line("-", new Font("Segoe UI Semibold", 12f, FontStyle.Bold), Theme.Text);
            detailHeader.Controls.Add(_requestNoLabel, 0, 0);

            _statusBadge = new Label
            {
                Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "-",
                AutoSize = false,
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                BackColor = Theme.Field,
                ForeColor = Theme.Muted
            };
            detailHeader.Controls.Add(_statusBadge, 1, 0);

            Ui.Row(detail, detailHeader, 30);
            Ui.Gap(detail, 8);

            _typeLabel = Ui.Line("-", Theme.Small, Theme.Text);
            _filedLabel = Ui.Line("-", Theme.Small, Theme.Text);
            _feeLabel = Ui.Line("-", Theme.Small, Theme.Text);
            _paymentLabel = Ui.Line("-", Theme.Small, Theme.Text);
            _releasedLabel = Ui.Line("-", Theme.Small, Theme.Text);
            _orLabel = Ui.Line("-", Theme.Small, Theme.Text);

            var metaTop = new TableLayoutPanel
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
                metaTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            }
            metaTop.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            metaTop.Controls.Add(Ui.MetaCell("TYPE", _typeLabel), 0, 0);
            metaTop.Controls.Add(Ui.MetaCell("FILED", _filedLabel), 1, 0);
            metaTop.Controls.Add(Ui.MetaCell("FEE", _feeLabel), 2, 0);
            metaTop.Controls.Add(Ui.MetaCell("PAYMENT", _paymentLabel), 3, 0);

            Ui.Row(detail, metaTop, 38);
            Ui.Gap(detail, 4);

            var metaBottom = new TableLayoutPanel
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
                metaBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            }
            metaBottom.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var releasedCell = Ui.MetaCell("RELEASED", _releasedLabel);
            metaBottom.Controls.Add(releasedCell, 0, 0);
            metaBottom.SetColumnSpan(releasedCell, 2);

            var orCell = Ui.MetaCell("OR NUMBER", _orLabel);
            metaBottom.Controls.Add(orCell, 2, 0);
            metaBottom.SetColumnSpan(orCell, 2);

            Ui.Row(detail, metaBottom, 38);
            Ui.Gap(detail, 6);

            Ui.Row(detail, Ui.Caption("REGISTRAR REMARKS"), 16);

            _remarksLabel = Ui.Block("-", Theme.Small, Theme.Text);
            Ui.Row(detail, _remarksLabel, 44);

            detailCard.Controls.Add(Ui.ScrollHost(detail));
            root.Controls.Add(detailCard, 0, 1);

            Controls.Add(root);

            ResumeLayout(false);
        }

        private DataRowView SelectedRow
        {
            get
            {
                if (_grid == null || _grid.CurrentRow == null)
                {
                    return null;
                }

                return _grid.CurrentRow.DataBoundItem as DataRowView;
            }
        }

        private void LoadRequests()
        {
            var previous = SelectedRow == null ? null : Convert.ToString(SelectedRow["Request No"]);
            var table = RequestService.ListForStudent(_session.StudentId ?? 0);

            _grid.DataSource = table;

            var idColumn = _grid.Columns["Id"];
            if (idColumn != null)
            {
                idColumn.Visible = false;
            }

            var feeColumn = _grid.Columns["Fee"];
            if (feeColumn != null)
            {
                feeColumn.DefaultCellStyle.Format = "#,##0.00";
                feeColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (!string.IsNullOrEmpty(previous))
            {
                foreach (DataGridViewRow row in _grid.Rows)
                {
                    var view = row.DataBoundItem as DataRowView;
                    if (view != null && Convert.ToString(view["Request No"]) == previous)
                    {
                        row.Selected = true;
                        _grid.CurrentCell = row.Cells[0];
                        break;
                    }
                }
            }

            ShowDetails();
        }

        private void SelectionChanged(object sender, EventArgs e)
        {
            ShowDetails();
        }

        private void ShowDetails()
        {
            var row = SelectedRow;
            if (row == null)
            {
                _requestNoLabel.Text = "No request selected";
                _typeLabel.Text = "File a new request to get started.";
                _filedLabel.Text = "-";
                _releasedLabel.Text = "-";
                _feeLabel.Text = "-";
                _paymentLabel.Text = "-";
                _orLabel.Text = "-";
                _remarksLabel.Text = "Select a row above to see its details.";
                _statusBadge.Text = "-";
                _statusBadge.BackColor = Theme.Field;
                _statusBadge.ForeColor = Theme.Muted;

                if (_cancelButton != null)
                {
                    _cancelButton.Enabled = false;
                }

                if (_cardButton != null)
                {
                    _cardButton.Enabled = false;
                }

                return;
            }

            var request = RequestService.Get(Convert.ToInt32(row["Id"]));
            if (request == null)
            {
                return;
            }

            _requestNoLabel.Text = request.RequestNo;
            _typeLabel.Text = request.RequestType;
            _filedLabel.Text = request.DateRequested.ToString("MMM d, yyyy h:mm tt");
            _releasedLabel.Text = request.DateReleased.HasValue
                ? request.DateReleased.Value.ToString("MMM d, yyyy")
                : "Not yet released";
            _feeLabel.Text = "PHP " + request.FeeAmount.ToString("#,##0.00");
            _paymentLabel.Text = request.PaymentStatus + (request.PaymentStatus == PaymentStatus.Paid ? string.Empty : " - pay at the cashier");
            _orLabel.Text = string.IsNullOrEmpty(request.OrNumber) ? "-" : request.OrNumber;
            _remarksLabel.Text = string.IsNullOrWhiteSpace(request.Remarks) ? "No remarks yet." : request.Remarks;

            _statusBadge.Text = request.Status.ToUpperInvariant();
            _statusBadge.BackColor = Theme.StatusBackground(request.Status);
            _statusBadge.ForeColor = Theme.StatusColor(request.Status);

            _cancelButton.Enabled = request.Status == RequestStatus.Pending;
            _cardButton.Enabled = request.Status == RequestStatus.ReadyForPickup || request.Status == RequestStatus.Released;
        }

        private void CancelClicked(object sender, EventArgs e)
        {
            var row = SelectedRow;
            if (row == null)
            {
                return;
            }

            if (!Ui.Confirm(this, "Cancel this request? This cannot be undone."))
            {
                return;
            }

            try
            {
                RequestService.Cancel(Convert.ToInt32(row["Id"]), "Cancelled by the student.");
                LoadRequests();
            }
            catch (Exception ex)
            {
                Ui.ShowError(this, ex.Message);
            }
        }

        private void ViewCardClicked(object sender, EventArgs e)
        {
            var row = SelectedRow;
            if (row == null || !_session.StudentId.HasValue || _cardButton == null || !_cardButton.Enabled)
            {
                return;
            }

            try
            {
                using (var form = new IdCardForm(_session.StudentId.Value))
                {
                    form.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                Ui.ShowError(this, ex.Message);
            }
        }
    }
}
