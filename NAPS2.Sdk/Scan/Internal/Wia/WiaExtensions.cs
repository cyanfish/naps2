using NAPS2.Wia;

namespace NAPS2.Scan.Internal.Wia;

public static class WiaExtensions
{
    public static void SafeSetProperty(this WiaItemBase item, int propId, int value)
    {
        try
        {
            item.SetProperty(propId, value);
        }
        catch (Exception e)
        {
            Log.ErrorException("Error setting property", e);
        }
    }
        
    public static void SafeSetPropertyClosest(this WiaItemBase item, int propId, ref int value)
    {
        try
        {
            item.SetPropertyClosest(propId, ref value);
        }
        catch (Exception e)
        {
            Log.ErrorException("Error setting property", e);
        }
    }

    public static void SafeSetPropertyRange(this WiaItemBase item, int propId, int value, int expectedMin, int expectedMax)
    {
        try
        {
            item.SetPropertyRange(propId, value, expectedMin, expectedMax);
        }
        catch (Exception e)
        {
            Log.ErrorException("Error setting property", e);
        }
    }
}