# NAPS2 - Not Another PDF Scanner

<p align="center">
<img src="https://www.naps2.com/images/naps2-desktop-win.png?1" width="400" alt="Windows 上的 NAPS2" /> <img src="https://www.naps2.com/images/naps2-desktop-mac.png?1" width="400" alt="Mac 上的 NAPS2" /> <img src="https://www.naps2.com/images/naps2-desktop-linux.png?1" width="400" alt="Linux 上的 NAPS2" />
  <br/>
  <i>Windows、Mac 和 Linux 上的 NAPS2</i>
</p>

NAPS2 是一款文档扫描应用，专注于简洁和易用。它可以从 WIA、TWAIN、SANE 和 ESCL 扫描仪扫描文档，按您的需要组织页面，并将其保存为 PDF、TIFF、JPEG 或 PNG。还可以使用 [Tesseract](https://github.com/tesseract-ocr/tesseract) 进行光学字符识别 (OCR)。

系统要求：
- Windows 7 及以上（x64、x86）
- macOS 10.15 及以上（x64、arm64）
- Linux（x64、arm64）（GTK 3.20+、glibc 2.27+、libsane）

请访问 NAPS2 主页：[www.naps2.com](http://www.naps2.com)。

其他链接：
- [下载](https://www.naps2.com/download)
- [文档](https://www.naps2.com/support)
- [翻译](https://translate.naps2.com/)
- [提交工单](https://sourceforge.net/p/naps2/tickets/)
- [捐赠](https://www.naps2.com/donate?src=readme)

## NAPS2.Sdk（开发人员用）

[![NuGet](https://img.shields.io/nuget/v/NAPS2.Sdk)](https://www.nuget.org/packages/NAPS2.Sdk/)

[NAPS2.Sdk](https://github.com/cyanfish/naps2/tree/master/NAPS2.Sdk) 是一个功能齐全的扫描库，支持 Windows、Mac 和 Linux 上的 WIA、TWAIN、SANE 和 ESCL 扫描仪。
[阅读更多。](https://github.com/cyanfish/naps2/tree/master/NAPS2.Sdk)

## 构建说明
想为 NAPS2 或 NAPS2.Sdk 做贡献？请查看 [Github wiki](https://github.com/cyanfish/naps2/wiki/1.-Building-&-Development-Environment) 以获取构建说明和更多信息。

## 许可证

NAPS2 在 GNU GPL 2.0（或更高版本）许可下发布。部分项目具有额外的许可选项：
- NAPS2.Escl.* - GNU LGPL 2.1（或更高版本）
- NAPS2.Images.* - GNU LGPL 2.1（或更高版本）
- NAPS2.Internals - GNU LGPL 2.1（或更高版本）
- NAPS2.Sdk - GNU LGPL 2.1（或更高版本）
- NAPS2.Sdk.Samples - MIT
