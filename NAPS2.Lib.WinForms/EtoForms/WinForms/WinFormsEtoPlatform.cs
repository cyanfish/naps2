namespace NAPS2.EtoForms.WinForms;

public class WinFormsEtoPlatform : IEtoPlatform
{
    public IListView<T> CreateListView<T>(ListViewBehavior<T> behavior) =>
        new WinFormsListView<T>(behavior);
}