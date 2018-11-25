using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.Logging
{
    public class EventParams
    {
        public string Name { get; set; }

        public int? Pages { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string User { get; set; } = $"{Environment.UserDomainName}\\{Environment.UserName}";

        public string DeviceName { get; set; }

        public string ProfileName { get; set; }

        public string BitDepth { get; set; }

        public string FileFormat { get; set; }

        public override string ToString()
        {
            var result = new StringBuilder();
            if (Name != null)
            {
                result.AppendLine($"Event: {Name}");
            }
            if (Pages != null)
            {
                result.AppendLine($"Pages: {Pages}");
            }
            if (DeviceName != null)
            {
                result.AppendLine($"Device Name: {DeviceName}");
            }
            if (ProfileName != null)
            {
                result.AppendLine($"Profile Name: {ProfileName}");
            }
            if (BitDepth != null)
            {
                result.AppendLine($"Bit Depth: {BitDepth}");
            }
            if (FileFormat != null)
            {
                result.AppendLine($"File Format: {FileFormat}");
            }

            result.AppendLine($"Timestamp: {Timestamp}");
            result.AppendLine($"User: {User}");

            return result.ToString();
        }
    }
}