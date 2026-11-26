using System;
using System.Drawing;
using System.Windows.Forms;
using SchoolIdReplacement.Models;
using SchoolIdReplacement.Services;
using SchoolIdReplacement.UI;

namespace SchoolIdReplacement.Forms
{
    public class LoginForm : Form
    {
        private TextBox _usernameBox;
        private TextBox _passwordBox;
        private Label _errorLabel;
        private RoundedButton _signInButton;

        public LoginForm()
        {
            InitializeComponent();
        }

        public Session SignedInSession { get; private set; }

        private void InitializeComponent()
        {
            SuspendLayout();

            Text = "School ID Replacement System - Sign in";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(910, 560);
            BackColor = Color.White;
            Font = Theme.Body;

            // Brand panel
            var brand = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(390, 560),
                BackColor = Theme.Sidebar
            };

            var logo = new Avatar
            {
                Location = new Point(40, 52),
                Size = new Size(56, 56),
                Initials = "ID",
                FillColor = Theme.Accent,
                TextColor = Color.White
            };
            brand.Controls.Add(logo);

            var schoolName = Ui.TextBlock(SchoolProfile.Name, new Font("Segoe UI Semibold", 15f, FontStyle.Bold), Color.White);
            schoolName.Location = new Point(40, 126);
            brand.Controls.Add(schoolName);

            var tagline = Ui.TextBlock("Online School ID Replacement System", Theme.Body, Theme.SidebarText);
            tagline.Location = new Point(42, 158);
            brand.Controls.Add(tagline);

            brand.Controls.Add(MakeRule(40, 200, 270));

            var featureOne = Ui.TextBlock("File a replacement request without queueing", Theme.Body, Color.FromArgb(203, 213, 225));
            featureOne.Location = new Point(42, 226);
            brand.Controls.Add(featureOne);

            var featureTwo = Ui.TextBlock("Track approval, payment and release status", Theme.Body, Color.FromArgb(203, 213, 225));
            featureTwo.Location = new Point(42, 254);
            brand.Controls.Add(featureTwo);

            var featureThree = Ui.TextBlock("Print a verified student ID card", Theme.Body, Color.FromArgb(203, 213, 225));
            featureThree.Location = new Point(42, 282);
            brand.Controls.Add(featureThree);

            var version = Ui.TextBlock(
                "Version 1.0  -  School year " + SchoolProfile.SchoolYear,
                Theme.Tiny,
                Theme.SidebarText);
            version.Location = new Point(42, 512);
            brand.Controls.Add(version);

            Controls.Add(brand);

            // Sign-in form
            var title = Ui.TextBlock("Welcome back", Theme.H1, Theme.Text);
            title.Location = new Point(454, 92);
            Controls.Add(title);

            var subtitle = Ui.TextBlock("Sign in with your student number or registrar account.", Theme.Body, Theme.Muted);
            subtitle.Location = new Point(456, 132);
            Controls.Add(subtitle);

            var usernameLabel = Ui.TextBlock("USERNAME", new Font("Segoe UI Semibold", 8f, FontStyle.Bold), Theme.Muted);
            usernameLabel.Location = new Point(456, 180);
            Controls.Add(usernameLabel);

            _usernameBox = Ui.InputBox();
            _usernameBox.Location = new Point(456, 202);
            _usernameBox.Size = new Size(376, 30);
            _usernameBox.MaxLength = 50;
            Controls.Add(_usernameBox);

            var passwordLabel = Ui.TextBlock("PASSWORD", new Font("Segoe UI Semibold", 8f, FontStyle.Bold), Theme.Muted);
            passwordLabel.Location = new Point(456, 252);
            Controls.Add(passwordLabel);

            _passwordBox = Ui.InputBox();
            _passwordBox.Location = new Point(456, 274);
            _passwordBox.Size = new Size(376, 30);
            _passwordBox.MaxLength = 100;
            _passwordBox.UseSystemPasswordChar = true;
            Controls.Add(_passwordBox);

            var showPassword = new CheckBox
            {
                Text = "Show password",
                Font = Theme.Small,
                ForeColor = Theme.Muted,
                AutoSize = true,
                Location = new Point(456, 312),
                BackColor = Color.Transparent
            };
            showPassword.CheckedChanged += delegate { _passwordBox.UseSystemPasswordChar = !showPassword.Checked; };
            Controls.Add(showPassword);

            _errorLabel = new Label
            {
                Location = new Point(456, 338),
                Size = new Size(376, 22),
                Font = Theme.Small,
                ForeColor = Theme.Danger,
                AutoEllipsis = true,
                Text = string.Empty,
                BackColor = Color.Transparent
            };
            Controls.Add(_errorLabel);

            _signInButton = Ui.PrimaryButton("Sign in");
            _signInButton.Location = new Point(456, 366);
            _signInButton.Size = new Size(376, 46);
            _signInButton.Click += SignInClicked;
            Controls.Add(_signInButton);

            var hint = new Panel
            {
                Location = new Point(456, 436),
                Size = new Size(376, 74),
                BackColor = Theme.Field,
                BorderStyle = BorderStyle.None
            };
            var hintTitle = Ui.TextBlock("Demo accounts", Theme.BodyStrong, Theme.Text);
            hintTitle.Location = new Point(14, 12);
            hint.Controls.Add(hintTitle);
            var hintRegistrar = Ui.TextBlock("Registrar: admin / admin123", Theme.Small, Theme.Muted);
            hintRegistrar.Location = new Point(14, 32);
            hint.Controls.Add(hintRegistrar);

            var hintStudent = Ui.TextBlock("Student: 2024-0001 / student123", Theme.Small, Theme.Muted);
            hintStudent.Location = new Point(14, 52);
            hint.Controls.Add(hintStudent);
            Controls.Add(hint);

            AcceptButton = _signInButton;
            ActiveControl = _usernameBox;

            ResumeLayout(false);
        }

        private static Panel MakeRule(int x, int y, int width)
        {
            return new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, 1),
                BackColor = Color.FromArgb(51, 65, 85)
            };
        }

        private void SignInClicked(object sender, EventArgs e)
        {
            _errorLabel.Text = string.Empty;
            _signInButton.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                SignedInSession = AuthService.Login(_usernameBox.Text, _passwordBox.Text);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (InvalidOperationException ex)
            {
                _errorLabel.Text = ex.Message;
                _passwordBox.SelectAll();
                _passwordBox.Focus();
            }
            catch (Exception ex)
            {
                _errorLabel.Text = "Unable to sign in: " + ex.Message;
            }
            finally
            {
                _signInButton.Enabled = true;
                Cursor = Cursors.Default;
            }
        }
    }
}
