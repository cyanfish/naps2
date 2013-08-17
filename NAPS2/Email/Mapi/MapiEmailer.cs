/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using NAPS2.Email.Exceptions;

namespace NAPS2.Email.Mapi
{
    public class MapiEmailer : IEmailer
    {
        // MAPISendMail is documented at:
        // http://msdn.microsoft.com/en-us/library/windows/desktop/dd296721%28v=vs.85%29.aspx
        // Flags and return codes are documented at:
        // http://msdn.microsoft.com/en-us/library/windows/desktop/hh707275%28v=vs.85%29.aspx

        #region MAPISendMail flags

        private const int NONE = 0;

        /// <summary>
        /// Prompt the user for credentials if necessary.
        /// </summary>
        private const int MAPI_LOGON_UI = 1;

        /// <summary>
        /// Prompt the user to customize the message before sending.
        /// </summary>
        private const int MAPI_DIALOG = 8;

        #endregion

        #region MAPISendMail return codes

        private const int SUCCESS = 0;
        private const int MAPI_E_USER_ABORT = 1;

        #endregion

        [DllImport("MAPI32.DLL")]
        private static extern int MAPISendMail(IntPtr session, IntPtr hwnd, MapiMessage message, int flags, int reserved);

        /// <summary>
        /// Sends an email described by the given message object.
        /// </summary>
        /// <param name="message">The object describing the email message.</param>
        /// <exception cref="EmailException">Throws an EmailException if an error occurred.</exception>
        /// <returns>Returns true if the message was sent, false if the user aborted.</returns>
        public bool SendEmail(EmailMessage message)
        {
            // Translate files & recipients to unmanaged MAPI structures
            var files = message.AttachmentFilePaths.Select(path => new MapiFileDesc
            {
                position = -1,
                path = path,
                name = Path.GetFileName(path)
            }).ToArray();
            var recips = message.Recipients.Select(recipient => new MapiRecipDesc
            {
                name = recipient.Name,
                address = "SMTP:" + recipient.Address,
                recipClass = recipient.Type == EmailRecipientType.Cc ? MapiRecipClass.Cc
                           : recipient.Type == EmailRecipientType.Bcc ? MapiRecipClass.Bcc
                           : MapiRecipClass.To
            }).ToArray();

            // Create a MAPI structure for the entirety of the message
            var mapiMessage = new MapiMessage
            {
                subject = message.Subject,
                noteText = message.BodyText,
                recips = ToUnmanagedArray(recips),
                recipCount = recips.Length,
                files = ToUnmanagedArray(files),
                fileCount = files.Length
            };

            // Determine the flags used to send the message
            int flags = NONE;
            if (!message.AutoSend)
            {
                flags |= MAPI_DIALOG;
            }
            if (!message.AutoSend || !message.SilentSend)
            {
                flags |= MAPI_LOGON_UI;
            }

            // Send the message
            int returnCode = MAPISendMail(IntPtr.Zero, IntPtr.Zero, mapiMessage, flags, 0);

            // Process the result
            if (returnCode == MAPI_E_USER_ABORT)
            {
                return false;
            }
            if (returnCode != SUCCESS)
            {
                throw new EmailException(new MapiException(returnCode));
            }
            return true;
        }

        /// <summary>
        /// Allocates an unmanaged array and populates it with the content of the given managed array.
        /// </summary>
        /// <typeparam name="T">The type of the array's elements.</typeparam>
        /// <param name="managedArray">The array from which to copy the content.</param>
        /// <returns>A pointer to the start of the unmanaaged array.</returns>
        private IntPtr ToUnmanagedArray<T>(IList<T> managedArray)
        {
            int elementSize = Marshal.SizeOf(typeof(T));

            // Allocate the unmanaged array
            int arraySize = managedArray.Count * elementSize;
            IntPtr unmanagedArray = Marshal.AllocHGlobal(arraySize);

            // Populate it from the content of the managed array
            for (int i = 0; i < managedArray.Count; ++i)
            {
                int ptrOffset = i * elementSize;
                Marshal.StructureToPtr(managedArray[i], unmanagedArray + ptrOffset, false);
            }

            return unmanagedArray;
        }
    }
}
