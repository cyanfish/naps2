using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Resources;

namespace NAPS2.Scan
{
    /// <summary>
    /// An attribute used for enum values that assigns a string from a resources file.
    /// The string value is accessed using the ScanEnumExtensions.Description extension method.
    /// </summary>
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        private readonly string resourceName;
        private readonly ResourceManager resourceManager;

        public LocalizedDescriptionAttribute(Type resourceType, string resourceName)
        {
            this.resourceName = resourceName;
            resourceManager = new ResourceManager(resourceType);
        }

        public override string Description => resourceManager.GetString(resourceName);
    }
}
