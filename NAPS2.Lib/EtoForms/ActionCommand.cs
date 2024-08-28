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

    public Image? GetIconImage(float scale) =>
        IconName != null ? EtoPlatform.Current.IconProvider.GetIcon(IconName, scale) : null;
}