using System;
using System.Drawing;
using System.Windows.Forms;
using SchoolIdReplacement.Models;
using SchoolIdReplacement.Services;
using SchoolIdReplacement.UI;

namespace SchoolIdReplacement.Views
{
    public class NewRequestView : UserControl
    {
        private readonly Session _session;

        private ComboBox _typeBox;
        private TextBox _reasonBox;
        private Label _requirementLabel;
        private Label _feeLabel;
        private RoundedButton _submitButton;

        public NewRequestView(Session session)
        {
            _session = session;
            InitializeComponent();
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

            // ---------------------------------------------------------- Form card
            var formCard = new CardPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 14, 0),
                Padding = new Padding(24)
            };

            var form = Ui.Stack();

            Ui.Row(form, Ui.Line("File a replacement request", Theme.H2, Theme.Text), 32);

            var subtitle = Ui.Block(
                "The registrar will review your request and contact you through your school email.",
                Theme.Small,
                Theme.Muted);
            Ui.Row(form, subtitle, 34);

            Ui.Gap(form, 14);
            Ui.Row(form, Ui.Caption("REPLACEMENT TYPE"), 18);

            _typeBox = Ui.Dropdown(RequestTypes.All);
            _typeBox.SelectedIndexChanged += TypeChanged;
            Ui.Row(form, _typeBox, 28);

            Ui.Gap(form, 12);

            var requirement = new Panel
            {
                BackColor = Theme.InfoSoft,
                Padding = new Padding(12, 8, 12, 8),
                Margin = new Padding(0)
            };
            _requirementLabel = Ui.Block(string.Empty, Theme.Small, Theme.Info);
            requirement.Controls.Add(_requirementLabel);
            Ui.Row(form, requirement, 60);

            Ui.Gap(form, 16);
            Ui.Row(form, Ui.Caption("REASON AND DETAILS"), 18);

            _reasonBox = Ui.MemoBox();
            _reasonBox.MaxLength = 480;
            Ui.Row(form, _reasonBox, 120);

            Ui.Gap(form, 6);
            Ui.Row(
                form,
                Ui.Line("At least 15 characters. Include dates and the last place you used the ID.", Theme.Tiny, Theme.Muted),
                18);

            Ui.Gap(form, 12);
            _feeLabel = Ui.Line("Assessment: PHP 0.00", Theme.H3, Theme.Accent);
            Ui.Row(form, _feeLabel, 24);

            Ui.Gap(form, 12);

            var buttons = Ui.InlineBar();
            _submitButton = Ui.PrimaryButton("Submit request");
            _submitButton.Size = new Size(160, 44);
            _submitButton.Margin = new Padding(0);
            _submitButton.Click += SubmitClicked;
            buttons.Controls.Add(_submitButton);

            var clear = Ui.SecondaryButton("Clear");
            clear.Size = new Size(112, 44);
            clear.Margin = new Padding(10, 0, 0, 0);
            clear.Click += delegate
            {
                _typeBox.SelectedIndex = 0;
                _reasonBox.Clear();
                _reasonBox.Focus();
            };
            buttons.Controls.Add(clear);

            Ui.Row(form, buttons, 44);
            Ui.Gap(form, 4);

            formCard.Controls.Add(Ui.ScrollHost(form));
            root.Controls.Add(formCard, 0, 0);

            // ---------------------------------------------------------- Side card
            var sideCard = new CardPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(14, 0, 0, 0),
                Padding = new Padding(20)
            };

            var side = Ui.Stack();
            Ui.Row(side, Ui.Line("Before you file", Theme.H3, Theme.Text), 26);
            Ui.Gap(side, 8);

            var notes = new[]
            {
                "Only one open request is allowed at a time.",
                "Lost IDs need an affidavit of loss from the barangay.",
                "Stolen IDs require a police report reference.",
                "Bring the old card when claiming a replacement.",
                "Fees are paid at the cashier after approval."
            };

            for (var index = 0; index < notes.Length; index++)
            {
                Ui.Row(side, NoteRow(notes[index]), 44);
            }

            Ui.Gap(side, 8);
            Ui.Row(side, new Panel { BackColor = Theme.Border }, 1);
            Ui.Gap(side, 10);

            Ui.Row(side, Ui.Caption("YOUR RECORD"), 18);
            Ui.Gap(side, 6);

            Ui.Row(side, Ui.Line(_session.FullName, Theme.BodyStrong, Theme.Text), 22);
            Ui.Row(side, Ui.Line("Student No: " + (_session.StudentNo ?? "-"), Theme.Small, Theme.Muted), 22);

            var registrar = Ui.Block("Registrar: " + SchoolProfile.Contact, Theme.Small, Theme.Muted);
            Ui.Row(side, registrar, 34);

            Ui.Gap(side, 8);
            Ui.Row(
                side,
                Ui.Block("Keep the request number shown after filing. You will need it when claiming the card.", Theme.Tiny, Theme.Muted),
                40);

            sideCard.Controls.Add(Ui.ScrollHost(side));
            root.Controls.Add(sideCard, 1, 0);

            Controls.Add(root);

            TypeChanged(null, EventArgs.Empty);

            ResumeLayout(false);
        }

        private static Control NoteRow(string text)
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
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 16f));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            row.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var dash = new Label
            {
                Text = "-",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Theme.Accent,
                AutoSize = false,
                Size = new Size(12, 18),
                TextAlign = ContentAlignment.TopLeft,
                Margin = new Padding(0, 2, 0, 0)
            };
            row.Controls.Add(dash, 0, 0);

            var label = Ui.Block(text, Theme.Small, Theme.Text);
            label.Padding = new Padding(0, 0, 0, 2);
            row.Controls.Add(label, 1, 0);

            return row;
        }

        private void TypeChanged(object sender, EventArgs e)
        {
            var type = Convert.ToString(_typeBox.SelectedItem);
            _requirementLabel.Text = RequestTypes.DescriptionFor(type);
            _feeLabel.Text = "Assessment: PHP " + RequestTypes.FeeFor(type).ToString("#,##0.00");
        }

        private void SubmitClicked(object sender, EventArgs e)
        {
            if (!_session.StudentId.HasValue)
            {
                Ui.ShowError(this, "Your account is not linked to a student record. Please contact the registrar.");
                return;
            }

            _submitButton.Enabled = false;

            try
            {
                var request = RequestService.Create(
                    _session.StudentId.Value,
                    Convert.ToString(_typeBox.SelectedItem),
                    _reasonBox.Text);

                Ui.ShowInfo(
                    this,
                    "Your request was filed successfully.\r\n\r\n" +
                    "Request number: " + request.RequestNo + "\r\n" +
                    "Status: " + request.Status + "\r\n" +
                    "Assessment: PHP " + request.FeeAmount.ToString("#,##0.00") + "\r\n\r\n" +
                    "Track it under \"My Requests\".");

                _reasonBox.Clear();
                _typeBox.SelectedIndex = 0;
            }
            catch (InvalidOperationException ex)
            {
                Ui.ShowError(this, ex.Message);
            }
            catch (Exception ex)
            {
                Ui.ShowError(this, "The request could not be saved.\r\n\r\n" + ex.Message);
            }
            finally
            {
                _submitButton.Enabled = true;
            }
        }
    }
}
