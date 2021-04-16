using NAPS2.EtoForms.WinForms;

namespace NAPS2.EtoForms
{
    public class WinFormsEtoPlatform : IEtoPlatform
    {
        public IListView<T> CreateListView<T>(ListViewBehavior<T> behavior) =>
            new WinFormsListView<T>(behavior);
    }
}