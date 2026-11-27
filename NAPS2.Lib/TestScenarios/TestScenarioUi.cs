#if TEST_SCENARIOS
using System.Globalization;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Eto.Forms;
using NAPS2.EtoForms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.TestScenarios;

/// <summary>
/// Finds, describes and manipulates the controls in a window. All methods must be called on the UI thread.
/// </summary>
internal static class TestScenarioUi
{
    /// <summary>
    /// Gets the controls in the window in layout order. Indices in this list are used to identify controls.
    /// </summary>
    public static List<Control> GetControls(Window window)
    {
        var controls = new List<Control>();
        var root = window switch
        {
            EtoFormBase form => form.LayoutController.Content,
            EtoDialogBase dialog => dialog.LayoutController.Content,
            _ => null
        };
        if (root != null)
        {
            AddControls(root, controls);
        }
        return controls;
    }

    private static void AddControls(LayoutElement element, List<Control> controls)
    {
        switch (element)
        {
            case LayoutControl { Control: { } control }:
                AddControls(control, controls);
                break;
            case LayoutContainer container:
                foreach (var child in container.Children)
                {
                    AddControls(child, controls);
                }
                break;
            case BufferLayoutElement buffer:
                AddControls(buffer.Element, controls);
                break;
        }
    }

    private static void AddControls(Control control, List<Control> controls)
    {
        controls.Add(control);
        if (control is Container container)
        {
            foreach (var child in container.VisualControls)
            {
                AddControls(child, controls);
            }
        }
    }

    public static JsonObject Describe(Control control, int index)
    {
        var obj = new JsonObject
        {
            ["index"] = index,
            ["type"] = control.GetType().Name
        };
        if (GetText(control) is { } text)
        {
            obj["text"] = text;
        }
        switch (control)
        {
            case CheckBox checkBox:
                obj["checked"] = checkBox.Checked;
                break;
            case RadioButton radioButton:
                obj["checked"] = radioButton.Checked;
                break;
            case ListControl listControl:
                obj["selectedIndex"] = listControl.SelectedIndex;
                obj["items"] = new JsonArray(GetItemTexts(listControl).Select(x => (JsonNode?) x).ToArray());
                break;
            case NumericStepper stepper:
                obj["value"] = stepper.Value;
                break;
            case Slider slider:
                obj["value"] = slider.Value;
                break;
        }
        obj["enabled"] = control.Enabled;
        obj["visible"] = control.Visible;
        var location = control.PointToScreen(Eto.Drawing.PointF.Empty);
        obj["bounds"] = new JsonArray((int) location.X, (int) location.Y, control.Width, control.Height);
        return obj;
    }

    /// <summary>
    /// Finds a control by its index (from GetControls), its text (or selected item for lists), or the text of the
    /// label before it.
    /// </summary>
    public static Control FindControl(Window window, string? text, string? label, int? index)
    {
        var controls = GetControls(window);
        if (index != null)
        {
            return index >= 0 && index < controls.Count
                ? controls[index.Value]
                : throw new ArgumentException($"No control with index {index} in window \"{window.Title}\"");
        }
        List<Control> matches;
        if (label != null)
        {
            matches = controls
                .Select((control, i) => (control, i))
                .Where(x => x.control is Label { Visible: true } && Normalize(GetText(x.control)) == Normalize(label))
                .Select(x => controls.Skip(x.i + 1).FirstOrDefault(c => c.Visible && c is not (Label or ImageView)))
                .OfType<Control>()
                .ToList();
        }
        else if (text != null)
        {
            matches = controls.Where(c => Normalize(GetText(c)) == Normalize(text)).ToList();
        }
        else
        {
            throw new ArgumentException("A control's text, label or index must be specified");
        }
        return matches.FirstOrDefault(c => c.Visible && c.Enabled)
               ?? matches.FirstOrDefault()
               ?? throw new ArgumentException(
                   $"No control with {(label != null ? "label" : "text")} \"{label ?? text}\" in window \"{window.Title}\"");
    }

    public static void Click(Control control)
    {
        switch (control)
        {
            case Button button:
                button.PerformClick();
                break;
            case LinkButton linkButton:
                // LinkButton has no PerformClick
                typeof(LinkButton).GetMethod("OnClick", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .Invoke(linkButton, [EventArgs.Empty]);
                break;
            case CheckBox checkBox:
                checkBox.Checked = !(checkBox.Checked ?? false);
                break;
            case RadioButton radioButton:
                radioButton.Checked = true;
                break;
            default:
                throw new ArgumentException($"Can't click a {control.GetType().Name}");
        }
    }

    public static void SetValue(Control control, string value)
    {
        switch (control)
        {
            case TextBox textBox:
                textBox.Text = value;
                break;
            case TextArea textArea:
                textArea.Text = value;
                break;
            case CheckBox checkBox:
                checkBox.Checked = bool.Parse(value);
                break;
            case RadioButton radioButton:
                radioButton.Checked = bool.Parse(value);
                break;
            case ListControl listControl:
                var texts = GetItemTexts(listControl);
                var itemIndex = texts.FindIndex(x => Normalize(x) == Normalize(value));
                if (itemIndex == -1 && !int.TryParse(value, out itemIndex))
                {
                    throw new ArgumentException(
                        $"No item \"{value}\" in {control.GetType().Name}; items: {string.Join(", ", texts)}");
                }
                listControl.SelectedIndex = itemIndex;
                break;
            case NumericStepper stepper:
                stepper.Value = double.Parse(value, CultureInfo.InvariantCulture);
                break;
            case Slider slider:
                slider.Value = int.Parse(value, CultureInfo.InvariantCulture);
                break;
            default:
                throw new ArgumentException($"Can't set the value of a {control.GetType().Name}");
        }
    }

    /// <summary>
    /// Gets the text shown by the control, or else its selected item or tooltip (e.g. for icon buttons).
    /// </summary>
    private static string? GetText(Control control) => control switch
    {
        TextControl { Text: { Length: > 0 } text } => text,
        ListControl { SelectedIndex: >= 0 } listControl => GetItemTexts(listControl)[listControl.SelectedIndex],
        { ToolTip: { Length: > 0 } toolTip } => toolTip,
        TextControl textControl => textControl.Text,
        _ => null
    };

    public static List<string> GetItemTexts(ListControl listControl) =>
        listControl.DataStore?.Cast<object>()
            .Select(item => listControl.ItemTextBinding?.GetValue(item) ?? item.ToString() ?? "")
            .ToList() ?? [];

    public static string Normalize(string? text) =>
        Regex.Replace((text ?? "").Replace("&", ""), @"\s+", " ").Trim().ToLowerInvariant();
}
#endif
