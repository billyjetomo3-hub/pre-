using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using SchoolIdReplacement.Models;
using SchoolIdReplacement.Services;
using SchoolIdReplacement.UI;

namespace SchoolIdReplacement.Forms
{
    public class IdCardForm : Form
    {
        private readonly Student _student;
        private readonly Image _photo;
        private readonly Bitmap _card;

        public IdCardForm(int studentId)
        {
            _student = StudentService.Get(studentId);
            if (_student == null)
            {
                throw new InvalidOperationException("The student record could not be found.");
            }

            _photo = PhotoStorage.Load(_student.PhotoPath);
            _card = IdCardRenderer.Render(_student, _photo);

            InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_card != null)
                {
                    _card.Dispose();
                }
                if (_photo != null)
                {
                    _photo.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            Text = "Student ID card - " + _student.StudentNo;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = Color.White;
            Font = Theme.Body;
            ClientSize = new Size(792, 552);

            var heading = Ui.TextBlock("Student ID card", Theme.H2, Theme.Text);
            heading.Location = new Point(28, 22);
            Controls.Add(heading);

            var subtitle = Ui.TextBlock(
                _student.FullName + "  -  " + _student.StudentNo + "  -  " + _student.CourseLine,
                Theme.Small,
                Theme.Muted);
            subtitle.Location = new Point(30, 54);
            Controls.Add(subtitle);

            var preview = new CardPanel
            {
                Location = new Point(28, 88),
                Size = new Size(736, 400),
                Padding = new Padding(16)
            };
            var picture = new PictureBox
            {
                Dock = DockStyle.Fill,
                Image = _card,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Theme.Surface
            };
            preview.Controls.Add(picture);
            Controls.Add(preview);

            var print = Ui.PrimaryButton("Print card");
            print.Location = new Point(28, 500);
            print.Size = new Size(150, 42);
            print.Click += PrintClicked;
            Controls.Add(print);

            var previewButton = Ui.SecondaryButton("Print preview");
            previewButton.Location = new Point(190, 500);
            previewButton.Size = new Size(150, 42);
            previewButton.Click += PreviewClicked;
            Controls.Add(previewButton);

            var close = Ui.SecondaryButton("Close");
            close.Location = new Point(614, 500);
            close.Size = new Size(150, 42);
            close.DialogResult = DialogResult.Cancel;
            Controls.Add(close);

            CancelButton = close;

            ResumeLayout(false);
        }

        private void PrintClicked(object sender, EventArgs e)
        {
            using (var document = CreateDocument())
            using (var dialog = new PrintDialog { Document = document, UseEXDialog = true })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        document.Print();
                        Ui.ShowInfo(this, "The ID card was sent to the printer.");
                    }
                    catch (Exception ex)
                    {
                        Ui.ShowError(this, "The card could not be printed.\r\n\r\n" + ex.Message);
                    }
                }
            }
        }

        private void PreviewClicked(object sender, EventArgs e)
        {
            using (var document = CreateDocument())
            using (var preview = new PrintPreviewDialog
            {
                Document = document,
                Width = 900,
                Height = 700,
                StartPosition = FormStartPosition.CenterParent,
                Text = "ID card preview"
            })
            {
                preview.ShowDialog(this);
            }
        }

        private PrintDocument CreateDocument()
        {
            var document = new PrintDocument();
            document.DocumentName = "Student ID - " + _student.StudentNo;
            document.DefaultPageSettings.Margins = new Margins(60, 60, 60, 60);
            document.PrintPage += PrintPage;
            return document;
        }

        private void PrintPage(object sender, PrintPageEventArgs e)
        {
            var width = IdCardRenderer.CardWidthInches * 100f;
            var height = IdCardRenderer.CardHeightInches * 100f;
            var bounds = new RectangleF(e.MarginBounds.X, e.MarginBounds.Y, width, height);
            IdCardRenderer.Draw(e.Graphics, bounds, _student, _photo, false);
            e.HasMorePages = false;
        }
    }
}
