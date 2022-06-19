namespace NAPS2.EtoForms;

public interface IFormStateController
{
    bool SaveFormState { get; set; }
    bool RestoreFormState { get; set; }
    string FormName { get; }
}