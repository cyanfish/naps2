using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config;
namespace NAPS2.ImportExport.Email
{
    public class EmailSettingsContainer
    {
        private readonly IUserConfigManager userConfigManager;

        private EmailSettings localEmailSettings;

        public EmailSettingsContainer(IUserConfigManager userConfigManager)
        {
            this.userConfigManager = userConfigManager;
        }

        public EmailSettings EmailSettings
        {
            get => localEmailSettings ?? userConfigManager.Config.EmailSettings ?? new EmailSettings();
            set => localEmailSettings = value;
        }
    }
}
