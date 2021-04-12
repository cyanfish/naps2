using System;
using System.Diagnostics;
using System.Windows.Input;
using Eto.Forms;

namespace NAPS2.EtoForms
{
    public static class EtoHelpers
    {
        public static Label NoWrap(string text)
        {
            return new Label { Text = text, Wrap = WrapMode.None };
        }

        public static LinkButton Link(string text, Action onClick = null)
        {
            onClick ??= () => Process.Start(text);
            return new LinkButton
            {
                Text = text,
                Command = new ActionCommand(onClick)
            };
        }

        private class ActionCommand : ICommand
        {
            private readonly Action _action;

            public ActionCommand(Action action)
            {
                _action = action;
            }

            public bool CanExecute(object parameter) => true;

            public void Execute(object parameter) => _action();

            public event EventHandler CanExecuteChanged;
        }
    }
}
