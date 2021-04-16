namespace NAPS2.EtoForms
{
    public interface IEtoPlatform
    {
        IListView<T> CreateListView<T>(ListViewBehavior<T> behavior);
    }
}