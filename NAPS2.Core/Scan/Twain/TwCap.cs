using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Twain
{
    internal enum TwCap : short
    {
        // ReSharper disable InconsistentNaming

        // all data sources are REQUIRED to support these caps
        CAP_XFERCOUNT = 0x0001,

        // image data sources are REQUIRED to support these caps
        ICAP_COMPRESSION = 0x0100,
        ICAP_PIXELTYPE = 0x0101,
        /// <summary>
        /// default is TWUN_INCHES
        /// </summary>
        ICAP_UNITS = 0x0102,
        ICAP_XFERMECH = 0x0103,

        // all data sources MAY support these caps
        CAP_AUTHOR = 0x1000,
        CAP_CAPTION = 0x1001,
        CAP_FEEDERENABLED = 0x1002,
        CAP_FEEDERLOADED = 0x1003,
        CAP_TIMEDATE = 0x1004,
        CAP_SUPPORTEDCAPS = 0x1005,
        CAP_EXTENDEDCAPS = 0x1006,
        CAP_AUTOFEED = 0x1007,
        CAP_CLEARPAGE = 0x1008,
        CAP_FEEDPAGE = 0x1009,
        CAP_REWINDPAGE = 0x100a,
        /// <summary>
        /// Added 1.1
        /// </summary>
        CAP_INDICATORS = 0x100b,
        /// <summary>
        /// Added 1.6
        /// </summary>
        CAP_SUPPORTEDCAPSEXT = 0x100c,
        /// <summary>
        /// Added 1.6
        /// </summary>
        CAP_PAPERDETECTABLE = 0x100d,
        /// <summary>
        /// Added 1.6
        /// </summary>
        CAP_UICONTROLLABLE = 0x100e,
        /// <summary>
        /// Added 1.6
        /// </summary>
        CAP_DEVICEONLINE = 0x100f,
        /// <summary>
        /// Added 1.6
        /// </summary>
        CAP_AUTOSCAN = 0x1010,
        /// <summary>
        /// Added 1.7
        /// </summary>
        CAP_THUMBNAILSENABLED = 0x1011,
        /// <summary>
        /// Added 1.7
        /// </summary>
        CAP_DUPLEX = 0x1012,
        /// <summary>
        /// Added 1.7
        /// </summary>
        CAP_DUPLEXENABLED = 0x1013,
        /// <summary>
        /// Added 1.7
        /// </summary>
        CAP_ENABLEDSUIONLY = 0x1014,
        /// <summary>
        /// Added 1.7
        /// </summary>
        CAP_CUSTOMDSDATA = 0x1015,
        /// <summary>
        /// Added 1.7
        /// </summary>
        CAP_ENDORSER = 0x1016,
        /// <summary>
        /// Added 1.7
        /// </summary>
        CAP_JOBCONTROL = 0x1017,
        /// <summary>
        /// Added 1.8
        /// </summary>
        CAP_ALARMS = 0x1018,
        /// <summary>
        /// Added 1.8
        /// </summary>
        CAP_ALARMVOLUME = 0x1019,
        /// <summary>
        /// Added 1.8
        /// </summary>
        CAP_AUTOMATICCAPTURE = 0x101a,
        /// <summary>
        /// Added 1.8
        /// </summary>
        CAP_TIMEBEFOREFIRSTCAPTURE = 0x101b,
        /// <summary>
        /// Added 1.8
        /// </summary>
        CAP_TIMEBETWEENCAPTURES = 0x101c,
        /// <summary>
        /// Added 1.8
        /// </summary>
        CAP_CLEARBUFFERS = 0x101d,
        /// <summary>
        /// Added 1.8
        /// </summary>
        CAP_MAXBATCHBUFFERS = 0x101e,
        /// <summary>
        /// Added 1.8
        /// </summary>
        CAP_DEVICETIMEDATE = 0x101f,
        /// <summary>
        /// Added 1.8
        /// </summary>
        CAP_POWERSUPPLY = 0x1020,
        /// <summary>
        /// Added 1.8
        /// </summary>
        CAP_CAMERAPREVIEWUI = 0x1021,
        /// <summary>
        /// Added 1.8
        /// </summary>
        CAP_DEVICEEVENT = 0x1022,
        /// <summary>
        /// Added 1.8
        /// </summary>
        CAP_SERIALNUMBER = 0x1024,
        /// <summary>
        /// Added 1.8
        /// </summary>
        CAP_PRINTER = 0x1026,
        /// <summary>
        /// Added 1.8
        /// </summary>
        CAP_PRINTERENABLED = 0x1027,
        /// <summary>
        /// Added 1.8
        /// </summary>
        CAP_PRINTERINDEX = 0x1028,
        /// <summary>
        /// Added 1.8
        /// </summary>
        CAP_PRINTERMODE = 0x1029,
        /// <summary>
        /// Added 1.8
        /// </summary>
        CAP_PRINTERSTRING = 0x102a,
        /// <summary>
        /// Added 1.8
        /// </summary>
        CAP_PRINTERSUFFIX = 0x102b,
        /// <summary>
        /// Added 1.8
        /// </summary>
        CAP_LANGUAGE = 0x102c,
        /// <summary>
        /// Added 1.8
        /// </summary>
        CAP_FEEDERALIGNMENT = 0x102d,
        /// <summary>
        /// Added 1.8
        /// </summary>
        CAP_FEEDERORDER = 0x102e,
        /// <summary>
        /// Added 1.8
        /// </summary>
        CAP_REACQUIREALLOWED = 0x1030,
        /// <summary>
        /// Added 1.8
        /// </summary>
        CAP_BATTERYMINUTES = 0x1032,
        /// <summary>
        /// Added 1.8
        /// </summary>
        CAP_BATTERYPERCENTAGE = 0x1033,
        /// <summary>
        /// Added 1.91
        /// </summary>
        CAP_CAMERASIDE = 0x1034,
        /// <summary>
        /// Added 1.91
        /// </summary>
        CAP_SEGMENTED = 0x1035,
        /// <summary>
        /// Added 2.0
        /// </summary>
        CAP_CAMERAENABLED = 0x1036,
        /// <summary>
        /// Added 2.0
        /// </summary>
        CAP_CAMERAORDER = 0x1037,
        /// <summary>
        /// Added 2.0
        /// </summary>
        CAP_MICRENABLED = 0x1038,
        /// <summary>
        /// Added 2.0
        /// </summary>
        CAP_FEEDERPREP = 0x1039,
        /// <summary>
        /// Added 2.0
        /// </summary>
        CAP_FEEDERPOCKET = 0x103a,
        /// <summary>
        /// Added 2.1
        /// </summary>
        CAP_AUTOMATICSENSEMEDIUM = 0x103b,
        /// <summary>
        /// Added 2.1
        /// </summary>
        CAP_CUSTOMINTERFACEGUID = 0x103c,

        // image data sources MAY support these caps
        ICAP_AUTOBRIGHT = 0x1100,
        ICAP_BRIGHTNESS = 0x1101,
        ICAP_CONTRAST = 0x1103,
        ICAP_CUSTHALFTONE = 0x1104,
        ICAP_EXPOSURETIME = 0x1105,
        ICAP_FILTER = 0x1106,
        ICAP_FLASHUSED = 0x1107,
        ICAP_GAMMA = 0x1108,
        ICAP_HALFTONES = 0x1109,
        ICAP_HIGHLIGHT = 0x110a,
        ICAP_IMAGEFILEFORMAT = 0x110c,
        ICAP_LAMPSTATE = 0x110d,
        ICAP_LIGHTSOURCE = 0x110e,
        ICAP_ORIENTATION = 0x1110,
        ICAP_PHYSICALWIDTH = 0x1111,
        ICAP_PHYSICALHEIGHT = 0x1112,
        ICAP_SHADOW = 0x1113,
        ICAP_FRAMES = 0x1114,
        ICAP_XNATIVERESOLUTION = 0x1116,
        ICAP_YNATIVERESOLUTION = 0x1117,
        ICAP_XRESOLUTION = 0x1118,
        ICAP_YRESOLUTION = 0x1119,
        ICAP_MAXFRAMES = 0x111a,
        ICAP_TILES = 0x111b,
        ICAP_BITORDER = 0x111c,
        ICAP_CCITTKFACTOR = 0x111d,
        ICAP_LIGHTPATH = 0x111e,
        ICAP_PIXELFLAVOR = 0x111f,
        ICAP_PLANARCHUNKY = 0x1120,
        ICAP_ROTATION = 0x1121,
        ICAP_SUPPORTEDSIZES = 0x1122,
        ICAP_THRESHOLD = 0x1123,
        ICAP_XSCALING = 0x1124,
        ICAP_YSCALING = 0x1125,
        ICAP_BITORDERCODES = 0x1126,
        ICAP_PIXELFLAVORCODES = 0x1127,
        ICAP_JPEGPIXELTYPE = 0x1128,
        ICAP_TIMEFILL = 0x112a,
        ICAP_BITDEPTH = 0x112b,
        /// <summary>
        /// Added 1.5
        /// </summary>
        ICAP_BITDEPTHREDUCTION = 0x112c,
        /// <summary>
        /// Added 1.6
        /// </summary>
        ICAP_UNDEFINEDIMAGESIZE = 0x112d,
        /// <summary>
        /// Added 1.7
        /// </summary>
        ICAP_IMAGEDATASET = 0x112e,
        /// <summary>
        /// Added 1.7
        /// </summary>
        ICAP_EXTIMAGEINFO = 0x112f,
        /// <summary>
        /// Added 1.7
        /// </summary>
        ICAP_MINIMUMHEIGHT = 0x1130,
        /// <summary>
        /// Added 1.7
        /// </summary>
        ICAP_MINIMUMWIDTH = 0x1131,
        /// <summary>
        /// Added 2.0
        /// </summary>
        ICAP_AUTODISCARDBLANKPAGES = 0x1134,
        /// <summary>
        /// Added 1.8
        /// </summary>
        ICAP_FLIPROTATION = 0x1136,
        /// <summary>
        /// Added 1.8
        /// </summary>
        ICAP_BARCODEDETECTIONENABLED = 0x1137,
        /// <summary>
        /// Added 1.8
        /// </summary>
        ICAP_SUPPORTEDBARCODETYPES = 0x1138,
        /// <summary>
        /// Added 1.8
        /// </summary>
        ICAP_BARCODEMAXSEARCHPRIORITIES = 0x1139,
        /// <summary>
        /// Added 1.8
        /// </summary>
        ICAP_BARCODESEARCHPRIORITIES = 0x113a,
        /// <summary>
        /// Added 1.8
        /// </summary>
        ICAP_BARCODESEARCHMODE = 0x113b,
        /// <summary>
        /// Added 1.8
        /// </summary>
        ICAP_BARCODEMAXRETRIES = 0x113c,
        /// <summary>
        /// Added 1.8
        /// </summary>
        ICAP_BARCODETIMEOUT = 0x113d,
        /// <summary>
        /// Added 1.8
        /// </summary>
        ICAP_ZOOMFACTOR = 0x113e,
        /// <summary>
        /// Added 1.8
        /// </summary>
        ICAP_PATCHCODEDETECTIONENABLED = 0x113f,
        /// <summary>
        /// Added 1.8
        /// </summary>
        ICAP_SUPPORTEDPATCHCODETYPES = 0x1140,
        /// <summary>
        /// Added 1.8
        /// </summary>
        ICAP_PATCHCODEMAXSEARCHPRIORITIES = 0x1141,
        /// <summary>
        /// Added 1.8
        /// </summary>
        ICAP_PATCHCODESEARCHPRIORITIES = 0x1142,
        /// <summary>
        /// Added 1.8
        /// </summary>
        ICAP_PATCHCODESEARCHMODE = 0x1143,
        /// <summary>
        /// Added 1.8
        /// </summary>
        ICAP_PATCHCODEMAXRETRIES = 0x1144,
        /// <summary>
        /// Added 1.8
        /// </summary>
        ICAP_PATCHCODETIMEOUT = 0x1145,
        /// <summary>
        /// Added 1.8
        /// </summary>
        ICAP_FLASHUSED2 = 0x1146,
        /// <summary>
        /// Added 1.8
        /// </summary>
        ICAP_IMAGEFILTER = 0x1147,
        /// <summary>
        /// Added 1.8
        /// </summary>
        ICAP_NOISEFILTER = 0x1148,
        /// <summary>
        /// Added 1.8
        /// </summary>
        ICAP_OVERSCAN = 0x1149,
        /// <summary>
        /// Added 1.8
        /// </summary>
        ICAP_AUTOMATICBORDERDETECTION = 0x1150,
        /// <summary>
        /// Added 1.8
        /// </summary>
        ICAP_AUTOMATICDESKEW = 0x1151,
        /// <summary>
        /// Added 1.8
        /// </summary>
        ICAP_AUTOMATICROTATE = 0x1152,
        /// <summary>
        /// Added 1.9
        /// </summary>
        ICAP_JPEGQUALITY = 0x1153,
        /// <summary>
        /// Added 1.91
        /// </summary>
        ICAP_FEEDERTYPE = 0x1154,
        /// <summary>
        /// Added 1.91
        /// </summary>
        ICAP_ICCPROFILE = 0x1155,
        /// <summary>
        /// Added 2.0
        /// </summary>
        ICAP_AUTOSIZE = 0x1156,
        /// <summary>
        /// Added 2.1
        /// </summary>
        ICAP_AUTOMATICCROPUSESFRAME = 0x1157,
        /// <summary>
        /// Added 2.1
        /// </summary>
        ICAP_AUTOMATICLENGTHDETECTION = 0x1158,
        /// <summary>
        /// Added 2.1
        /// </summary>
        ICAP_AUTOMATICCOLORENABLED = 0x1159,
        /// <summary>
        /// Added 2.1
        /// </summary>
        ICAP_AUTOMATICCOLORNONCOLORPIXELTYPE = 0x115a,
        /// <summary>
        /// Added 2.1
        /// </summary>
        ICAP_COLORMANAGEMENTENABLED = 0x115b,
        /// <summary>
        /// Added 2.1
        /// </summary>
        ICAP_IMAGEMERGE = 0x115c,
        /// <summary>
        /// Added 2.1
        /// </summary>
        ICAP_IMAGEMERGEHEIGHTTHRESHOLD = 0x115d,
        /// <summary>
        /// Added 2.1
        /// </summary>
        ICAP_SUPPORTEDEXTIMAGEINFO = 0x115e,

        // image data sources MAY support these audio caps
        /// <summary>
        /// Added 1.8
        /// </summary>
        ACAP_XFERMECH = 0x1202,

        // ReSharper enable InconsistentNaming
    }
}