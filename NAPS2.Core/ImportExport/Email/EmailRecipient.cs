using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.ImportExport.Email
{
    public class EmailRecipient
    {
        public static IEnumerable<EmailRecipient> FromText(EmailRecipientType recipType, string recipText)
        {
            if (string.IsNullOrWhiteSpace(recipText))
            {
                yield break;
            }
            foreach (string address in recipText.Split(','))
            {
                yield return new EmailRecipient
                {
                    Name = address.Trim(),
                    Address = address.Trim(),
                    Type = recipType
                };
            }
        }

        public EmailRecipient()
        {
            Name = "";
        }

        /// <summary>
        /// Gets or sets the recipient's name. Can be empty but must not be null.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the recipient's email address.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the type of recipient ("to", "cc", "bcc").
        /// </summary>
        public EmailRecipientType Type { get; set; }
    }
}
