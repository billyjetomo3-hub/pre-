using System;
using System.Drawing;
using System.Windows.Forms;

namespace SchoolIdReplacement.UI
{
    /// <summary>
    /// Small factory for the controls and layout pieces used by every screen.
    /// Screens are built from docked rows instead of absolute positions so that
    /// nothing overlaps or gets clipped when a window is resized.
    /// </summary>
    internal static class Ui
    {
        // ---------------------------------------------------------------- text

        public static Label TextBlock(string text, Font font, Color color)
        {
            return new Label
            {
                Text = text,
                Font = font,
                ForeColor = color,
                AutoSize = true,
                BackColor = Color.Transparent
            };
        }

        /// <summary>Single line label that fills its cell and ends with an ellipsis.</summary>
        public static Label Line(string text, Font font, Color color)
        {
            return new Label
            {
                Text = text,
                Font = font,
                ForeColor = color,
                BackColor = Color.Transparent,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
                Margin = new Padding(0)
            };
        }

        /// <summary>Label that wraps inside its cell instead of growing past it.</summary>
        public static Label Block(string text, Font font, Color color)
        {
            return new Label
            {
                Text = text,
                Font = font,
                ForeColor = color,
                BackColor = Color.Transparent,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                Margin = new Padding(0)
            };
        }

        /// <summary>Small uppercase style caption used above values and fields.</summary>
        public static Label Caption(string text)
        {
            return Line(text, Theme.Caption, Theme.Muted);
        }

        // --------------------------------------------------------------- input

        public static TextBox InputBox()
        {
            return new TextBox
            {
                Font = Theme.Input,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                ForeColor = Theme.Text
            };
        }

        public static TextBox MemoBox()
        {
            var box = InputBox();
            box.Multiline = true;
            box.ScrollBars = ScrollBars.Vertical;
            box.Height = 110;
            return box;
        }

        public static ComboBox Dropdown(string[] items)
        {
            var combo = new ComboBox
            {
                Font = Theme.Input,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Theme.Text
            };

            if (items != null && items.Length > 0)
            {
                combo.Items.AddRange(items);
                combo.SelectedIndex = 0;
            }

            return combo;
        }

        public static DataGridView DataGrid()
        {
            var grid = new DataGridView
            {
                BackgroundColor = Theme.Surface,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Theme.Border,
                EnableHeadersVisualStyles = false,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AllowUserToOrderColumns = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeight = 42,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                RowTemplate = { Height = 40 },
                Font = Theme.Body,
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };

            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Theme.Surface,
                ForeColor = Theme.Muted,
                SelectionBackColor = Theme.Surface,
                SelectionForeColor = Theme.Muted,
                Font = new Font("Segoe UI Semibold", 8.75f, FontStyle.Bold),
                Padding = new Padding(8, 0, 0, 0),
                Alignment = DataGridViewContentAlignment.MiddleLeft
            };

            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Theme.Surface,
                ForeColor = Theme.Text,
                SelectionBackColor = Theme.AccentSoft,
                SelectionForeColor = Theme.Text,
                Font = Theme.Body,
                Padding = new Padding(8, 0, 0, 0),
                Alignment = DataGridViewContentAlignment.MiddleLeft
            };

            grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(250, 251, 253),
                ForeColor = Theme.Text,
                SelectionBackColor = Theme.AccentSoft,
                SelectionForeColor = Theme.Text,
                Padding = new Padding(8, 0, 0, 0)
            };

            return grid;
        }

        // ------------------------------------------------------------- buttons

        public static RoundedButton PrimaryButton(string text)
        {
            return new RoundedButton
            {
                Text = text,
                FillColor = Theme.Accent,
                HoverColor = Theme.AccentHover,
                PressedColor = Color.FromArgb(55, 48, 163),
                ForeColor = Color.White
            };
        }

        public static RoundedButton SecondaryButton(string text)
        {
            return new RoundedButton
            {
                Text = text,
                FillColor = Color.White,
                HoverColor = Theme.Field,
                PressedColor = Theme.Border,
                ForeColor = Theme.Text,
                BorderColor = Theme.Border,
                BorderWidth = 1
            };
        }

        public static RoundedButton DangerButton(string text)
        {
            return new RoundedButton
            {
                Text = text,
                FillColor = Theme.Danger,
                HoverColor = Color.FromArgb(153, 27, 27),
                PressedColor = Color.FromArgb(127, 29, 29),
                ForeColor = Color.White
            };
        }

        public static RoundedButton SuccessButton(string text)
        {
            return new RoundedButton
            {
                Text = text,
                FillColor = Theme.Success,
                HoverColor = Color.FromArgb(22, 101, 52),
                PressedColor = Color.FromArgb(20, 83, 45),
                ForeColor = Color.White
            };
        }

        public static Label Badge(string text, Color foreground, Color background)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI Semibold", 8.25f, FontStyle.Bold),
                ForeColor = foreground,
                BackColor = background,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(8, 0, 8, 0),
                Height = 24,
                Margin = new Padding(0)
            };
        }

        // ------------------------------------------------------------- layout

        /// <summary>
        /// A vertical stack that takes the full width of its parent and grows in
        /// height with the rows that are added to it.
        /// </summary>
        public static TableLayoutPanel Stack()
        {
            var stack = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 0,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            return stack;
        }

        /// <summary>Adds a full width row of a fixed height to a stack.</summary>
        public static void Row(TableLayoutPanel stack, Control control, int height)
        {
            stack.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
            stack.RowCount = stack.RowStyles.Count;

            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(0);

            stack.Controls.Add(control, 0, stack.RowCount - 1);
        }

        /// <summary>Adds empty vertical space to a stack.</summary>
        public static void Gap(TableLayoutPanel stack, int height)
        {
            Row(stack, new Panel { BackColor = Color.Transparent }, height);
        }

        /// <summary>Adds a caption plus input field pair to a stack.</summary>
        public static void Field(TableLayoutPanel stack, string caption, Control input, int gapAfter)
        {
            Row(stack, Caption(caption), 18);
            Row(stack, input, 28);

            if (gapAfter > 0)
            {
                Gap(stack, gapAfter);
            }
        }

        /// <summary>Caption above a value, used in the detail panels.</summary>
        public static Control MetaCell(string caption, Control value)
        {
            var cell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 12, 0)
            };

            cell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            cell.RowStyles.Add(new RowStyle(SizeType.Absolute, 16f));
            cell.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            cell.Controls.Add(Line(caption, Theme.Micro, Theme.Muted), 0, 0);
            cell.Controls.Add(value, 0, 1);

            return cell;
        }

        /// <summary>Horizontal strip of buttons that keeps its natural width.</summary>
        public static FlowLayoutPanel InlineBar()
        {
            return new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
        }

        /// <summary>
        /// Bottom toolbar. Buttons are added right to left, so the first button
        /// added ends up on the right edge of the card.
        /// </summary>
        public static FlowLayoutPanel ActionBar()
        {
            return new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
        }

        /// <summary>
        /// Scrollable host for tall content. Keeps every control reachable on a
        /// short screen instead of clipping the last rows.
        /// </summary>
        public static Panel ScrollHost(Control content)
        {
            var host = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Theme.Surface,
                Margin = new Padding(0)
            };

            content.Dock = DockStyle.Top;
            host.Controls.Add(content);

            return host;
        }

        // ------------------------------------------------------------ feedback

        public static void ShowError(IWin32Window owner, string message)
        {
            MessageBox.Show(owner, message, "School ID Replacement System", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void ShowInfo(IWin32Window owner, string message)
        {
            MessageBox.Show(owner, message, "School ID Replacement System", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static bool Confirm(IWin32Window owner, string message)
        {
            return MessageBox.Show(owner, message, "Please confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }
    }
}
