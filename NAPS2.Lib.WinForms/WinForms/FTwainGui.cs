namespace NAPS2.WinForms;

// TODO: Do we want to migrate this to Eto? Ideally we can figure out why Twain Legacy is needed for some circumstances
// and remove this form entirely...
internal partial class FTwainGui : FormBase
{
    public FTwainGui()
    {
        InitializeComponent();
        SaveFormState = false;
        RestoreFormState = false;
        label1.Text = UiStrings.WaitingForTwain;
    }
}