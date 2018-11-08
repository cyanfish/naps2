using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Scan.Wia.Native;

namespace NAPS2.Scan.Wia
{
    public static class WiaItemExtensions
    {
        public static string Id(this WiaItem item)
        {
            return item.Property(WiaPropertyId.DIP_DEV_ID).Value.ToString();
        }

        public static string Name(this WiaItem item)
        {
            return item.Property(WiaPropertyId.DIP_DEV_NAME).Value.ToString();
        }

        public static bool SupportsFeeder(this WiaItem device)
        {
            int capabilities = (int)device.Property(WiaPropertyId.DPS_DOCUMENT_HANDLING_CAPABILITIES).Value;
            return (capabilities & WiaPropertyValue.FEEDER) != 0;
        }

        public static bool SupportsDuplex(this WiaItem device)
        {
            int capabilities = (int)device.Property(WiaPropertyId.DPS_DOCUMENT_HANDLING_CAPABILITIES).Value;
            return (capabilities & WiaPropertyValue.DUPLEX) != 0;
        }

        public static bool FeederReady(this WiaItem device)
        {
            int status = (int)device.Property(WiaPropertyId.DPS_DOCUMENT_HANDLING_STATUS).Value;
            return (status & WiaPropertyValue.FEED_READY) != 0;
        }

        public static void SetProperty(this WiaItem item, int propId, int value)
        {
            var prop = item.Property(propId);
            if (prop != null)
            {
                prop.Value = value;
            }
        }

        public static void SetPropertyRange(this WiaItem item, int propId, int value, int expectedMin, int expectedMax)
        {
            var prop = item.Property(propId);
            if (prop != null)
            {
                int expectedAbs = value - expectedMin;
                int expectedRange = expectedMax - expectedMin;
                int actualRange = prop.SubTypeMax - prop.SubTypeMin;
                int actualValue = expectedAbs * actualRange / expectedRange + prop.SubTypeMin;
                if (prop.SubTypeStep != 0)
                {
                    actualValue -= actualValue % prop.SubTypeStep;
                }
                actualValue = Math.Min(actualValue, prop.SubTypeMax);
                actualValue = Math.Max(actualValue, prop.SubTypeMin);
                prop.Value = actualValue;
            }
        }

        public static int GetPropertyMax(this WiaItem item, int propId)
        {
            var prop = item.Property(propId);
            if (prop != null)
            {
                if (prop.SubType == WiaProperty.SubTypes.Range)
                {
                    return prop.SubTypeMax;
                }
                if (prop.SubType == WiaProperty.SubTypes.List && prop.SubTypeValues.Any())
                {
                    return prop.SubTypeValues.Cast<int>().Max();
                }
            }
            return int.MaxValue;
        }
    }
}
