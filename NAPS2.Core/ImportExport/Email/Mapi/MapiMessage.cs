using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace NAPS2.ImportExport.Email.Mapi
{
    // A MAPI structure describing an email message and its metadata.
    // Documented at: http://msdn.microsoft.com/en-us/library/windows/desktop/dd296732%28v=vs.85%29.aspx
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal class MapiMessage
    {
        public int reserved;
        public string subject;
        public string noteText;
        public string messageType;
        public string dateReceived;
        public string conversationID;
        public int flags;
        public IntPtr originator;
        public int recipCount;
        public IntPtr recips;
        public int fileCount;
        public IntPtr files;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal class MapiMessageW
    {
        public int reserved;
        public string subject;
        public string noteText;
        public string messageType;
        public string dateReceived;
        public string conversationID;
        public int flags;
        public IntPtr originator;
        public int recipCount;
        public IntPtr recips;
        public int fileCount;
        public IntPtr files;
    }
}
