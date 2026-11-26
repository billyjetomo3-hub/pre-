using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using SchoolIdReplacement.Data;
using SchoolIdReplacement.Models;
using SchoolIdReplacement.Services;
using SchoolIdReplacement.UI;

namespace SchoolIdReplacement.Views
{
    public class SettingsView : UserControl
    {
        private readonly Session _session;

        private TextBox _currentPassword;
        private TextBox _newPassword;
        private TextBox _confirmPassword;
        private Label _messageLabel;

        public SettingsView(Session session)
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
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // ------------------------------------------------------ Password card
            var passwordCard = new CardPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 14, 0),
                Padding = new Padding(24)
            };

            var form = Ui.Stack();
            Ui.Row(form, Ui.Line("Change password", Theme.H3, Theme.Text), 28);

            var passwordSubtitle = Ui.Block("Use at least 6 characters and keep it private.", Theme.Small, Theme.Muted);
            Ui.Row(form, passwordSubtitle, 20);

            Ui.Gap(form, 14);

            _currentPassword = Ui.InputBox();
            _currentPassword.UseSystemPasswordChar = true;
            Ui.Field(form, "CURRENT PASSWORD", _currentPassword, 12);

            _newPassword = Ui.InputBox();
            _newPassword.UseSystemPasswordChar = true;
            Ui.Field(form, "NEW PASSWORD", _newPassword, 12);

            _confirmPassword = Ui.InputBox();
            _confirmPassword.UseSystemPasswordChar = true;
            Ui.Field(form, "CONFIRM NEW PASSWORD", _confirmPassword, 10);

            _messageLabel = Ui.Block(string.Empty, Theme.Small, Theme.Success);
            Ui.Row(form, _messageLabel, 40);

            Ui.Gap(form, 4);

            var saveBar = Ui.InlineBar();
            var save = Ui.PrimaryButton("Update password");
            save.Size = new Size(180, 42);
            save.Margin = new Padding(0);
            save.Click += ChangePasswordClicked;
            saveBar.Controls.Add(save);
            Ui.Row(form, saveBar, 42);

            passwordCard.Controls.Add(Ui.ScrollHost(form));
            root.Controls.Add(passwordCard, 0, 0);

            // -------------------------------------------------------- System card
            var systemCard = new CardPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(14, 0, 0, 0),
                Padding = new Padding(24)
            };

            var info = Ui.Stack();
            Ui.Row(info, Ui.Line("System information", Theme.H3, Theme.Text), 28);
            Ui.Gap(info, 10);

            var rows = new[]
            {
                new[] { "SIGNED IN AS", _session.FullName + " (" + _session.Username + ")" },
                new[] { "ROLE", _session.Role },
                new[] { "SCHOOL", SchoolProfile.Name },
                new[] { "SCHOOL YEAR", SchoolProfile.SchoolYear },
                new[] { "DATABASE SERVER", SafeServer() },
                new[] { "DATABASE NAME", SafeDatabase() },
                new[] { "APPLICATION", "School ID Replacement System 1.0" }
            };

            for (var index = 0; index < rows.Length; index++)
            {
                Ui.Row(info, Ui.Caption(rows[index][0]), 16);
                Ui.Row(info, Ui.Line(rows[index][1], Theme.BodyStrong, Theme.Text), 22);
                Ui.Gap(info, 6);
            }

            Ui.Gap(info, 6);

            var folderBar = Ui.InlineBar();
            var folder = Ui.SecondaryButton("Open photo folder");
            folder.Size = new Size(180, 38);
            folder.Margin = new Padding(0);
            folder.Click += delegate
            {
                try
                {
                    System.IO.Directory.CreateDirectory(PhotoStorage.Folder);
                    Process.Start("explorer.exe", "\"" + PhotoStorage.Folder + "\"");
                }
                catch (Exception ex)
                {
                    Ui.ShowError(this, ex.Message);
                }
            };
            folderBar.Controls.Add(folder);
            Ui.Row(info, folderBar, 38);

            systemCard.Controls.Add(Ui.ScrollHost(info));
            root.Controls.Add(systemCard, 1, 0);

            Controls.Add(root);

            ResumeLayout(false);
        }

        private static string SafeServer()
        {
            try
            {
                return Database.ServerName;
            }
            catch
            {
                return "Unavailable";
            }
        }

        private static string SafeDatabase()
        {
            try
            {
                return Database.DatabaseName;
            }
            catch
            {
                return "Unavailable";
            }
        }

        private void ChangePasswordClicked(object sender, EventArgs e)
        {
            _messageLabel.ForeColor = Theme.Danger;
            _messageLabel.Text = string.Empty;

            if (_newPassword.Text != _confirmPassword.Text)
            {
                _messageLabel.Text = "The new password and the confirmation do not match.";
                return;
            }

            try
            {
                AuthService.ChangePassword(_session.UserId, _currentPassword.Text, _newPassword.Text);

                _messageLabel.ForeColor = Theme.Success;
                _messageLabel.Text = "Your password was updated successfully.";
                _currentPassword.Clear();
                _newPassword.Clear();
                _confirmPassword.Clear();
            }
            catch (InvalidOperationException ex)
            {
                _messageLabel.Text = ex.Message;
            }
            catch (Exception ex)
            {
                _messageLabel.Text = "The password could not be changed: " + ex.Message;
            }
        }
    }
}
