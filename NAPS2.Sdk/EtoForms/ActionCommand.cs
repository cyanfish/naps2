using Eto.Forms;

namespace NAPS2.EtoForms;

public class ActionCommand : Command
{
    public ActionCommand(Action action) : base((sender, args) => action())
    {
    }
}