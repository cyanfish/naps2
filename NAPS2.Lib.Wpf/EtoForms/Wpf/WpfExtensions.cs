using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
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

    public static T? FindDescendant<T>(this DependencyObject parent, Func<T, bool>? predicate = null)
        where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T match && (predicate == null || predicate(match)))
            {
                return match;
            }
            if (child.FindDescendant(predicate) is { } descendant)
            {
                return descendant;
            }
        }
        return null;
    }
}
