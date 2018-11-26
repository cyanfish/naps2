using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config;
namespace NAPS2.ImportExport.Email
{
    public class EmailSettingsContainer
    {
        private EmailSettings localEmailSettings;
        
        public EmailSettings EmailSettings
        {
            get => localEmailSettings ?? UserConfig.Current.EmailSettings ?? new EmailSettings();
            set => localEmailSettings = value;
        }
    }
}
