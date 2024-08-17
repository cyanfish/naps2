# NAPS2.Sdk

[![NuGet](https://img.shields.io/nuget/v/NAPS2.Sdk)](https://www.nuget.org/packages/NAPS2.Sdk/)

NAPS2.Sdk is a fully-featured scanning library, supporting WIA, TWAIN, SANE, and ESCL scanners on Windows, Mac, and Linux.

## Packages

NAPS2.Sdk is modular, and depending on your needs you may have to reference a different set of packages.

### Required Packages

- **[NAPS2.Sdk](https://www.nuget.org/packages/NAPS2.Sdk/)**
  - Contains core scanning functionality for all platforms. 
- Exactly one of:
  - **[NAPS2.Images.Gdi](https://www.nuget.org/packages/NAPS2.Images.Gdi/)**
    - For working with `System.Drawing.Bitmap` images. (Windows Forms)
  - **[NAPS2.Images.Wpf](https://www.nuget.org/packages/NAPS2.Images.Wpf/)**
    - For working with ` System.Windows.Media.Imaging` images. (WPF)
  - **[NAPS2.Images.Gtk](https://www.nuget.org/packages/NAPS2.Images.Gtk/)**
    - For working with `Gdk.Pixbuf` images. (Linux)
  - **[NAPS2.Images.Mac](https://www.nuget.org/packages/NAPS2.Images.Mac/)**
    - For working with `AppKit.NSImage` images. (Mac)
  - **[NAPS2.Images.ImageSharp](https://www.nuget.org/packages/NAPS2.Images.ImageSharp/)**
    - For working with [`ImageSharp`](https://github.com/SixLabors/ImageSharp) images.

### Optional Packages

- **[NAPS2.Sdk.Worker.Win32](https://www.nuget.org/packages/NAPS2.Sdk.Worker.Win32/)**
  - For scanning with [TWAIN on Windows](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/TwainSample.cs).
- **[NAPS2.Pdfium.Binaries](https://www.nuget.org/packages/NAPS2.Pdfium.Binaries/)**
  - For [importing PDFs](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/PdfImportSample.cs).
- **[NAPS2.Sane.Binaries](https://www.nuget.org/packages/NAPS2.Sane.Binaries/)**
  - For [using SANE drivers]() on Mac. (Linux has them pre-installed, and Windows isn't supported.) 
- **[NAPS2.Tesseract.Binaries](https://www.nuget.org/packages/NAPS2.Tesseract.Binaries/)**
  - For [running OCR](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/OcrSample.cs). (You can also use a separate Tesseract installation if you like.)
- **[NAPS2.Escl.Server](https://www.nuget.org/packages/NAPS2.Escl.Server/)**
  - For [sharing scanners](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/NetworkSharingSample.cs) across the local network.

## Usage

```c#
// Set up
using var scanningContext = new ScanningContext(new GdiImageContext());
var controller = new ScanController(scanningContext);

// Query for available scanning devices
var devices = await controller.GetDeviceList();

// Set scanning options
var options = new ScanOptions
{
    Device = devices.First(),
    PaperSource = PaperSource.Feeder,
    PageSize = PageSize.A4,
    Dpi = 300
};

// Scan and save images
int i = 1;
await foreach (var image in controller.Scan(options))
{
    image.Save($"page{i++}.jpg");
}

// Scan and save PDF
var images = await controller.Scan(options).ToListAsync();
var pdfExporter = new PdfExporter(scanningContext);
await pdfExporter.Export("doc.pdf", images);
```

More [samples](https://github.com/cyanfish/naps2/tree/master/NAPS2.Sdk.Samples):
- ["Hello World" scanning](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/HelloWorldSample.cs)
- [Scan and save to PDF/images](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/ScanAndSaveSample.cs)
- [Scan with TWAIN drivers](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/TwainSample.cs)
- [Scan to System.Drawing.Bitmap](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/ScanToBitmapSample.cs)
- [Import and export PDFs](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/PdfImportSample.cs)
- [Export PDFs with OCR](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/OcrSample.cs)
- [Store image data on the filesystem](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/FileStorageSample.cs)
- [Share scanners on the local network](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/NetworkSharingSample.cs)

Also see:
- [SDK Homepage](https://www.naps2.com/sdk)
- [Full Api Docs](https://www.naps2.com/sdk/doc/api/)

## Web Scanning with JS/TS

NAPS2's [scanner-sharing](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/NetworkSharingSample.cs) server uses ESCL, which is a [standard](https://mopria.org/mopria-escl-specification) HTTP protocol and can be used from a web browser with JavaScript or TypeScript.

See the [naps2-webscan](https://github.com/cyanfish/naps2-webscan) project for example code to scan from a browser.

## Drivers

|           | Windows | Mac | Linux |
|-----------|---------|-----|-------|
| **WIA**   | X       |     |       |
| **TWAIN** | X       | *   |       |
| **Apple** |         | X   |       |
| **SANE**  |         | X   | X     |
| **ESCL**  | X       | X   | X     |

[WIA](https://docs.microsoft.com/en-us/windows/win32/wia/-wia-startpage) (Windows Image Acquisition) is a Microsoft technology for scanners (and cameras). Many scanners provide WIA drivers for Windows.

[TWAIN](https://twain.org/) is a cross-platform standard for image acquisition. Many scanners provide TWAIN drivers for Windows and/or Mac.

Apple's [ImageCaptureCore](https://developer.apple.com/documentation/imagecapturecore) provides access to TWAIN and ESCL scanners on Mac devices.

[SANE](http://www.sane-project.org/) is an open-source API and set of backends for various scanners. Primarily for Linux, [supported devices](http://www.sane-project.org/sane-supported-devices.html) use backends made by open-source contributors or the manufacturer themselves.

[ESCL](https://mopria.org/mopria-escl-specification), also known as Apple AirScan, is a standard protocol for scanning over a network. Many modern scanners support ESCL, and as it's a network protocol, specific drivers aren't required. ESCL can also be used over a USB connection in some cases.

### Choosing a Driver

Each platform has a default driver (WIA on Windows, Apple on Mac, and SANE on Linux). To use another driver, you only need to specify it when querying for devices:

```c#
var devices = await controller.GetDeviceList(Driver.Twain);
```

### Worker Processes

Using the TWAIN driver on Windows usually requires the calling process to be 32-bit. If you want to use TWAIN from a 64-bit process, NAPS2 provides a 32-bit worker process:

```c#
// Reference the NAPS2.Sdk.Worker.Win32 package and call this method
scanningContext.SetUpWin32Worker();
```

## Contributing

Looking to contribute to NAPS2 or NAPS2.Sdk? Have a look at the [wiki](https://github.com/cyanfish/naps2/wiki/1.-Building-&-Development-Environment).
