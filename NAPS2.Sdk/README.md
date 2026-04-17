# NAPS2.Sdk

[![NuGet](https://img.shields.io/nuget/v/NAPS2.Sdk)](https://www.nuget.org/packages/NAPS2.Sdk/)

NAPS2.Sdk 是一个功能齐全的扫描库，支持 Windows、Mac 和 Linux 上的 WIA、TWAIN、SANE 和 ESCL 扫描仪。

## 包

NAPS2.Sdk 是模块化的，您可能需要根据需求引用不同的包。

### 必需包

- **[NAPS2.Sdk](https://www.nuget.org/packages/NAPS2.Sdk/)**
  - 包含适用于所有平台的核心扫描功能。
- 以下其中之一：
  - **[NAPS2.Images.Gdi](https://www.nuget.org/packages/NAPS2.Images.Gdi/)**
    - 用于处理 `System.Drawing.Bitmap` 图像。（Windows 窗体）
  - **[NAPS2.Images.Wpf](https://www.nuget.org/packages/NAPS2.Images.Wpf/)**
    - 用于处理 `System.Windows.Media.Imaging` 图像。（WPF）
  - **[NAPS2.Images.Gtk](https://www.nuget.org/packages/NAPS2.Images.Gtk/)**
    - 用于处理 `Gdk.Pixbuf` 图像。（Linux）
  - **[NAPS2.Images.Mac](https://www.nuget.org/packages/NAPS2.Images.Mac/)**
    - 用于处理 `AppKit.NSImage` 图像。（Mac）
  - **[NAPS2.Images.ImageSharp](https://www.nuget.org/packages/NAPS2.Images.ImageSharp/)**
    - 用于处理 [`ImageSharp`](https://github.com/SixLabors/ImageSharp) 图像。

### 可选包

- **[NAPS2.Sdk.Worker.Win32](https://www.nuget.org/packages/NAPS2.Sdk.Worker.Win32/)**
  - 用于在 Windows 上进行 [TWAIN 扫描](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/TwainSample.cs)。
- **[NAPS2.Pdfium.Binaries](https://www.nuget.org/packages/NAPS2.Pdfium.Binaries/)**
  - 用于 [导入 PDF](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/PdfImportSample.cs)。
- **[NAPS2.Sane.Binaries](https://www.nuget.org/packages/NAPS2.Sane.Binaries/)**
  - 用于在 Mac 上使用 SANE 驱动。（Linux 已预装，Windows 不支持。）
- **[NAPS2.Tesseract.Binaries](https://www.nuget.org/packages/NAPS2.Tesseract.Binaries/)**
  - 用于 [运行 OCR](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/OcrSample.cs)。（也可以使用单独的 Tesseract 安装。）
- **[NAPS2.Escl.Server](https://www.nuget.org/packages/NAPS2.Escl.Server/)**
  - 用于在本地网络中 [共享扫描仪](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/NetworkSharingSample.cs)。

## 用法

```c#
// 初始化
using var scanningContext = new ScanningContext(new GdiImageContext());
var controller = new ScanController(scanningContext);

// 查询可用扫描设备
var devices = await controller.GetDeviceList();

// 设置扫描选项
var options = new ScanOptions
{
    Device = devices.First(),
    PaperSource = PaperSource.Feeder,
    PageSize = PageSize.A4,
    Dpi = 300
};

// 扫描并保存图像
int i = 1;
await foreach (var image in controller.Scan(options))
{
    image.Save($"page{i++}.jpg");
}

// 扫描并保存 PDF
var images = await controller.Scan(options).ToListAsync();
var pdfExporter = new PdfExporter(scanningContext);
await pdfExporter.Export("doc.pdf", images);
```

更多 [示例](https://github.com/cyanfish/naps2/tree/master/NAPS2.Sdk.Samples)：
- [“Hello World” 扫描](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/HelloWorldSample.cs)
- [扫描并保存为 PDF/图像](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/ScanAndSaveSample.cs)
- [使用 TWAIN 驱动扫描](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/TwainSample.cs)
- [扫描到 System.Drawing.Bitmap](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/ScanToBitmapSample.cs)
- [导入和导出 PDF](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/PdfImportSample.cs)
- [使用 OCR 导出 PDF](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/OcrSample.cs)
- [在文件系统上存储图像数据](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/FileStorageSample.cs)
- [在本地网络上共享扫描仪](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/NetworkSharingSample.cs)

另见：
- [SDK 主页](https://www.naps2.com/sdk)
- [完整 API 文档](https://www.naps2.com/sdk/doc/api/)

## 使用 JS/TS 进行网页扫描

NAPS2 的 [扫描仪共享](https://github.com/cyanfish/naps2/blob/master/NAPS2.Sdk.Samples/NetworkSharingSample.cs) 服务器使用 ESCL，这是一种 [标准](https://mopria.org/mopria-escl-specification) HTTP 协议，可通过 JavaScript 或 TypeScript 从浏览器使用。

请参阅 [naps2-webscan](https://github.com/cyanfish/naps2-webscan) 项目，获取从浏览器扫描的示例代码。

## 驱动程序

|           | Windows | Mac | Linux |
|-----------|---------|-----|-------|
| **WIA**   | X       |     |       |
| **TWAIN** | X       | *   |       |
| **Apple** |         | X   |       |
| **SANE**  |         | X   | X     |
| **ESCL**  | X       | X   | X     |

[WIA](https://docs.microsoft.com/en-us/windows/win32/wia/-wia-startpage)（Windows 图像采集）是用于扫描仪（和相机）的 Microsoft 技术。许多扫描仪为 Windows 提供 WIA 驱动程序。

[TWAIN](https://twain.org/) 是一种跨平台图像采集标准。许多扫描仪为 Windows 和/或 Mac 提供 TWAIN 驱动程序。

Apple 的 [ImageCaptureCore](https://developer.apple.com/documentation/imagecapturecore) 为 Mac 设备提供访问 TWAIN 和 ESCL 扫描仪的能力。

[SANE](http://www.sane-project.org/) 是一个开源的 API 和多个扫描仪后端。主要用于 Linux，[支持的设备](http://www.sane-project.org/sane-supported-devices.html) 使用由开源贡献者或制造商提供的后端。

[ESCL](https://mopria.org/mopria-escl-specification)，也称为 Apple AirScan，是一种用于网络扫描的标准协议。许多现代扫描仪支持 ESCL，因为它是一种网络协议，不需要特定驱动。在某些情况下，ESCL 也可以通过 USB 连接使用。

### 选择驱动程序

每个平台都有默认驱动程序（Windows 上为 WIA，Mac 上为 Apple，Linux 上为 SANE）。要使用其他驱动程序，只需在查询设备时指定它：

```c#
var devices = await controller.GetDeviceList(Driver.Twain);
```

### 工作进程

在 Windows 上使用 TWAIN 驱动通常要求调用进程为 32 位。如果希望从 64 位进程中使用 TWAIN，NAPS2 提供了一个 32 位工作进程：

```c#
// 引用 NAPS2.Sdk.Worker.Win32 包并调用此方法
scanningContext.SetUpWin32Worker();
```

## 贡献

想为 NAPS2 或 NAPS2.Sdk 做贡献？请查看 [wiki](https://github.com/cyanfish/naps2/wiki/1.-Building-&-Development-Environment)。
