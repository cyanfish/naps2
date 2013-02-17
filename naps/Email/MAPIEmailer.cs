using System;
using System.Runtime.InteropServices;
using System.IO;

namespace NAPS.Email
{
    public class MAPIEmailer : IEmailer
    {
        private const int MAPI_LOGON_UI = 0x00000001;
        private const int MAPI_DIALOG = 0x00000008;

        public bool SendEmail(string attachmentFileName, string subject)
        {
            IntPtr session = new IntPtr(0);
            IntPtr winhandle = new IntPtr(0);

            MAPIMessage msg = new MAPIMessage();
            msg.subject = subject;

            int sizeofMapiDesc = Marshal.SizeOf(typeof(MAPIFileDesc));
            IntPtr pMapiDesc = Marshal.AllocHGlobal(sizeofMapiDesc);

            MAPIFileDesc fileDesc = new MAPIFileDesc();
            fileDesc.position = -1;
            int ptr = (int)pMapiDesc;

            string strPath = attachmentFileName;
            fileDesc.name = Path.GetFileName(strPath);
            fileDesc.path = strPath;
            Marshal.StructureToPtr(fileDesc, (IntPtr)ptr, false);

            msg.files = pMapiDesc;
            msg.fileCount = 1;

            return MAPISendMail(session, winhandle, msg, MAPI_LOGON_UI | MAPI_DIALOG, 0) == 0;
        }

        [DllImport("MAPI32.DLL")]
        private static extern int MAPISendMail(IntPtr sess, IntPtr hwnd, MAPIMessage message, int flg, int rsv);
    }
}
