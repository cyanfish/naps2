using Eto.Forms;

namespace NAPS2.EtoForms;

public class DropEventArgs : EventArgs
{
    public DropEventArgs(int position, IDataObject data)
    {
        Position = position;
        Data = data;
    }

    public int Position { get; }
        
    public IDataObject Data { get; }
}