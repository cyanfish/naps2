using NAPS2.EtoForms;

namespace NAPS2.WinForms;

public interface IFormFactory
{
    T Create<T>() where T : IFormBase;
}