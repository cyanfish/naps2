using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using NAPS2.EtoForms.Layout;
using Command = Eto.Forms.Command;

namespace NAPS2.EtoForms.Wpf;

public static class WpfExtensions
{
    public static void BindCommand(this ButtonBase button, Command command)
    {
        // Eto commands are ICommands, so this handles both clicks and the enabled state
        button.Command = command;
        AutomationProperties.SetName(button, command.ToolBarText);
    }

    public static void BindEnabled(this UIElement element, Command command)
    {
        element.IsEnabled = command.Enabled;
        command.EnabledChanged += (_, _) => element.IsEnabled = command.Enabled;
    }

    public static ContextMenu ToWpfContextMenu(this MenuProvider menuProvider, Eto.Forms.Window window) =>
        (ContextMenu) C.ContextMenu(window, menuProvider).ControlObject;
}
