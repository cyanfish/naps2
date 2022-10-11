namespace NAPS2.EtoForms;

public interface IFormFactory
{
    T Create<T>() where T : IFormBase;
}