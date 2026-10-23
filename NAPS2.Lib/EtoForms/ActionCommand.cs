using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms;

public class ActionCommand : Command
{
    public ActionCommand()
    {
    }

    public ActionCommand(Action action) : base((sender, args) => action())
    {
    }

    public ActionCommand(Func<Task> action) : base(async (sender, args) => await action())
    {
    }

    public string Text
    {
        get => string.IsNullOrEmpty(ToolBarText) ? MenuText : ToolBarText;
        set
        {
            ToolBarText = value;
            MenuText = value;
            TextChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? TextChanged;

    public string? IconName { get; set; }

    public Image? GetIconImage(IconScenario scenario, float scale)
    {
        if (Image != null)
        {
            return Image.Clone().ResizeTo((int) (scenario.ToSize() * scale));
        }
        return IconName != null ? EtoPlatform.Current.IconProvider.GetIcon(IconName, scenario, scale) : null;
    }
}