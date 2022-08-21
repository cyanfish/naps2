namespace NAPS2.Util;

public static class MacExtensions
{
    public static T WithAction<T>(this T control, Action action) where T : NSControl
    {
        control.Target = new ActionTarget(action);
        control.Action = new Selector("action");
        return control;
    }

    public static T WithAction<T>(this T control, Action<T> action) where T : NSControl
    {
        control.Target = new ActionTarget(() => action(control));
        control.Action = new Selector("action");
        return control;
    }

    public static NSToolbarItem WithAction(this NSToolbarItem control, Action action)
    {
        control.Target = new ActionTarget(action);
        control.Action = new Selector("action");
        return control;
    }

    public static NSMenuItem WithAction(this NSMenuItem control, Action action)
    {
        control.Target = new ActionTarget(action);
        control.Action = new Selector("action");
        return control;
    }

    public class ActionTarget : NSObject
    {
        private readonly Action _action;

        public ActionTarget(Action action)
        {
            _action = action;
        }

        [Export("action")]
        public void Action()
        {
            _action();
        }
    }
}