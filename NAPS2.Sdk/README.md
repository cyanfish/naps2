# NAPS2.Sdk

NAPS2.Sdk is a fully-featured scanning library, supporting WIA, TWAIN, SANE, and ESCL scanners on Windows, Mac, and Linux.

## Drivers

|           | Windows | Mac | Linux |
|-----------|---------|-----|-------|
| **WIA**   | X       |     |       |
| **TWAIN** | X       | X   |       |
| **SANE**  |         |     | X     |
| **ESCL**  | X       | X   | X     |

[WIA](https://docs.microsoft.com/en-us/windows/win32/wia/-wia-startpage) (Windows Image Acquisition) is a Microsoft technology for scanners (and cameras). Many scanners provide WIA drivers for Windows.

[TWAIN](https://twain.org/) is a cross-platform standard for image acquisition. Many scanners provide TWAIN drivers for Windows and/or Mac.

[SANE](http://www.sane-project.org/) is an open-source API and set of backends for various scanners. Primarily for Linux, [supported devices](http://www.sane-project.org/sane-supported-devices.html) use backends made by open-source contributors or the manufacturer themselves.

[ESCL](https://mopria.org/mopria-escl-specification), also known as Apple AirScan, is a standard protocol for scanning over a network. Many modern scanners support ESCL, and as it's a network protocol, specific drivers aren't required.

## Usage

See the [Samples](https://github.com/cyanfish/naps2/tree/master/NAPS2.Sdk.Samples).

## Contributing

<!-- TODO: Move dev onboarding to the github wiki -->
Looking to contribute to NAPS2 or NAPS2.Sdk? Have a look at the [Developer Onboarding](https://www.naps2.com/doc-dev-onboarding.html) page.
