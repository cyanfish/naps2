using System;
using System.Runtime.InteropServices;
using System.IO;

namespace NAPS.MAPI
{
    class CMAPI
    {
        private const int MAPI_LOGON_UI = 0x00000001;
        private const int MAPI_DIALOG = 0x00000008;

        public static int SendMail(string strAttachmentFileName, string strSubject)
        {
            IntPtr session = new IntPtr(0);
            IntPtr winhandle = new IntPtr(0);

            CMapiMessage msg = new CMapiMessage();
            msg.subject = strSubject;

            int sizeofMapiDesc = Marshal.SizeOf(typeof(CMapiFileDesc));
            IntPtr pMapiDesc = Marshal.AllocHGlobal(sizeofMapiDesc);

            CMapiFileDesc fileDesc = new CMapiFileDesc();
            fileDesc.position = -1;
            int ptr = (int)pMapiDesc;

            string strPath = strAttachmentFileName;
            fileDesc.name = Path.GetFileName(strPath);
            fileDesc.path = strPath;
            Marshal.StructureToPtr(fileDesc, (IntPtr)ptr, false);

            msg.files = pMapiDesc;
            msg.fileCount = 1;

            return MAPISendMail(session, winhandle, msg, MAPI_LOGON_UI | MAPI_DIALOG, 0);
        }

        [DllImport("MAPI32.DLL")]
        private static extern int MAPISendMail(IntPtr sess, IntPtr hwnd,CMapiMessage message, int flg, int rsv);
    }
}
