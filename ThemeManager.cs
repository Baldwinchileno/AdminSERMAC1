using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public static class ThemeManager
{
    public static class Colors
    {
        public static class Light
        {
            public static Color Background = Color.White;
            public static Color Text = Color.Black;
            public static Color Primary = Color.FromArgb(0, 122, 204);
            public static Color Secondary = Color.FromArgb(108, 117, 125);
            public static Color Success = Color.FromArgb(40, 167, 69);
            public static Color Danger = Color.FromArgb(220, 53, 69);
            public static Color Warning = Color.FromArgb(255, 193, 7);
            public static Color Info = Color.FromArgb(23, 162, 184);
            public static Color GridHeader = Color.FromArgb(240, 240, 240);
            public static Color GridAlternate = Color.FromArgb(249, 249, 249);
        }

        public static class Dark
        {
            public static Color Background = Color.FromArgb(33, 37, 41);
            public static Color Text = Color.FromArgb(248, 249, 250);
            public static Color Primary = Color.FromArgb(0, 123, 255);
            public static Color Secondary = Color.FromArgb(108, 117, 125);
            public static Color Success = Color.FromArgb(40, 167, 69);
            public static Color Danger = Color.FromArgb(220, 53, 69);
            public static Color Warning = Color.FromArgb(255, 193, 7);
            public static Color Info = Color.FromArgb(23, 162, 184);
            public static Color GridHeader = Color.FromArgb(52, 58, 64);
            public static Color GridAlternate = Color.FromArgb(44, 48, 52);
        }
    }

    private static bool isDarkMode = false;
    public static event EventHandler ThemeChanged;

    public static bool IsDarkMode
    {
        get => isDarkMode;
        set
        {
            if (isDarkMode != value)
            {
                isDarkMode = value;
                ThemeChanged?.Invoke(null, EventArgs.Empty);
                SaveThemePreference();
            }
        }
    }

    public static void ApplyTheme(Form form)
    {
        var colors = IsDarkMode ? Colors.Dark : Colors.Light;
        ApplyThemeToControl(form, colors);

        foreach (Control control in form.Controls)
        {
            ApplyThemeToControl(control, colors);
        }
    }

    private static void ApplyThemeToControl(Control control, dynamic colors)
    {
        control.BackColor = colors.Background;
        control.ForeColor = colors.Text;

        switch (control)
        {
            case Button button:
                ApplyButtonTheme(button, colors);
                break;
            case DataGridView grid:
                ApplyGridTheme(grid, colors);
                break;
            case TextBox textBox:
                ApplyTextBoxTheme(textBox, colors);
                break;
            case ComboBox comboBox:
                ApplyComboBoxTheme(comboBox, colors);
                break;
        }

        foreach (Control child in control.Controls)
        {
            ApplyThemeToControl(child, colors);
        }
    }

    private static void ApplyButtonTheme(Button button, dynamic colors)
    {
        if (button.Tag?.ToString() == "primary")
        {
            button.BackColor = colors.Primary;
            button.ForeColor = Color.White;
        }
        else if (button.Tag?.ToString() == "danger")
        {
            button.BackColor = colors.Danger;
            button.ForeColor = Color.White;
        }

        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
    }

    private static void ApplyGridTheme(DataGridView grid, dynamic colors)
    {
        grid.BackgroundColor = colors.Background;
        grid.GridColor = colors.Secondary;
        grid.DefaultCellStyle.BackColor = colors.Background;
        grid.DefaultCellStyle.ForeColor = colors.Text;
        grid.ColumnHeadersDefaultCellStyle.BackColor = colors.GridHeader;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = colors.Text;
        grid.AlternatingRowsDefaultCellStyle.BackColor = colors.GridAlternate;
        grid.EnableHeadersVisualStyles = false;
    }

    private static void ApplyTextBoxTheme(TextBox textBox, dynamic colors)
    {
        textBox.BorderStyle = BorderStyle.FixedSingle;
        if (IsDarkMode)
        {
            textBox.BackColor = Color.FromArgb(52, 58, 64);
            textBox.ForeColor = Color.White;
        }
    }

    private static void ApplyComboBoxTheme(ComboBox comboBox, dynamic colors)
    {
        if (IsDarkMode)
        {
            comboBox.BackColor = Color.FromArgb(52, 58, 64);
            comboBox.ForeColor = Color.White;
        }
    }

    private static void SaveThemePreference()
    {
        Properties.Settings.Default.IsDarkMode = IsDarkMode;
        Properties.Settings.Default.Save();
    }

    public static void LoadThemePreference()
    {
        IsDarkMode = Properties.Settings.Default.IsDarkMode;
    }
}
