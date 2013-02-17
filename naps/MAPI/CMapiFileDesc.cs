using System;
using System.Runtime.InteropServices;
using System.IO;

namespace NAPS.MAPI
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    class CMapiFileDesc
    {
        public int reserved;
        public int flags;
        public int position;
        public string path;
        public string name;
        public IntPtr type;
    }
}
