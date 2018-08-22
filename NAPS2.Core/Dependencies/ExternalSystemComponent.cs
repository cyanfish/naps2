using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NAPS2.Config;
using NAPS2.Util;

namespace NAPS2.Dependencies
{
    public class ExternalSystemComponent : IExternalComponent
    {
        private readonly PlatformSupport platformSupport;

        private bool installed;

        public ExternalSystemComponent(string path, string dataPath, PlatformSupport platformSupport = null)
        {
            this.platformSupport = platformSupport;
            Path = path;
            DataPath = System.IO.Path.Combine(ExternalComponent.BasePath, dataPath);
        }

        private void CheckIfInstalled()
        {
            if (IsSupported)
            {
                try
                {
                    var process = Process.Start(Path);
                    if (process != null)
                    {
                        installed = true;
                        process.Kill();
                    }
                }
                catch (Exception)
                {
                    // Component is not installed on the system path (or had an error)
                }
            }
        }

        public string Path { get; }

        public string DataPath { get; }

        public bool IsInstalled
        {
            get
            {
                if (installed)
                {
                    return true;
                }
                CheckIfInstalled();
                return installed;
            }
        }

        public bool IsSupported => platformSupport == null || platformSupport.Validate();
    }
}
