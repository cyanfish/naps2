# NAPS2.Wia

NAPS2.Wia is a standalone component that acts as a low-level wrapper around [Windows Image Acquisition (WIA)](https://docs.microsoft.com/en-us/windows/win32/wia/-wia-startpage).

Compared to the COM-based [wiaaut.dll](https://docs.microsoft.com/en-us/previous-versions/windows/desktop/wiaaut/-wiaaut-startpage), you get:
- WIA 2.0 support
- Better feeder compatibility
- Easy-to-use and idiomatic .NET interface

## Known issues
- Native acquisition with WIA 1.0 only works in 32-bit processes. NAPS2.Sdk provides a 32-bit worker process that can be used from 64-bit applications. Most users shouldn't be affected since WIA 2.0 has no such issue.

## Supported Platforms
- .NET Framework 4.0 (Windows XP+)
- .NET Core 2.0+ (Windows)
- .NET Standard 2.0+ (Windows)

## License
Unlike most of NAPS2.Sdk which is licensed under the LGPL, NAPS2.Wia uses the more permissive MIT license. 