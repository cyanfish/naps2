using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Ui;

namespace NAPS2.EtoForms.Widgets
{
    public class FolderPath
    {
        private readonly IFormBase _form;
        private readonly DialogHelper? _dialogHelper;

        private readonly TextBox _path = new();
        private readonly Button _choose = new() { Text = UiStrings.Ellipsis };
        private LayoutVisibility? _visibility;

        public FolderPath(IFormBase form, DialogHelper? dialogHelper) 
        {
            _form = form;
            _dialogHelper = dialogHelper;
            _choose.Click += OpenPathDialog;
            _path.TextChanged += (_, _) => TextChanged?.Invoke(this, EventArgs.Empty);
        }

        public string? Text
        {
            get => _path.Text;
            set => _path.Text = value;
        }

        public event EventHandler? TextChanged;

        public static implicit operator LayoutElement(FolderPath control)
        {
            return control.AsControl();
        }

        public LayoutColumn AsControl()
        {
            return L.Column(
                L.Row(
                    _path.Scale().AlignCenter().Visible(_visibility),
                    _dialogHelper != null
                        ? _choose.Width(EtoPlatform.Current.IsGtk ? null : 40).MaxHeight(22).Visible(_visibility)
                        : C.None()
                ).SpacingAfter(2)
            );
        }
        
        private void OpenPathDialog(object? sender, EventArgs e)
        {
            string savePath;
            if (_dialogHelper!.PromptToSelectFolder(_path.Text, out savePath!) == true)
            {
                _path.Text = savePath;
            }
        }

        public FolderPath Visible(LayoutVisibility visibility)
        {
            _visibility = visibility;
            return this;
        }
    }
}
