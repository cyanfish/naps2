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
  - For [importing PDFs]().
- **[NAPS2.Sane.Binaries](https://www.nuget.org/packages/NAPS2.Sane.Binaries/)**
  - For [using SANE drivers]() on Mac. (Linux has them pre-installed, and Windows isn't supported.) 
- **[NAPS2.Tesseract.Binaries](https://www.nuget.org/packages/NAPS2.Tesseract.Binaries/)**
  - For [running OCR](). (You can also use a separate Tesseract installation if you like.) 

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

## Usage

See the [Samples](https://github.com/cyanfish/naps2/tree/master/NAPS2.Sdk.Samples).

## Contributing

<!-- TODO: Move dev onboarding to the github wiki -->
Looking to contribute to NAPS2 or NAPS2.Sdk? Have a look at the [Developer Onboarding](https://www.naps2.com/doc/dev-onboarding) page.
