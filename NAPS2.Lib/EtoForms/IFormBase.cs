namespace NAPS2.EtoForms;

public interface IFormBase
{
    FormStateController FormStateController { get; }

    IFormFactory FormFactory { get; set; }

    Naps2Config Config { get; set; }
}