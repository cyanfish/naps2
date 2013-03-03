/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2012-2013  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace NAPS2.Email
{
    public class MAPIEmailer : IEmailer
    {
        private const int MAPI_LOGON_UI = 0x00000001;
        private const int MAPI_DIALOG = 0x00000008;

        public bool SendEmail(string attachmentFileName, string subject)
        {
            var session = new IntPtr(0);
            var winhandle = new IntPtr(0);

            var msg = new MAPIMessage();
            msg.subject = subject;

            int sizeofMapiDesc = Marshal.SizeOf(typeof(MAPIFileDesc));
            IntPtr pMapiDesc = Marshal.AllocHGlobal(sizeofMapiDesc);

            var fileDesc = new MAPIFileDesc();
            fileDesc.position = -1;
            var ptr = (int)pMapiDesc;

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
