using System;
using System.ComponentModel;
using System.Resources;

namespace NAPS2.Scan
{
    /// <summary>
    /// An attribute used for enum values that assigns a string from a resources file.
    /// The string value is accessed using the ScanEnumExtensions.Description extension method.
    /// </summary>
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        private readonly string _resourceName;
        private readonly ResourceManager _resourceManager;

        public LocalizedDescriptionAttribute(Type resourceType, string resourceName)
        {
            _resourceName = resourceName;
            _resourceManager = new ResourceManager(resourceType);
        }

        public override string Description => _resourceManager.GetString(_resourceName);
    }
}
