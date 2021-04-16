using System;
using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;
using NAPS2.Images;

namespace NAPS2.EtoForms
{
    public interface IListView<T> : Images.ISelectable<T>
    {
        Control Control { get; }
        
        Size ImageSize { get; set; }
        
        event EventHandler SelectionChanged;
        
        event EventHandler ItemClicked;

        event EventHandler<DropEventArgs> Drop;
        
        bool AllowDrag { get; set; }
        
        bool AllowDrop { get; set; }

        ListSelection<T> Selection { get; set; }

        void SetItems(IEnumerable<T> items);
    }
}
