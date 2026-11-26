using System;
using System.Drawing;
using System.Windows.Forms;
using SchoolIdReplacement.Services;
using SchoolIdReplacement.UI;

namespace SchoolIdReplacement.Views
{
    public class ReportsView : UserControl
    {
        private DateTimePicker _fromPicker;
        private DateTimePicker _toPicker;
        private Label _summaryLabel;
        private DataGridView _statusGrid;
        private DataGridView _typeGrid;

        public ReportsView()
        {
            InitializeComponent();
            Generate();
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
                RowCount = 3,
                BackColor = Theme.Background,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 148f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            // ------------------------------------------------------- Filter card
            var filterCard = new CardPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 0),
                Padding = new Padding(20)
            };

            var filter = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = Theme.Surface,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            filter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            filter.RowStyles.Add(new RowStyle(SizeType.Absolute, 26f));
            filter.RowStyles.Add(new RowStyle(SizeType.Absolute, 8f));
            filter.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
            filter.RowStyles.Add(new RowStyle(SizeType.Absolute, 8f));
            filter.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            filter.Controls.Add(Ui.Line("Reporting period", Theme.H3, Theme.Text), 0, 0);

            var controls = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            var fromLabel = new Label
            {
                Text = "FROM",
                Font = Theme.Caption,
                ForeColor = Theme.Muted,
                AutoSize = true,
                Margin = new Padding(0, 9, 8, 0),
                BackColor = Color.Transparent
            };
            controls.Controls.Add(fromLabel);

            _fromPicker = new DateTimePicker
            {
                Size = new Size(140, 26),
                Margin = new Padding(0, 3, 16, 0),
                Format = DateTimePickerFormat.Short,
                Font = Theme.Body,
                Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)
            };
            controls.Controls.Add(_fromPicker);

            var toLabel = new Label
            {
                Text = "TO",
                Font = Theme.Caption,
                ForeColor = Theme.Muted,
                AutoSize = true,
                Margin = new Padding(0, 9, 8, 0),
                BackColor = Color.Transparent
            };
            controls.Controls.Add(toLabel);

            _toPicker = new DateTimePicker
            {
                Size = new Size(140, 26),
                Margin = new Padding(0, 3, 16, 0),
                Format = DateTimePickerFormat.Short,
                Font = Theme.Body,
                Value = DateTime.Now
            };
            controls.Controls.Add(_toPicker);

            var generate = Ui.PrimaryButton("Generate");
            generate.Size = new Size(110, 32);
            generate.Margin = new Padding(0, 1, 8, 0);
            generate.Click += delegate { Generate(); };
            controls.Controls.Add(generate);

            var export = Ui.SecondaryButton("Export CSV");
            export.Size = new Size(120, 32);
            export.Margin = new Padding(0, 1, 0, 0);
            export.Click += delegate { Export(); };
            controls.Controls.Add(export);

            filter.Controls.Add(controls, 0, 2);

            _summaryLabel = Ui.Line(string.Empty, Theme.Tiny, Theme.Muted);
            filter.Controls.Add(_summaryLabel, 0, 4);

            filterCard.Controls.Add(filter);
            root.Controls.Add(filterCard, 0, 0);

            // ------------------------------------------------------- Status table
            var statusCard = new CardPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 14, 0, 0),
                Padding = new Padding(20)
            };

            var statusContent = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Theme.Surface,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            statusContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            statusContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 26f));
            statusContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 10f));
            statusContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            statusContent.Controls.Add(Ui.Line("Requests by status", Theme.H3, Theme.Text), 0, 0);

            _statusGrid = Ui.DataGrid();
            statusContent.Controls.Add(_statusGrid, 0, 2);

            statusCard.Controls.Add(statusContent);
            root.Controls.Add(statusCard, 0, 1);

            // --------------------------------------------------------- Type table
            var typeCard = new CardPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 14, 0, 0),
                Padding = new Padding(20)
            };

            var typeContent = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Theme.Surface,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            typeContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            typeContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 26f));
            typeContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 10f));
            typeContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            typeContent.Controls.Add(Ui.Line("Requests by replacement type", Theme.H3, Theme.Text), 0, 0);

            _typeGrid = Ui.DataGrid();
            typeContent.Controls.Add(_typeGrid, 0, 2);

            typeCard.Controls.Add(typeContent);
            root.Controls.Add(typeCard, 0, 2);

            Controls.Add(root);

            ResumeLayout(false);
        }

        private void Generate()
        {
            if (_toPicker.Value.Date < _fromPicker.Value.Date)
            {
                Ui.ShowError(this, "The end date must be on or after the start date.");
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;

                var from = _fromPicker.Value.Date;
                var to = _toPicker.Value.Date;

                _statusGrid.DataSource = ReportService.SummaryByStatus(from, to);
                _typeGrid.DataSource = ReportService.SummaryByType(from, to);

                var totalRequests = 0;
                var totalFees = 0m;

                foreach (System.Data.DataRow row in ((System.Data.DataTable)_statusGrid.DataSource).Rows)
                {
                    totalRequests += Convert.ToInt32(row["Requests"]);
                    totalFees += Convert.ToDecimal(row["Total Fees"]);
                }

                _summaryLabel.Text = totalRequests + " request(s)  -  PHP " + totalFees.ToString("#,##0.00") + " assessed  -  " +
                    from.ToString("MMM d, yyyy") + " to " + to.ToString("MMM d, yyyy");

                FormatMoney(_statusGrid, "Total Fees");
                FormatMoney(_typeGrid, "Assessed Fees");
            }
            catch (Exception ex)
            {
                Ui.ShowError(this, "The report could not be generated.\r\n\r\n" + ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private static void FormatMoney(DataGridView grid, string columnName)
        {
            var column = grid.Columns[columnName];
            if (column != null)
            {
                column.DefaultCellStyle.Format = "#,##0.00";
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

        private void Export()
        {
            try
            {
                using (var dialog = new SaveFileDialog
                {
                    Title = "Export report",
                    Filter = "CSV file (*.csv)|*.csv",
                    FileName = "id-replacement-report-" + DateTime.Now.ToString("yyyyMMdd") + ".csv"
                })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }

                    var table = ReportService.SummaryByStatus(_fromPicker.Value.Date, _toPicker.Value.Date);
                    ReportService.ExportCsv(table, dialog.FileName);
                    Ui.ShowInfo(this, "Report exported to:\r\n" + dialog.FileName);
                }
            }
            catch (Exception ex)
            {
                Ui.ShowError(this, "The report could not be exported.\r\n\r\n" + ex.Message);
            }
        }
    }
}
