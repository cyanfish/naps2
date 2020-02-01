# NAPS2.Wia

NAPS2.Wia is a standalone component that acts as a low-level wrapper around [Windows Image Acquisition (WIA)](https://docs.microsoft.com/en-us/windows/win32/wia/-wia-startpage).

Compared to the COM-based [wiaaut.dll](https://docs.microsoft.com/en-us/previous-versions/windows/desktop/wiaaut/-wiaaut-startpage), you get:
- WIA 2.0 support
- Better feeder compatibility
- Idiomatic .NET interface

If you're looking for a higher-level and easier-to-use scanning interface, check out [NAPS2.Sdk](https://github.com/cyanfish/naps2/tree/master/NAPS2.Sdk).

## Example

```
using var deviceManager = new WiaDeviceManager();

// Prompt the user to select a scanner
using var device = deviceManager.PromptForDevice(parentForm.Handle);

// Select between Flatbed/Feeder
using var item = device.FindSubItem("Feeder");

// Enable duplex scanning
item.SetProperty(WiaPropertyId.IPS_DOCUMENT_HANDLING_SELECT,
                 WiaPropertyValue.DUPLEX | WiaPropertyValue.FRONT_FIRST);

// Set up the scan
using var transfer = item.StartTransfer();
transfer.PageScanned += (sender, args) =>
{
    using (args.Stream)
    {
        var bitmap = new Bitmap(args.Stream);
        // Do something with the image
    }
};

// Do the actual scan
transfer.Download();
```

## Known issues
- Native acquisition with WIA 1.0 only works in 32-bit processes. NAPS2.Sdk provides a 32-bit worker process that can be used from 64-bit applications. Most users shouldn't be affected since WIA 2.0 has no such issue.

## Supported Platforms
- .NET Framework 4.0 (Windows XP+)
- .NET Core 2.0+ (Windows)
- .NET Standard 2.0+ (Windows)

## License
Unlike most of NAPS2.Sdk which is licensed under the LGPL, NAPS2.Wia uses the more permissive MIT license. 