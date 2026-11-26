using System;
using System.Drawing;
using System.Windows.Forms;
using SchoolIdReplacement.Models;
using SchoolIdReplacement.Services;
using SchoolIdReplacement.UI;

namespace SchoolIdReplacement.Forms
{
    public class StudentEditForm : Form
    {
        private readonly Student _student;
        private readonly bool _isNew;
        private readonly Image _originalPhoto;

        private TextBox _studentNo;
        private TextBox _firstName;
        private TextBox _middleName;
        private TextBox _lastName;
        private TextBox _course;
        private TextBox _yearLevel;
        private TextBox _section;
        private TextBox _email;
        private TextBox _contactNo;
        private TextBox _address;
        private TextBox _password;
        private PictureBox _photoBox;
        private Label _errorLabel;

        private string _photoPath;

        public StudentEditForm(Student student)
        {
            _isNew = student == null;
            _student = student ?? new Student();
            _photoPath = _student.PhotoPath;
            _originalPhoto = PhotoStorage.Load(_photoPath);

            InitializeComponent();
            Populate();
        }

        public Student SavedStudent { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _originalPhoto != null)
            {
                _originalPhoto.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            Text = _isNew ? "Add student" : "Edit student";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = Color.White;
            Font = Theme.Body;
            ClientSize = new Size(700, 700);

            var heading = Ui.TextBlock(_isNew ? "New student record" : "Edit student record", Theme.H2, Theme.Text);
            heading.Location = new Point(28, 20);
            Controls.Add(heading);

            var subtitle = Ui.TextBlock(
                _isNew ? "A login account is created automatically using the student number." : "Changes are saved to the student record and the campus directory.",
                Theme.Small,
                Theme.Muted);
            subtitle.Location = new Point(30, 52);
            Controls.Add(subtitle);

            _studentNo = AddField("STUDENT NUMBER", 28, 92);
            _firstName = AddField("FIRST NAME", 350, 92);
            _middleName = AddField("MIDDLE NAME", 28, 154);
            _lastName = AddField("LAST NAME", 350, 154);
            _course = AddField("COURSE / PROGRAM", 28, 216);
            _yearLevel = AddField("YEAR LEVEL", 350, 216);
            _section = AddField("SECTION", 28, 278);
            _contactNo = AddField("CONTACT NUMBER", 350, 278);
            _email = AddField("EMAIL ADDRESS", 28, 340);
            _email.Width = 582;

            _address = AddField("ADDRESS", 28, 402);
            _address.Width = 582;

            if (_isNew)
            {
                _password = AddField("TEMPORARY PASSWORD", 28, 464);
                _password.UseSystemPasswordChar = true;
                _password.Text = "student123";

                var hint = Ui.TextBlock("Minimum 6 characters. Share it with the student.", Theme.Tiny, Theme.Muted);
                hint.Location = new Point(30, 520);
                Controls.Add(hint);
            }
            else
            {
                var hint = Ui.TextBlock("Forgot the password? Use \"Reset password\" on the Students screen.", Theme.Tiny, Theme.Muted);
                hint.Location = new Point(30, 472);
                Controls.Add(hint);
            }

            var photoPanel = new Panel
            {
                Location = new Point(350, 464),
                Size = new Size(266, 110),
                BackColor = Theme.Field
            };
            _photoBox = new PictureBox
            {
                Location = new Point(10, 10),
                Size = new Size(72, 90),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            photoPanel.Controls.Add(_photoBox);

            var browse = Ui.SecondaryButton("Choose photo");
            browse.Location = new Point(92, 10);
            browse.Size = new Size(120, 32);
            browse.Click += BrowsePhoto;
            photoPanel.Controls.Add(browse);

            var clear = Ui.SecondaryButton("Remove");
            clear.Location = new Point(92, 48);
            clear.Size = new Size(120, 32);
            clear.Click += delegate
            {
                _photoPath = null;
                _photoBox.Image = null;
            };
            photoPanel.Controls.Add(clear);

            Controls.Add(photoPanel);

            _errorLabel = new Label
            {
                Location = new Point(28, 588),
                Size = new Size(582, 20),
                Font = Theme.Small,
                ForeColor = Theme.Danger,
                Text = string.Empty,
                BackColor = Color.Transparent
            };
            Controls.Add(_errorLabel);

            var cancel = Ui.SecondaryButton("Cancel");
            cancel.Location = new Point(382, 618);
            cancel.Size = new Size(110, 42);
            cancel.DialogResult = DialogResult.Cancel;
            Controls.Add(cancel);

            var save = Ui.PrimaryButton("Save student");
            save.Location = new Point(504, 618);
            save.Size = new Size(130, 42);
            save.Click += SaveClicked;
            Controls.Add(save);

            AcceptButton = save;
            CancelButton = cancel;

            ResumeLayout(false);
        }

        private TextBox AddField(string label, int x, int y)
        {
            var caption = Ui.TextBlock(label, new Font("Segoe UI Semibold", 8f, FontStyle.Bold), Theme.Muted);
            caption.Location = new Point(x, y);
            Controls.Add(caption);

            var box = Ui.InputBox();
            box.Location = new Point(x, y + 20);
            box.Size = new Size(236, 30);
            Controls.Add(box);

            return box;
        }

        private void Populate()
        {
            _studentNo.Text = _student.StudentNo;
            _firstName.Text = _student.FirstName;
            _middleName.Text = _student.MiddleName;
            _lastName.Text = _student.LastName;
            _course.Text = _student.Course;
            _yearLevel.Text = _student.YearLevel;
            _section.Text = _student.Section;
            _email.Text = _student.Email;
            _contactNo.Text = _student.ContactNo;
            _address.Text = _student.Address;

            if (_originalPhoto != null)
            {
                _photoBox.Image = _originalPhoto;
            }
        }

        private void BrowsePhoto(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog
            {
                Title = "Choose the student's 1x1 photo",
                Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp"
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    _photoPath = PhotoStorage.Import(dialog.FileName, _studentNo.Text);
                    var image = PhotoStorage.Load(_photoPath);

                    if (_photoBox.Image != null && !ReferenceEquals(_photoBox.Image, _originalPhoto))
                    {
                        _photoBox.Image.Dispose();
                    }

                    _photoBox.Image = image;
                }
                catch (Exception ex)
                {
                    _errorLabel.Text = ex.Message;
                }
            }
        }

        private void SaveClicked(object sender, EventArgs e)
        {
            _errorLabel.Text = string.Empty;

            var student = _isNew ? new Student() : _student.Clone();
            student.StudentNo = _studentNo.Text.Trim();
            student.FirstName = _firstName.Text.Trim();
            student.MiddleName = _middleName.Text.Trim();
            student.LastName = _lastName.Text.Trim();
            student.Course = _course.Text.Trim();
            student.YearLevel = _yearLevel.Text.Trim();
            student.Section = _section.Text.Trim();
            student.Email = _email.Text.Trim();
            student.ContactNo = _contactNo.Text.Trim();
            student.Address = _address.Text.Trim();
            student.PhotoPath = _photoPath;

            try
            {
                Cursor = Cursors.WaitCursor;

                if (_isNew)
                {
                    StudentService.Create(student, _password.Text);
                }
                else
                {
                    StudentService.Update(student);
                }

                SavedStudent = student;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (InvalidOperationException ex)
            {
                _errorLabel.Text = ex.Message;
                _errorLabel.ForeColor = Theme.Danger;
            }
            catch (Exception ex)
            {
                _errorLabel.Text = "The student could not be saved: " + ex.Message;
                _errorLabel.ForeColor = Theme.Danger;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
    }
}
