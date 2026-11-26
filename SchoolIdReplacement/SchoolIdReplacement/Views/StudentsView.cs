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
    public class StudentsView : UserControl
    {
        private readonly Session _session;

        private DataGridView _grid;
        private TextBox _searchBox;
        private Label _summaryLabel;

        public StudentsView(Session session)
        {
            _session = session;
            InitializeComponent();
            LoadStudents();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            BackColor = Theme.Background;
            Dock = DockStyle.Fill;

            var card = new CardPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                Padding = new Padding(20)
            };

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = Theme.Surface,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 48f));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 10f));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 12f));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 48f));

            // -------------------------------------------------------------- Header
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
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 340f));
            header.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var titles = Ui.Stack();
            Ui.Row(titles, Ui.Line("Student records", Theme.H3, Theme.Text), 24);
            _summaryLabel = Ui.Line(string.Empty, Theme.Tiny, Theme.Muted);
            Ui.Row(titles, _summaryLabel, 20);
            header.Controls.Add(titles, 0, 0);

            var searchArea = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 8, 0, 8),
                Padding = new Padding(0)
            };
            searchArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            searchArea.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96f));
            searchArea.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            _searchBox = Ui.InputBox();
            _searchBox.Dock = DockStyle.Fill;
            _searchBox.Margin = new Padding(0, 0, 8, 0);
            _searchBox.KeyDown += delegate(object sender, KeyEventArgs args)
            {
                if (args.KeyCode == Keys.Enter)
                {
                    args.SuppressKeyPress = true;
                    LoadStudents();
                }
            };
            searchArea.Controls.Add(_searchBox, 0, 0);

            var search = Ui.PrimaryButton("Search");
            search.Dock = DockStyle.Fill;
            search.Margin = new Padding(0);
            search.Size = new Size(96, 32);
            search.Click += delegate { LoadStudents(); };
            searchArea.Controls.Add(search, 1, 0);

            header.Controls.Add(searchArea, 1, 0);
            content.Controls.Add(header, 0, 0);

            // ---------------------------------------------------------------- Grid
            _grid = Ui.DataGrid();
            _grid.CellDoubleClick += delegate { EditStudent(); };
            content.Controls.Add(_grid, 0, 2);

            // -------------------------------------------------------- Action bar
            var actions = Ui.ActionBar();
            actions.Padding = new Padding(0, 6, 0, 6);

            var add = Ui.PrimaryButton("Add student");
            add.Size = new Size(120, 36);
            add.Margin = new Padding(8, 0, 0, 0);
            add.Click += delegate { AddStudent(); };
            actions.Controls.Add(add);

            var edit = Ui.SecondaryButton("Edit");
            edit.Size = new Size(84, 36);
            edit.Margin = new Padding(8, 0, 0, 0);
            edit.Click += delegate { EditStudent(); };
            actions.Controls.Add(edit);

            var reset = Ui.SecondaryButton("Reset password");
            reset.Size = new Size(130, 36);
            reset.Margin = new Padding(8, 0, 0, 0);
            reset.Click += delegate { ResetPassword(); };
            actions.Controls.Add(reset);

            var toggle = Ui.SecondaryButton("Activate / deactivate");
            toggle.Size = new Size(166, 36);
            toggle.Margin = new Padding(8, 0, 0, 0);
            toggle.Click += delegate { ToggleActive(); };
            actions.Controls.Add(toggle);

            var print = Ui.SecondaryButton("ID card");
            print.Size = new Size(98, 36);
            print.Margin = new Padding(8, 0, 0, 0);
            print.Click += delegate { ShowCard(); };
            actions.Controls.Add(print);

            var refresh = Ui.SecondaryButton("Refresh");
            refresh.Size = new Size(90, 36);
            refresh.Margin = new Padding(8, 0, 0, 0);
            refresh.Click += delegate { LoadStudents(); };
            actions.Controls.Add(refresh);

            content.Controls.Add(actions, 0, 4);

            card.Controls.Add(content);
            Controls.Add(card);

            ResumeLayout(false);
        }

        private void LoadStudents()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                var table = StudentService.List(_searchBox.Text);
                _grid.DataSource = table;

                var idColumn = _grid.Columns["Id"];
                if (idColumn != null)
                {
                    idColumn.Visible = false;
                }

                _summaryLabel.Text = table.Rows.Count + " student record(s). Double-click a row to edit it.";
            }
            catch (Exception ex)
            {
                Ui.ShowError(this, "The student list could not be loaded.\r\n\r\n" + ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private int? SelectedStudentId()
        {
            if (_grid == null || _grid.CurrentRow == null)
            {
                return null;
            }

            var view = _grid.CurrentRow.DataBoundItem as DataRowView;
            return view == null ? (int?)null : Convert.ToInt32(view["Id"]);
        }

        private Student SelectedStudent()
        {
            var id = SelectedStudentId();
            return id.HasValue ? StudentService.Get(id.Value) : null;
        }

        private void AddStudent()
        {
            try
            {
                using (var dialog = new StudentEditForm(null))
                {
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        LoadStudents();
                        Ui.ShowInfo(this, "Student added. The account username is the student number and the temporary password was set in the form.");
                    }
                }
            }
            catch (Exception ex)
            {
                Ui.ShowError(this, ex.Message);
            }
        }

        private void EditStudent()
        {
            var student = SelectedStudent();
            if (student == null)
            {
                Ui.ShowError(this, "Please select a student first.");
                return;
            }

            try
            {
                using (var dialog = new StudentEditForm(student))
                {
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        LoadStudents();
                    }
                }
            }
            catch (Exception ex)
            {
                Ui.ShowError(this, ex.Message);
            }
        }

        private void ToggleActive()
        {
            var student = SelectedStudent();
            if (student == null)
            {
                Ui.ShowError(this, "Please select a student first.");
                return;
            }

            var activate = !student.IsActive;
            if (!Ui.Confirm(this, (activate ? "Reactivate " : "Deactivate ") + student.FullName + "?"))
            {
                return;
            }

            try
            {
                StudentService.SetActive(student.Id, activate);
                LoadStudents();
            }
            catch (Exception ex)
            {
                Ui.ShowError(this, ex.Message);
            }
        }

        private void ResetPassword()
        {
            var student = SelectedStudent();
            if (student == null)
            {
                Ui.ShowError(this, "Please select a student first.");
                return;
            }

            var password = PromptDialog.Ask(
                this,
                "Reset password",
                "Set a new temporary password for " + student.FullName + " (student number " + student.StudentNo + ").",
                "student123",
                false,
                true,
                true);

            if (password == null)
            {
                return;
            }

            try
            {
                StudentService.ResetPassword(student.Id, password);
                Ui.ShowInfo(this, "The password was reset. Tell the student to change it after signing in.");
            }
            catch (Exception ex)
            {
                Ui.ShowError(this, ex.Message);
            }
        }

        private void ShowCard()
        {
            var student = SelectedStudent();
            if (student == null)
            {
                Ui.ShowError(this, "Please select a student first.");
                return;
            }

            try
            {
                using (var form = new IdCardForm(student.Id))
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
