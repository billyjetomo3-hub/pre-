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
    public class RequestsView : UserControl
    {
        private readonly Session _session;

        private DataGridView _grid;
        private ComboBox _statusFilter;
        private TextBox _searchBox;

        private Label _requestNoLabel;
        private Label _studentLabel;
        private Label _typeLabel;
        private Label _filedLabel;
        private Label _feeLabel;
        private Label _paymentLabel;
        private Label _reasonLabel;
        private Label _remarksLabel;
        private Label _statusBadge;

        private RoundedButton _approveButton;
        private RoundedButton _rejectButton;
        private RoundedButton _paidButton;
        private RoundedButton _readyButton;
        private RoundedButton _releaseButton;
        private RoundedButton _printButton;

        public RequestsView(Session session)
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
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Theme.Background,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // ---------------------------------------------------------- List card
            var listCard = new CardPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 14, 0),
                Padding = new Padding(20)
            };

            var listContent = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = Theme.Surface,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            listContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            listContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));
            listContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 8f));
            listContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
            listContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 10f));
            listContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 116f));
            header.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var titles = Ui.Stack();
            Ui.Row(titles, Ui.Line("Replacement requests", Theme.H3, Theme.Text), 24);
            Ui.Row(titles, Ui.Line("Approve, record payments and release cards.", Theme.Tiny, Theme.Muted), 20);
            header.Controls.Add(titles, 0, 0);

            var headerActions = Ui.ActionBar();
            headerActions.Padding = new Padding(0, 6, 0, 0);

            var refresh = Ui.SecondaryButton("Refresh");
            refresh.Size = new Size(104, 32);
            refresh.Margin = new Padding(0);
            refresh.Click += delegate { LoadRequests(); };
            headerActions.Controls.Add(refresh);

            header.Controls.Add(headerActions, 1, 0);
            listContent.Controls.Add(header, 0, 0);

            var filters = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150f));
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96f));
            filters.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            _statusFilter = Ui.Dropdown(BuildStatusFilter());
            _statusFilter.Dock = DockStyle.Fill;
            _statusFilter.Margin = new Padding(0, 0, 8, 0);
            _statusFilter.SelectedIndexChanged += delegate { LoadRequests(); };
            filters.Controls.Add(_statusFilter, 0, 0);

            _searchBox = Ui.InputBox();
            _searchBox.Dock = DockStyle.Fill;
            _searchBox.Margin = new Padding(0, 2, 8, 4);
            _searchBox.KeyDown += delegate(object sender, KeyEventArgs args)
            {
                if (args.KeyCode == Keys.Enter)
                {
                    args.SuppressKeyPress = true;
                    LoadRequests();
                }
            };
            filters.Controls.Add(_searchBox, 1, 0);

            var searchButton = Ui.PrimaryButton("Search");
            searchButton.Size = new Size(96, 32);
            searchButton.Dock = DockStyle.Fill;
            searchButton.Margin = new Padding(0);
            searchButton.Click += delegate { LoadRequests(); };
            filters.Controls.Add(searchButton, 2, 0);

            listContent.Controls.Add(filters, 0, 2);

            _grid = Ui.DataGrid();
            _grid.SelectionChanged += delegate { ShowDetails(); };
            _grid.Margin = new Padding(0);
            listContent.Controls.Add(_grid, 0, 4);

            listCard.Controls.Add(listContent);
            root.Controls.Add(listCard, 0, 0);

            // -------------------------------------------------------- Detail card
            var detailCard = new CardPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(14, 0, 0, 0),
                Padding = new Padding(20)
            };

            var detail = Ui.Stack();

            Ui.Row(detail, Ui.Caption("REQUEST DETAILS"), 16);
            Ui.Gap(detail, 4);

            _statusBadge = new Label
            {
                Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "-",
                AutoSize = false,
                Size = new Size(160, 26),
                BackColor = Theme.Field,
                ForeColor = Theme.Muted
            };
            Ui.Row(detail, _statusBadge, 26);
            _statusBadge.Dock = DockStyle.None;
            _statusBadge.Size = new Size(160, 26);

            Ui.Gap(detail, 6);

            _requestNoLabel = Ui.Line("-", new Font("Segoe UI Semibold", 13f, FontStyle.Bold), Theme.Text);
            Ui.Row(detail, _requestNoLabel, 30);

            _studentLabel = Ui.Block("-", Theme.Body, Theme.Text);
            Ui.Row(detail, _studentLabel, 34);

            _typeLabel = Ui.Line("-", Theme.Small, Theme.Muted);
            Ui.Row(detail, _typeLabel, 18);

            Ui.Gap(detail, 8);

            _filedLabel = Ui.Line("-", Theme.Small, Theme.Text);
            Ui.Row(detail, Ui.MetaCell("FILED", _filedLabel), 32);

            _feeLabel = Ui.Line("-", Theme.Small, Theme.Text);
            Ui.Row(detail, Ui.MetaCell("FEE", _feeLabel), 32);

            _paymentLabel = Ui.Line("-", Theme.Small, Theme.Text);
            Ui.Row(detail, Ui.MetaCell("PAYMENT", _paymentLabel), 32);

            Ui.Gap(detail, 8);
            Ui.Row(detail, Ui.Caption("REASON"), 16);

            _reasonLabel = Ui.Block("-", Theme.Small, Theme.Text);
            Ui.Row(detail, _reasonLabel, 56);

            Ui.Gap(detail, 6);
            Ui.Row(detail, Ui.Caption("REGISTRAR REMARKS"), 16);

            _remarksLabel = Ui.Block("-", Theme.Small, Theme.Muted);
            Ui.Row(detail, _remarksLabel, 44);

            Ui.Gap(detail, 4);

            detailCard.Controls.Add(Ui.ScrollHost(detail));

            var actions = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 156,
                ColumnCount = 2,
                RowCount = 3,
                BackColor = Theme.Surface,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            for (var index = 0; index < 3; index++)
            {
                actions.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33f));
            }

            _approveButton = Ui.SuccessButton("Approve");
            _rejectButton = Ui.DangerButton("Reject");
            _paidButton = Ui.SecondaryButton("Record payment");
            _readyButton = Ui.SecondaryButton("Mark ready");
            _releaseButton = Ui.PrimaryButton("Release card");
            _printButton = Ui.SecondaryButton("Print ID card");

            ConfigureActionButton(_approveButton, 0, 0, actions);
            ConfigureActionButton(_rejectButton, 1, 0, actions);
            ConfigureActionButton(_paidButton, 0, 1, actions);
            ConfigureActionButton(_readyButton, 1, 1, actions);
            ConfigureActionButton(_releaseButton, 0, 2, actions);
            ConfigureActionButton(_printButton, 1, 2, actions);

            _approveButton.Click += ApproveClicked;
            _rejectButton.Click += RejectClicked;
            _paidButton.Click += PaidClicked;
            _readyButton.Click += ReadyClicked;
            _releaseButton.Click += ReleaseClicked;
            _printButton.Click += PrintCardClicked;

            detailCard.Controls.Add(actions);

            root.Controls.Add(detailCard, 1, 0);
            Controls.Add(root);

            ResumeLayout(false);
        }

        private static void ConfigureActionButton(RoundedButton button, int column, int row, TableLayoutPanel host)
        {
            button.Dock = DockStyle.Fill;
            button.Margin = new Padding(4);
            button.Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold);
            host.Controls.Add(button, column, row);
        }

        private static string[] BuildStatusFilter()
        {
            var values = new string[RequestStatus.All.Length + 1];
            values[0] = "All";

            for (var index = 0; index < RequestStatus.All.Length; index++)
            {
                values[index + 1] = RequestStatus.All[index];
            }

            return values;
        }

        private void LoadRequests()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                var previousId = SelectedRequestId();
                var table = RequestService.ListAll(Convert.ToString(_statusFilter.SelectedItem), _searchBox.Text);

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

                if (previousId.HasValue)
                {
                    foreach (DataGridViewRow row in _grid.Rows)
                    {
                        var view = row.DataBoundItem as DataRowView;
                        if (view != null && Convert.ToInt32(view["Id"]) == previousId.Value)
                        {
                            row.Selected = true;
                            _grid.CurrentCell = row.Cells[0];
                            break;
                        }
                    }
                }

                ShowDetails();
            }
            catch (Exception ex)
            {
                Ui.ShowError(this, "The requests could not be loaded.\r\n\r\n" + ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private int? SelectedRequestId()
        {
            if (_grid == null || _grid.CurrentRow == null)
            {
                return null;
            }

            var view = _grid.CurrentRow.DataBoundItem as DataRowView;
            return view == null ? (int?)null : Convert.ToInt32(view["Id"]);
        }

        private IdRequest SelectedRequest()
        {
            var id = SelectedRequestId();
            return id.HasValue ? RequestService.Get(id.Value) : null;
        }

        private void ShowDetails()
        {
            var request = SelectedRequest();

            if (request == null)
            {
                _requestNoLabel.Text = "No request selected";
                _studentLabel.Text = "Select a row to review the request.";
                _typeLabel.Text = "-";
                _filedLabel.Text = "-";
                _feeLabel.Text = "-";
                _paymentLabel.Text = "-";
                _reasonLabel.Text = "-";
                _remarksLabel.Text = "-";
                _statusBadge.Text = "-";
                _statusBadge.BackColor = Theme.Field;
                _statusBadge.ForeColor = Theme.Muted;
                SetActionsEnabled(false, false, false, false, false, false);
                return;
            }

            _requestNoLabel.Text = request.RequestNo;
            _studentLabel.Text = request.StudentName + "\r\n" + request.StudentNo;
            _typeLabel.Text = request.RequestType;
            _filedLabel.Text = request.DateRequested.ToString("MMM d, yyyy h:mm tt");
            _feeLabel.Text = "PHP " + request.FeeAmount.ToString("#,##0.00");
            _paymentLabel.Text = request.PaymentStatus + (string.IsNullOrEmpty(request.OrNumber) ? string.Empty : " - OR " + request.OrNumber);
            _reasonLabel.Text = string.IsNullOrWhiteSpace(request.Reason) ? "-" : request.Reason;
            _remarksLabel.Text = string.IsNullOrWhiteSpace(request.Remarks) ? "-" : request.Remarks;

            _statusBadge.Text = request.Status.ToUpperInvariant();
            _statusBadge.BackColor = Theme.StatusBackground(request.Status);
            _statusBadge.ForeColor = Theme.StatusColor(request.Status);

            var isPending = request.Status == RequestStatus.Pending;
            var isApproved = request.Status == RequestStatus.Approved;
            var isPaid = request.PaymentStatus == PaymentStatus.Paid;
            var isReady = request.Status == RequestStatus.ReadyForPickup;

            SetActionsEnabled(
                isPending,
                isPending,
                isApproved && !isPaid,
                isApproved && isPaid,
                isReady,
                isApproved || isReady || request.Status == RequestStatus.Released);
        }

        private void SetActionsEnabled(bool approve, bool reject, bool paid, bool ready, bool release, bool print)
        {
            _approveButton.Enabled = approve;
            _rejectButton.Enabled = reject;
            _paidButton.Enabled = paid;
            _readyButton.Enabled = ready;
            _releaseButton.Enabled = release;
            _printButton.Enabled = print;
        }

        private void ApproveClicked(object sender, EventArgs e)
        {
            var request = SelectedRequest();
            if (request == null)
            {
                return;
            }

            var remarks = PromptDialog.Ask(
                this,
                "Approve request",
                "Optional note for the student. Leave blank to approve without a note.",
                string.Empty,
                true,
                false,
                false);

            if (remarks == null)
            {
                return;
            }

            RunAction(delegate
            {
                RequestService.Approve(request.Id, _session.FullName, remarks);
                Ui.ShowInfo(this, "Request " + request.RequestNo + " approved.\r\nAsk the student to pay the fee at the cashier.");
            });
        }

        private void RejectClicked(object sender, EventArgs e)
        {
            var request = SelectedRequest();
            if (request == null)
            {
                return;
            }

            var remarks = PromptDialog.Ask(
                this,
                "Reject request",
                "Explain why the request is rejected. The student will see this note.",
                string.Empty,
                true,
                true,
                false);

            if (remarks == null)
            {
                return;
            }

            RunAction(delegate { RequestService.Reject(request.Id, _session.FullName, remarks); });
        }

        private void PaidClicked(object sender, EventArgs e)
        {
            var request = SelectedRequest();
            if (request == null)
            {
                return;
            }

            var orNumber = PromptDialog.Ask(
                this,
                "Record payment",
                "Enter the official receipt number issued by the cashier.",
                string.Empty,
                false,
                true,
                false);

            if (orNumber == null)
            {
                return;
            }

            RunAction(delegate { RequestService.MarkPaid(request.Id, _session.FullName, orNumber); });
        }

        private void ReadyClicked(object sender, EventArgs e)
        {
            var request = SelectedRequest();
            if (request == null)
            {
                return;
            }

            if (!Ui.Confirm(this, "Mark this card as printed and ready for pickup?"))
            {
                return;
            }

            RunAction(delegate
            {
                RequestService.MarkReady(request.Id, _session.FullName, "Card printed and ready for pickup.");
                Ui.ShowInfo(this, "The student can now claim the card at the registrar's office.");
            });
        }

        private void ReleaseClicked(object sender, EventArgs e)
        {
            var request = SelectedRequest();
            if (request == null)
            {
                return;
            }

            if (!Ui.Confirm(this, "Confirm that the card was released to the student?"))
            {
                return;
            }

            RunAction(delegate { RequestService.Release(request.Id, _session.FullName, "Card released to the student."); });
        }

        private void PrintCardClicked(object sender, EventArgs e)
        {
            var request = SelectedRequest();
            if (request == null)
            {
                return;
            }

            try
            {
                using (var form = new IdCardForm(request.StudentId))
                {
                    form.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                Ui.ShowError(this, ex.Message);
            }
        }

        private void RunAction(Action action)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                action();
                LoadRequests();
            }
            catch (InvalidOperationException ex)
            {
                Ui.ShowError(this, ex.Message);
            }
            catch (Exception ex)
            {
                Ui.ShowError(this, "The action could not be completed.\r\n\r\n" + ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
    }
}
