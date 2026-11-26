using NAPS2.EtoForms.Ui;

namespace NAPS2.EtoForms;

public interface IImageEditorFactory
{
    T Create<T>() where T : ImageEditor;
}
