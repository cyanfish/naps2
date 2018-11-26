using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Config;

namespace NAPS2.Dependencies
{
    public class ComponentManager
    {
        private string basePath;
        
        public string BasePath
        {
            get
            {
                if (basePath == null)
                {
                    var customPath = AppConfig.Current.ComponentsPath;
                    basePath = string.IsNullOrWhiteSpace(customPath)
                        ? Paths.Components
                        : Environment.ExpandEnvironmentVariables(customPath);
                }
                return basePath;
            }
        }
    }
}
