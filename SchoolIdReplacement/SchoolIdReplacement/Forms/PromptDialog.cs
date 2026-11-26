using System;
using System.Drawing;
using System.Windows.Forms;
using SchoolIdReplacement.UI;

namespace SchoolIdReplacement.Forms
{
    internal class PromptDialog : Form
    {
        private readonly bool _required;
        private TextBox _input;
        private Label _error;

        private PromptDialog(string title, string message, string initialValue, bool multiline, bool required, bool password)
        {
            _required = required;
            InitializeComponent(title, message, initialValue, multiline, password);
        }

        public string Value
        {
            get { return _input.Text.Trim(); }
        }

        public static string Ask(IWin32Window owner, string title, string message, string initialValue, bool multiline, bool required, bool password)
        {
            using (var dialog = new PromptDialog(title, message, initialValue, multiline, required, password))
            {
                var result = owner == null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
                return result == DialogResult.OK ? dialog.Value : null;
            }
        }

        private void InitializeComponent(string title, string message, string initialValue, bool multiline, bool password)
        {
            SuspendLayout();

            Text = title;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = Color.White;
            Font = Theme.Body;
            ClientSize = new Size(460, multiline ? 300 : 220);

            var heading = Ui.TextBlock(title, new Font("Segoe UI Semibold", 12f, FontStyle.Bold), Theme.Text);
            heading.Location = new Point(24, 22);
            Controls.Add(heading);

            var body = new Label
            {
                Text = message,
                Font = Theme.Small,
                ForeColor = Theme.Muted,
                Location = new Point(24, 52),
                Size = new Size(412, 38),
                BackColor = Color.Transparent
            };
            Controls.Add(body);

            _input = multiline ? Ui.MemoBox() : Ui.InputBox();
            _input.Location = new Point(24, 94);
            _input.Size = new Size(412, multiline ? 110 : 30);
            _input.Text = initialValue ?? string.Empty;
            _input.UseSystemPasswordChar = password;
            Controls.Add(_input);

            _error = new Label
            {
                Location = new Point(24, multiline ? 212 : 132),
                Size = new Size(412, 20),
                Font = Theme.Small,
                ForeColor = Theme.Danger,
                Text = string.Empty,
                BackColor = Color.Transparent
            };
            Controls.Add(_error);

            var cancel = Ui.SecondaryButton("Cancel");
            cancel.Location = new Point(228, multiline ? 244 : 164);
            cancel.Size = new Size(96, 38);
            cancel.DialogResult = DialogResult.Cancel;
            Controls.Add(cancel);

            var ok = Ui.PrimaryButton("Confirm");
            ok.Location = new Point(334, multiline ? 244 : 164);
            ok.Size = new Size(102, 38);
            ok.Click += OkClicked;
            Controls.Add(ok);

            AcceptButton = ok;
            CancelButton = cancel;
            ActiveControl = _input;

            ResumeLayout(false);
        }

        private void OkClicked(object sender, EventArgs e)
        {
            if (_required && Value.Length == 0)
            {
                _error.Text = "This field is required.";
                _input.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
