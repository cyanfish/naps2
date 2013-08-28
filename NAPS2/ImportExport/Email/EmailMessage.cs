using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.ImportExport.Email
{
    public class EmailMessage
    {
        public EmailMessage()
        {
            Recipients = new List<EmailRecipient>();
            Attachments = new List<EmailAttachment>();
        }

        public string Subject { get; set; }

        public string BodyText { get; set; }

        public List<EmailRecipient> Recipients { get; set; }

        public List<EmailAttachment> Attachments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the email should be sent automatically without prompting the user to make changes first.
        /// </summary>
        public bool AutoSend { get; set; }

        /// <summary>>
        /// Gets or sets a value indicating whether, if AutoSend is true, the mail should be sent without prompting the user for credentials when necessary.
        /// This may result in an authorization error.
        /// </summary>
        public bool SilentSend { get; set; }
    }
}
