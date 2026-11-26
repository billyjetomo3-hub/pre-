using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SchoolIdReplacement.Data;
using SchoolIdReplacement.Forms;
using SchoolIdReplacement.Models;

namespace SchoolIdReplacement
{
    internal static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [STAThread]
        private static void Main()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                try { SetProcessDPIAware(); } catch { }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += delegate(object sender, System.Threading.ThreadExceptionEventArgs args)
            {
                ShowFatal(args.Exception);
            };
            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs args)
            {
                ShowFatal(args.ExceptionObject as Exception);
            };

            try
            {
                Database.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "The system could not prepare its database.\r\n\r\n" + ex.Message,
                    "Database error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            while (true)
            {
                Session session;
                using (var login = new LoginForm())
                {
                    if (login.ShowDialog() != DialogResult.OK || login.SignedInSession == null)
                    {
                        return;
                    }
                    session = login.SignedInSession;
                    Session.Current = session;
                }

                using (var main = new MainForm(session))
                {
                    Application.Run(main);
                }

                if (!Session.SignOutRequested)
                {
                    return;
                }
                Session.SignOutRequested = false;
                Session.Current = null;
            }
        }

        private static void ShowFatal(Exception exception)
        {
            if (exception == null)
            {
                return;
            }

            MessageBox.Show(
                "An unexpected error occurred:\r\n\r\n" + exception.Message,
                "School ID Replacement System",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
