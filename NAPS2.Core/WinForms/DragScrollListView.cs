using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    public class DragScrollListView : ListView
    {
        // From http://stackoverflow.com/questions/660663/c-sharp-implementing-auto-scroll-in-a-listview-while-drag-dropping

        private Timer tmrLVScroll;
        private System.ComponentModel.IContainer components;
        private int mintScrollDirection;

        const int WM_VSCROLL = 277; // Vertical scroll
        const int SB_LINEUP = 0; // Scrolls one line up
        const int SB_LINEDOWN = 1; // Scrolls one line down

        public DragScrollListView()
        {
            InitializeComponent();
        }

        private int EdgeSize => Font.Height;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tmrLVScroll = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // tmrLVScroll
            // 
            this.tmrLVScroll.Tick += new System.EventHandler(this.tmrLVScroll_Tick);
            // 
            // ListViewBase
            // 
            this.DragOver += new System.Windows.Forms.DragEventHandler(this.ListViewBase_DragOver);
            this.ResumeLayout(false);

        }

        private void ListViewBase_DragOver(object sender, DragEventArgs e)
        {
            Point position = PointToClient(new Point(e.X, e.Y));

            if (position.Y <= EdgeSize)
            {
                // getting close to top, ensure previous item is visible
                mintScrollDirection = SB_LINEUP;
                tmrLVScroll.Enabled = true;
            }
            else if (position.Y >= ClientSize.Height - EdgeSize)
            {
                // getting close to bottom, ensure next item is visible
                mintScrollDirection = SB_LINEDOWN;
                tmrLVScroll.Enabled = true;
            }
            else
            {
                tmrLVScroll.Enabled = false;
            }
        }

        private void tmrLVScroll_Tick(object sender, EventArgs e)
        {
            Win32.SendMessage(Handle, WM_VSCROLL, (IntPtr)mintScrollDirection, IntPtr.Zero);
        }
    }
}
