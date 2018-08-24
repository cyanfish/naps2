using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config;

namespace NAPS2.Dependencies
{
    public class ComponentManager
    {
        private readonly AppConfigManager appConfigManager;

        private string basePath;

        public ComponentManager(AppConfigManager appConfigManager)
        {
            this.appConfigManager = appConfigManager;
        }

        public string BasePath
        {
            get
            {
                if (basePath == null)
                {
                    var customPath = appConfigManager.Config.ComponentsPath;
                    basePath = string.IsNullOrWhiteSpace(customPath)
                        ? Paths.Components
                        : Environment.ExpandEnvironmentVariables(customPath);
                }
                return basePath;
            }
        }
    }
}
