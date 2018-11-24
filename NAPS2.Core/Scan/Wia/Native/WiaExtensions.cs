using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Wia.Native
{
    public static class WiaExtensions
    {
        public static string Id(this IWiaDeviceProps device)
        {
            return device.Properties[WiaPropertyId.DIP_DEV_ID].Value.ToString();
        }

        public static string Name(this IWiaDeviceProps device)
        {
            return device.Properties[WiaPropertyId.DIP_DEV_NAME].Value.ToString();
        }

        public static string Name(this WiaItem item)
        {
            return item.Properties[WiaPropertyId.IPA_ITEM_NAME].Value.ToString();
        }

        public static string FullName(this WiaItem item)
        {
            return item.Properties[WiaPropertyId.IPA_FULL_ITEM_NAME].Value.ToString();
        }

        public static bool SupportsFeeder(this WiaDevice device)
        {
            int capabilities = (int)device.Properties[WiaPropertyId.DPS_DOCUMENT_HANDLING_CAPABILITIES].Value;
            return (capabilities & WiaPropertyValue.FEEDER) != 0;
        }

        public static bool SupportsDuplex(this WiaDevice device)
        {
            int capabilities = (int)device.Properties[WiaPropertyId.DPS_DOCUMENT_HANDLING_CAPABILITIES].Value;
            return (capabilities & WiaPropertyValue.DUPLEX) != 0;
        }

        public static bool FeederReady(this WiaDevice device)
        {
            int status = (int)device.Properties[WiaPropertyId.DPS_DOCUMENT_HANDLING_STATUS].Value;
            return (status & WiaPropertyValue.FEED_READY) != 0;
        }

        public static void SetProperty(this WiaItemBase item, int propId, int value)
        {
            var prop = item.Properties[propId];
            if (prop != null)
            {
                prop.Value = value;
            }
        }

        public static void SetPropertyRange(this WiaItemBase item, int propId, int value, int expectedMin, int expectedMax)
        {
            var prop = item.Properties[propId];
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

        public static Dictionary<int, object> SerializeEditable(this WiaPropertyCollection props)
        {
            return props.Where(x => x.Type == WiaPropertyType.I4).ToDictionary(x => x.Id, x => x.Value);
        }

        public static Dictionary<int, object> Delta(this WiaPropertyCollection props, Dictionary<int, object> target)
        {
            var source = props.SerializeEditable();
            var delta = new Dictionary<int, object>();
            foreach (var kvp in target)
            {
                if (source.ContainsKey(kvp.Key) && !Equals(source[kvp.Key], kvp.Value))
                {
                    delta.Add(kvp.Key, kvp.Value);
                }
            }
            return delta;
        }

        public static void DeserializeEditable(this WiaPropertyCollection props, Dictionary<int, object> values)
        {
            foreach (var kvp in values)
            {
                var prop = props[kvp.Key];
                if (prop != null)
                {
                    try
                    {
                        prop.Value = kvp.Value;
                    }
                    catch (WiaException)
                    {
                        // Skip non-writable properties
                        // Ideally this would not be done with an exception...
                    }
                }
            }
        }
    }
}
