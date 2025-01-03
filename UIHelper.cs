using System.Collections.Generic;
using System.Windows.Forms;

public class UIHelper
{
    public static void AddTooltip(Control control, string message)
    {
        var tooltip = new ToolTip
        {
            InitialDelay = 500,
            ReshowDelay = 100,
            ShowAlways = true,
            ToolTipTitle = string.Empty,
            UseAnimation = true,
            UseFading = true,
            IsBalloon = true
        };

        tooltip.SetToolTip(control, message);
    }

    public static void SetupAutoComplete(TextBox textBox, IEnumerable<string> source)
    {
        var autoComplete = new AutoCompleteStringCollection();
        autoComplete.AddRange(source.ToArray());

        textBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        textBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
        textBox.AutoCompleteCustomSource = autoComplete;
    }

    public static void SetupKeyboardShortcuts(Form form)
    {
        // Guardar - Ctrl+S
        form.KeyPreview = true;
        form.KeyDown += (s, e) =>
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                var saveButton = form.Controls.Find("saveButton", true).FirstOrDefault();
                if (saveButton != null && saveButton.Enabled)
                {
                    saveButton.PerformClick();
                    e.Handled = true;
                }
            }
        };

        // Otros atajos según sea necesario
    }

    public static void MakeFormResponsive(Form form)
    {
        form.Resize += (s, e) =>
        {
            // Ajustar tamaños y posiciones según el nuevo tamaño del formulario
            foreach (Control control in form.Controls)
            {
                AdjustControlSize(control, form.ClientSize);
            }
        };
    }

    private static void AdjustControlSize(Control control, Size formSize)
    {
        if (control.Tag is string tag)
        {
            var parts = tag.Split(',');
            foreach (var part in parts)
            {
                var keyValue = part.Trim().Split('=');
                if (keyValue.Length == 2)
                {
                    var key = keyValue[0].Trim();
                    var value = keyValue[1].Trim();

                    switch (key)
                    {
                        case "width":
                            if (value.EndsWith("%"))
                            {
                                var percentage = float.Parse(value.TrimEnd('%')) / 100;
                                control.Width = (int)(formSize.Width * percentage);
                            }
                            break;

                        case "height":
                            if (value.EndsWith("%"))
                            {
                                var percentage = float.Parse(value.TrimEnd('%')) / 100;
                                control.Height = (int)(formSize.Height * percentage);
                            }
                            break;
                    }
                }
            }
        }
    }
}