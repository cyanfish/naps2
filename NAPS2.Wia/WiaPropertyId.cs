namespace NAPS2.Wia;

/// <summary>
/// Property ID constants.
///
/// Prefix conventions:
/// D = device, I = item
/// I = information, P = property
/// A = all, S = scanner, C = camera
///
/// https://docs.microsoft.com/en-us/windows/desktop/wia/-wia-wiadeviceinfoprop
/// https://docs.microsoft.com/en-us/windows/desktop/wia/-wia-wiaitempropcommondevice
/// https://docs.microsoft.com/en-us/windows/desktop/wia/-wia-wiaitempropscannerdevice
/// https://docs.microsoft.com/en-us/windows/desktop/wia/-wia-wiaitempropcommonitem
/// https://docs.microsoft.com/en-us/windows/desktop/wia/-wia-wiaitempropscanneritem
/// </summary>
public static class WiaPropertyId
{
    public const int DIP_DEV_ID = 2;
    public const int DIP_VEND_DESC = 3;
    public const int DIP_DEV_DESC = 4;
    public const int DIP_DEV_TYPE = 5;
    public const int DIP_DEV_NAME = 7;
    public const int DIP_WIA_VERSION = 14;
        
    public const int DPS_HORIZONTAL_SHEET_FEED_SIZE = 3076;
    public const int DPS_VERTICAL_SHEET_FEED_SIZE = 3077;
    public const int DPS_HORIZONTAL_BED_SIZE = 3074;
    public const int DPS_VERTICAL_BED_SIZE = 3075;
    public const int DPS_DOCUMENT_HANDLING_CAPABILITIES = 3086;
    public const int DPS_DOCUMENT_HANDLING_STATUS = 3087;
    public const int DPS_DOCUMENT_HANDLING_SELECT = 3088;
    public const int DPS_PAGES = 3096;
        
    public const int IPA_ITEM_NAME = 4098;
    public const int IPA_FULL_ITEM_NAME = 4099;
    public const int IPA_DATATYPE = 4103;

    public const int IPS_XRES = 6147;
    public const int IPS_YRES = 6148;
    public const int IPS_XPOS = 6149;
    public const int IPS_YPOS = 6150;
    public const int IPS_XEXTENT = 6151;
    public const int IPS_YEXTENT = 6152;
    public const int IPS_BRIGHTNESS = 6154;
    public const int IPS_CONTRAST = 6155;
    public const int IPS_ORIENTATION = 6156;
    public const int IPS_MAX_HORIZONTAL_SIZE = 6165;
    public const int IPS_MAX_VERTICAL_SIZE = 6166;
    public const int IPS_DOCUMENT_HANDLING_SELECT = 3088;
    public const int IPS_PAGES = 3096;
}