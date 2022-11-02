using NAPS2.Ocr;

namespace NAPS2.EtoForms;

public class OcrLanguagesListViewBehavior : ListViewBehavior<Language>
{
    public OcrLanguagesListViewBehavior()
    {
        ShowLabels = true;
        Checkboxes = true;
    }

    public override string GetLabel(Language item)
    {
        return item.Name;
    }
}