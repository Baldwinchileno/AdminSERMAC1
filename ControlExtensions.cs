using System.Collections.Generic;

public static class ControlExtensions
{
    public static T WithTooltip<T>(this T control, string message) where T : Control
    {
        UIHelper.AddTooltip(control, message);
        return control;
    }

    public static TextBox WithAutoComplete(this TextBox textBox, IEnumerable<string> source)
    {
        UIHelper.SetupAutoComplete(textBox, source);
        return textBox;
    }
}