namespace NAPS2.WinForms
{
    public interface IFormFactory
    {
        T Create<T>() where T : FormBase;
    }
}