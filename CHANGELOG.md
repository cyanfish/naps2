Changes in 8.2.1:
- Microsoft Store 版本在使用一个月后添加了评论提示
- Windows：为 arm64 安装程序添加了 TWAIN 支持
- Windows：修复了一些 OCR 兼容性问题
- Windows：修复了预览窗口被截断的问题
- Mac：修复了 SANE 崩溃
- Escl：修复了一些兼容性问题
- Sane：修复了一些双面扫描兼容性问题
- Linux：修复了 Gmail/Outlook Web 选项未显示的问题
- Linux：修复了禁用 PDF 加密后按钮隐藏的问题
- Sdk：修复了多余的错误日志

Changes in 8.2.0:
- NAPS2 现在已在 Microsoft Store 上可用
  - 它收取少量费用以支持开发者并提供自动更新
  - NAPS2 将继续在 www.naps2.com 免费提供
- 在“图像”菜单下添加了“使用...编辑”以使用外部图像编辑器
- 为扫描仪共享添加了“即使 NAPS2 已关闭也共享”选项
  - 这将显示系统托盘图标并在登录时重启
- 导入文件名现在在保存时用作默认文件名
- “对所有已选”复选框现在保持选中
- Escl：将搜索设备的最大时间从 5 秒增加到 60 秒
- Escl：现在缓存扫描仪 IP 以实现更快、更可靠的扫描
- Windows：添加了 arm64 安装程序
- Windows：将某些驱动程序的“无友好名称”设备名称替换为“Unknown Scanner”
- Mac：修复了已保存文件扩展名不总是正确的问题
- Mac：当不是默认邮件阅读器时禁用“Apple Mail”电子邮件提供程序
- Mac：更新了拆分/合并图标
- Linux：修复了保存对话框的问题

Changes in 8.1.4:
- Windows：添加了“主题”设置，可在浅色和深色模式之间切换
- Linux：修复了旧版 Linux 上的 OCR 问题（例如 Ubuntu 20.04）
  - 不再支持 Ubuntu 18.04（如需要，请使用 NAPS2 7.5.3）

Changes in 8.1.3:
- Twain：修复了 Kyocera Ecosys 扫描仪的问题
- Sane：修复了一些扫描仪的双面功能
- Windows：修复了不同 DPI 的多显示器问题

Changes in 8.1.2:
- 为拆分操作添加了“对所有已选项应用”
- 修复了自动保存后“未保存更改”提示
- 修复了默认文件路径中的占位符问题
- Windows：修复了某些系统上的 OCR 错误
- Windows：修复了使用从右到左语言时的崩溃

Changes in 8.1.1:
- 修复了 dpi 选择问题

Changes in 8.1.0:
- 添加了自定义分辨率选择功能
- 添加了“新 Outlook”电子邮件提供程序
- 改进了“键盘快捷键”界面
- 修复了共享 TWAIN 扫描仪的问题
- 修复了一些按钮缺少屏幕阅读文本的问题
- 修复了 Flatpak 过大的问题

Changes in 8.0b3:
- 添加了“键盘快捷键”设置
- 添加了“--waitscan”和“--firstnow”控制台选项
- 添加了导入 zip 文件的支持
- 将 OCR 的 Tesseract 从 5.3.4 升级到 5.5.0
- 修复了杀毒软件误报问题
- 错误修复

Changes in 8.0b2:
- 修复自 7.5.3 起的问题
- 修复了 WIA 扫描
- 修复了 Windows 上的拖放
- 修复了 Linux 上的通知
- 修复了选择“使用本机 UI”时仍显示侧边栏设置的问题
- Linux flatpak 运行时已升级到 24.08

Changes in 8.0b1:
- [Beta feedback thread](https://github.com/cyanfish/naps2/discussions/467)
- 添加了扫描侧边栏
    - 可快速更改基本配置文件设置
    - 单击左下角图标（Mac 为左上角）打开/关闭
    - 管理员可以设置 [HideSidebar](https://www.naps2.com/doc/org-use#hide-sidebar) 来完全移除
- 更改了系统要求
    - 不再支持 Windows 7、8 和 8.1
    - 不再支持 32 位 Windows
    - 需要 Windows 10 1607 及以上
    - 不再支持 macOS 10.15 和 11
    - 需要 macOS 12 及以上
- 改进了对高 DPI 屏幕的支持
- Windows：添加了深色主题支持
- Twain：移除了“Legacy” twain 实现选项
- 将“Clear”图标更改为扫帚
- 性能改进
- 错误修复

Changes in 7.5.3:
- Windows：修复了最大化窗口的问题
- Mac：修复了 Apple Driver 的问题
- Linux：修复了与 Fedora 41 等的兼容性问题

Changes in 7.5.2:
- ~~NAPS2 is now available on the Microsoft Store & the Mac App Store~~
  - ~~It costs a small fee to support the developer and provide automatic updates~~
  - ~~NAPS2 will continue to be freely available at www.naps2.com~~
- Windows：安装程序和可执行文件现在采用 EV 代码签名
- 修复了 NAPS2.Console 的取消问题
- 修复了 AirSane 的 ESCL 兼容性
- 修复了 Apple Driver 的页面顺序问题
- 修复了自动保存文件提示取消不正确的问题

Changes in 7.5.1:
- Mac：使用了更多本机图标
- 修复了加载配置文件的问题

Changes in 7.5.0:
- 重新设计了设备选择
  - 驱动选择现在位于“Choose device”窗口中
  - 单击右上角按钮可在图标和列表视图之间切换
  - 不再允许在未选择设备的情况下创建配置文件
    - 若要在每次扫描时提示选择设备，必须显式选择“Always Ask”
- 添加了 ESCL 的“Manual IP”选项
- 可用配置文件选项现在根据扫描仪支持情况变化
- 改进了工作进程崩溃时的错误消息
- Sane：修复了选择错误灰度模式的问题
- 修复了包含 Unicode 的自动保存路径问题
- 修复了“Combine”黑白图像的问题

Changes in 7.4.3:
- 修复了一些 ESCL 连接问题
- 修复了 HCL Notes 的电子邮件兼容性
- 修复了“Outlook Web Access”电子邮件提供程序的问题
- 修复了某些 HP 设备的 SANE 兼容性
- 修复了 Fraktur 字体 OCR 语言的问题

Changes in 7.4.2:
- 错误修复

Changes in 7.4.1:
- 改进了 OCR 文本对齐
- 添加了 PDF 和图像文件的“Open With”支持
- 更改了一些标签以提高清晰度
  - “Automatically run OCR after scanning” → “Pre-emptively run OCR after scanning”
  - “Flip duplexed pages” → “Flip back sides of duplex pages”
- 添加了扫描仪共享的 HTTPS 支持
  - 默认情况下使用自动生成的自签名证书
  - 管理员可以设置 [EsclServerCertificatePath](https://www.naps2.com/doc/org-use#escl-server-certificate-path) 使用自定义证书
  - 管理员可以设置 [EsclSecurityPolicy](https://www.naps2.com/doc/org-use#escl-security-policy) 强制服务器/客户端仅使用 HTTPS
    - 这影响所有 ESCL 设备，而不仅仅是共享扫描仪
- 改进了 ESCL 在网络中断下的可靠性
- 修复了预览窗口缩放的一些问题
- 使确认对话框更一致（确定/取消 vs 是/否）
- 添加了更多默认键盘快捷键
- Mac：修复了键盘快捷键问题
- Mac：添加了一些缺失的菜单项（Zoom In/Out、Move Up/Down、Profiles）
- Linux：为 .deb/.rpm 包添加了签名
- Windows：.msi 安装程序不再可以用于覆盖 .exe 升级
- 错误修复

Changes in 7.4.0:
- 添加了撤销/重做（从右键菜单或 Ctrl+Z）
  - 删除无法撤销
- 添加了拆分/合并（在 Image 菜单下）
  - 拆分可用于书籍扫描以分离页面
  - 合并可用于将身份证正反两面包含在一张图像中
- 在 OCR 语言下拉列表中添加了“Multiple Languages”选项
- 添加了“Fix white balance and remove noise”OCR 选项
  - 这可以提高低质量扫描的 OCR 效果，但会使 OCR 更慢
  - 这相当于在 OCR 之前使用“Document Correction”
- 将 OCR 的 Tesseract 从 5.2.0 升级到 5.3.4
- 添加了“Show native TWAIN progress”配置文件选项（在高级下）
- 错误修复

Changes in 7.3.1:
- 改进了“Keep images across sessions”的加载时间
- PDF 加密设置现在在启用前隐藏
- 修复了某些 SANE 设备错误显示离线的问题
- 修复了某些 SANE 设备未遵守页面大小的问题
- 修复了非拉丁字母 OCR 的问题
- 错误修复

Changes in 7.3.0:
- 添加了一般“Settings”窗口，带有新选项（某些在 Mac/Linux 上不可用）：
  - 显示页码
  - 显示 Profiles 工具栏
  - Scan 菜单更改默认配置文件
  - Scan 按钮默认动作
  - Save 按钮默认动作
  - 保存后清除图像
  - 跨会话保留图像
  - 仅允许单个 NAPS2 实例
- 添加了相应的 appsettings.xml 选项
  - 见 https://www.naps2.com/doc/org-use
- 为某些 appsettings.xml 设置添加了 mode 属性：
  - mode="default" 提供用户默认值
  - mode="lock" 防止用户更改值
- 添加了新的控制台选项：
  - "--noprofile" 仅使用 CLI 选项（不使用 GUI 配置文件）
  - "--listdevices" 查看可用扫描设备
  - "--driver", "--device", "--source", "--pagesize", "--dpi", "--bitdepth" 扫描选项
  - "--deskew", "--rotate" 后处理选项
  - 见 https://www.naps2.com/doc/command-line
- Windows：更新了 .exe 安装程序样式

Changes in 7.2.2:
- 错误修复

Changes in 7.2.1:
- 错误修复

Changes in 7.2.0:
- Scanner Sharing
  - 与本地网络上的其他计算机共享扫描仪，例如：
    - 将桌面连接的 USB 扫描仪变成可由笔记本或手机使用的无线扫描仪
    - 允许仅 Windows 支持的扫描仪通过虚拟机在 Mac/Linux 上使用
    - 设置 Raspberry Pi 将 USB 扫描仪变成无线扫描仪
  - 在主机计算机的 Profiles 窗口中，单击 Scanner Sharing 并选择要共享的扫描仪
  - 在客户端计算机上，在配置文件设置中选择“ESCL Driver”，应能选择共享扫描仪
  - 共享需要主机保持打开
  - 共享扫描仪可用于任何支持 ESCL 的客户端，而不仅限于 NAPS2
    - 试试 Android 的 [Mopria Scan](https://play.google.com/store/apps/details?id=org.mopria.scan.application)
  - 在 appsettings.xml 中使用 NoScannerSharing 可禁用
- 配置文件窗口中的图标略有更新
- 启动时会清理旧的不可恢复文件
- Mac/Linux 已升级到 .NET 8 运行时
- Linux flatpak 运行时已升级到 23.08
- 错误修复

Changes in 7.1.2:
- Mac：修复了 macOS 14 Sonoma 的扫描问题

Changes in 7.1.1:
- 错误修复

Changes in 7.1.0:
- Windows：添加了 ESCL Driver 选项
  - 这允许使用大多数网络扫描仪而无需安装单独驱动
- 在某些情况下 PDF 保存速度大幅提升
- 导入的 PDF 现在呈现表单和注释
- 添加了印地语
- 错误修复
- NAPS2.Sdk 现已在 [Nuget](https://www.nuget.org/packages/NAPS2.Sdk) 上可用

Changes in 7.0b9:
- 提高了 PDF 页面大小的准确性
- OCR 运行时改进了 UI 响应速度
- Mac：提高了使用 Apple Driver 扫描时的色彩准确性
- Mac：添加了深色主题支持
- Linux：添加了深色主题支持
- Linux：添加了 arm64 .deb/.rpm 构建
- 错误修复

Changes in 7.0b8:
- 为 Mac 和 Linux 添加了“Email PDF”支持
  - Mac：Apple Mail、Gmail 和 Outlook Web 选项
  - Linux：Thunderbird、Gmail 和 Outlook Web 选项
- 为 Mac 和 Linux 添加了“Print”支持
- 为 Mac 和 Linux 添加了通知
  - 还更新了通知外观
- Linux：添加了拖放支持
- Linux：提高了与旧版 Linux（例如 Ubuntu 18.04）的兼容性
- Linux：在 .deb 包中添加了依赖项
- Sane：为 escl/airscan 后端显示 IP 地址
- Windows：将安装程序发布者更改为“NAPS2 Software”
- 改进了错误日志格式
- 添加了扫描诊断调试日志
  - 在 About 窗口勾选“Enable debug logging”
  - 这将在磁盘上记录扫描活动信息
  - 可以在与 errorlog.txt 相同的文件夹中找到 debuglog.txt
  - 在 appsettings.xml 中使用 NoDebugLogging 可隐藏该选项
- 添加了波斯尼亚语和印尼语
- 错误修复

Changes in 7.0b7:
- 错误修复

Changes in 7.0b6:
- 错误修复

Changes in 7.0b5:
- 添加了 2400/4800 dpi 选项
- Linux：添加了 .deb/.rpm 包
- Sane：显示设备时采用增量显示（仅适用于 Mac / Linux flatpak）
- 裁剪改进
- 修复了非 NAPS2 PDF 的 OCR 格式问题
- 错误修复

Changes in 7.0b4:
- Twain：更改了默认传输模式
  - “Alternative Transfer” 已重命名为“Memory Transfer”，并在选择“Default”时使用
  - “Native Transfer” 可用于恢复旧传输模式
- 保存的图像现在使用优化位深以减小文件大小
- 错误修复

Changes in 7.0b3:
- 错误修复

Changes in 7.0b2:
- 错误修复

Changes in 7.0b1:
- 大多数 NAPS2 代码已重写。界面大体相同，但内部差异很多。
    - [Beta feedback thread](https://github.com/cyanfish/naps2/discussions/35)
- 添加了 Mac 支持
    - 支持 macOS 10.15 及更高版本
    - 通用下载应适用于所有用户。若知晓 Mac 型号，可使用 Intel/Apple Silicon 下载以减小大小。
    - Mac 版 NAPS2 捆绑了用于 USB 扫描仪的 SANE 驱动，即使在新 M1/M2 Mac 上也可使用支持的扫描仪（通常没有制造商提供驱动时无法使用）。
- 添加了原生 Linux 支持
    - 需要 Flatpak 安装 (https://flatpak.org/setup/)
    - 不再需要 Mono
    - UI 现在应感觉像本机 Linux 应用
    - 性能和稳定性大幅提升
- TWAIN 支持已重构
    - 一些生命周期相关问题应已修复（例如只能扫描一次）
    - 使用“Use predefined settings”时，TWAIN 现在使用内置 NAPS2 进度窗口，允许多任务操作
    - TWAIN UI 不应再在控制台和批处理模式下可见
    - TWAIN 现在还应支持扫描更大图像（例如 1200dpi），不会出现内存不足问题
- 将 OCR 的 Tesseract 升级至 5.2.0
    - OCR 性能提高最多 30%
    - Tesseract 现在与 NAPS2 下载捆绑，因此无需额外下载（如果没有语言数据，仍需下载）
- PDF 导入和导出已重写为使用 Pdfium
    - 这意味着对不同类型 PDF 的支持更好
    - 在某些情况下导入/导出更快
    - Pdfium 已随 NAPS2 下载捆绑，因此不再需要额外下载以导入非 NAPS2 PDF
- 新的裁剪 UI
- 对空白页检测进行了小幅调整
- 图像列表调整
    - 选中图像仅以蓝色边框显示
    - 已优化间距
- 新的自动图像校正功能（进行中）
    - 图像菜单下的“Document Correction”
    - 自动修正颜色校准、噪点、倾斜和其他常见扫描问题
    - 未来此功能将集成到配置文件中
- 添加了导入/保存 JPEG2000 支持（目前仅限 Mac）
- 删除了少用的图像文件格式支持（.emf、.exif、.gif）
    - 如果需要请提出请求
- Windows 版 NAPS2 现在需要 .NET Framework 4.6.2
    - 这意味着不再支持 Windows XP
    - Windows 7 SP1 现在为最低要求
- 64 位 Windows 的安装位置现在为“Program Files”而非“Program Files (x86)”
- MSI 安装程序现在具有独立的 64 位和 32 位下载
- AppData 格式的 config.xml 和 Tesseract 文件已更改（将自动迁移）
- 改进了图标质量
- 翻译已迁移到 Crowdin
    - 见 [translate.naps2.com](https://translate.naps2.com)
- 各种性能和稳定性改进
- 错误修复

Changes in 6.1.2:
- 为 NAPS2.Console 添加了 Gmail 的 --autosend 支持
- 错误修复

Changes in 6.1.1:
- deskew 更快且更准确
- 错误修复

Changes in 6.1.0:
- 在 PDF 设置中添加了“单页文件”选项
- 改进了可访问性
- 裁剪更快
- 事件日志现在使用 XML 格式
- 错误修复

Changes in 6.0b4:
- Beta 反馈线程：https://sourceforge.net/p/naps2/discussion/general/thread/8776c818/
- 将 WIA 版本从 1.0 升级到 2.0；可在配置文件高级中更改回去
- 改进了 WIA 对进纸器和双面扫描的兼容性
- 添加了 WIA 后台扫描支持
    - “Use native UI” 下不可用
    - 这意味着可以同时使用多个设备扫描
- 删除了一些过时的 WIA 兼容性选项
- 错误修复

Changes in 6.0b3:
- Beta 反馈线程：https://sourceforge.net/p/naps2/discussion/general/thread/8776c818/
- 添加了可选事件日志
    - 见 https://www.naps2.com/doc-org-use.html#event-logging
- 改进了控制台导入速度
- 错误修复

Changes in 6.0b2:
- Beta 反馈线程：https://sourceforge.net/p/naps2/discussion/general/thread/8776c818/
- 6.0b1 的 OCR 用户需要点击 OCR 按钮并重新下载
- 修复了某些系统上 OCR 缺少 DLL 的问题
- 修复了 OCR 无法终止的问题
- 其他小修复和改进

Changes in 6.0b1:
- Beta 反馈线程：https://sourceforge.net/p/naps2/discussion/general/thread/8776c818/
- Linux 支持（下载可移植归档之一 - 目前实验版，请反馈！）
    - 需要 Mono（5.17+ 更好），见 https://www.naps2.com/doc-getting-started.html#system-requirements
- 添加自动更新检查
    - 在“关于”窗口中选择加入
    - MSI 安装版不可用
- 新 OCR 版本，在许多情况下精度显著提高
    - OCR 按钮会提示更新。可通过 appsettings.xml 中的 NoUpdatePrompt 标志禁用
    - Windows XP 不支持（将使用较旧版本）
    - 可在多个模式间选择：Fast（推荐）、Best（慢）和 Legacy（模拟旧版本）
- 添加了选择电子邮件提供程序的功能
    - 首次点击 Email PDF 时会提示选择。随后可在 Email Settings 中更改
    - 可在已安装客户端之间切换（Outlook、Thunderbird 等）
    - 支持 Gmail 和 Outlook Web Access 的 Web 邮件集成
- 添加了电子邮件附件名称的 Unicode 支持
- 裁剪选择将被记住（如果您在裁剪多个图像时需逐个调整）
- 添加了在后台运行大多数操作的能力以便多任务处理
- 改进了大图像处理性能
- 大幅减少了安装程序和可移植 ZIP 的大小
- 在控制台和批处理模式下最小化了 TWAIN UI
- NAPS2 安装程序现在已签名
    - 这最终应有助于减少 SmartScreen 通知
- 兼容系统上 NAPS2 现在将在 64 位模式下运行
    - 如果系统为 64 位，NAPS2 将更好地处理内存密集型操作
    - 如果您下载了用于打开任何 PDF 的附加组件（gsdll32.dll），可能需要重新下载 64 位版本
- 改进了开发者文档和可用性（见 https://www.naps2.com/doc-dev-onboarding.html）
- 错误修复

Changes in 5.8.2:
- 添加了日语
- 修复了导入某些 PDF 时的错误
- 修复了 Alternative Transfer TWAIN 选项的错误

Changes in 5.8.1:
- 修复了 PDF/A 支持的错误

Changes in 5.8.0:
- PDF/A 支持
    - 支持 PDF/A1-b、PDF/A2-b、PDF/A3-b 和 PDF/A3-u
    - 在“保存 PDF”菜单中点击“PDF 设置”，并在“兼容性”下选择
    - 在 NAPS2.Console 中使用 --pdfcompat。见 www.naps2.com/doc-command-line.html#pdf-options
    - 在 appsettings.xml 中使用 ForcePdfCompat。见 www.naps2.com/doc-org-use.html#force-pdf-compat
- TIFF 更改
    - 默认情况下对黑白 TIFF 文件进行更好压缩
    - 在图像设置中添加了“Compression”选项
    - 在图像设置中添加了“Single page files”选项，可防止保存多页 TIFF 文件
    - 在 NAPS2.Console 中使用 --tiffcomp 和 --split。见 www.naps2.com/doc-command-line.html#image-options
- 捐赠按钮
    - “关于”窗口现在有捐赠按钮
    - 使用一个月后会显示不显眼的捐赠提示
    - 在 appsettings.xml 中使用 HideDonateButton 可禁用两者。见 www.naps2.com/doc-org-use.html#hide-donate-button
    - MSI 发行版默认禁用该提示
- 为 EXE 安装向导添加了多语言支持

Changes in 5.7.1:
- 为 NAPS2.Console 添加了 --split、--splitscans、--splitpatcht 和 --splitsize 选项
    - 见 www.naps2.com/doc-command-line.html#split-options
- 为 NAPS2.Console 的 --import 添加了切片支持
    - 见 www.naps2.com/doc-command-line.html#slicing-imported-files

Changes in 5.7.0:
- 修复了 OCR 等下载问题
- 改进了 deskew
- 添加了批处理取消确认
- 轻微性能提升
- 错误修复

Changes in 5.6.2:
- 错误修复

Changes in 5.6.1:
- 修复了崩溃

Changes in 5.6.0:
- 将最大缩略图尺寸从 256x256 提高到 1024x1024
- 改进了 PDF 导入以允许更多类型的 PDF 导入
- 导入的 PDF 现在可以使用 OCR（如果其中尚未包含文本）
- 改进了部分黑白图像的 PDF 文件大小
- 将亮度和对比度调整合并到单个窗口
- 添加了色相、饱和度、黑白和锐化图像调整
- 在预览窗口中添加了更多键盘快捷键（箭头键换页，Ctrl/Alt/Shift + 箭头键平移）
- 通过 appsettings.xml 添加了 HideImportButton、HideOcrButton、HideSavePdfButton 和 HideSaveImages 选项
- 通过 appsettings.xml 添加了 OcrState 和 OcrDefaultLanguage 选项
- 错误修复

Changes in 5.5.0:
- 添加了导入任意 PDF 的支持（需要额外下载，可通过 appsettings.xml 中的 NoUpdatePrompt 或 DisableGenericPdfImport 禁用）
- 添加了使用 NAPS2.Console 安装可选组件的能力（使用 “--install” 参数）
- 添加了 TWAIN 兼容选项“Alternative Transfer”
- 将许可证/贡献者文件名添加了 .txt 扩展名
- 错误修复

Changes in 5.4.0:
- 添加了自动 deskew 选项（在 Rotate 菜单或配置文件设置的高级下）（感谢 Peter Hommel）
- 在预览窗口中添加了单页保存按钮
- 为自动保存设置添加了“Prompt for file path”选项
- 将“Force matching page size”拆分为“Stretch to page size”和“Crop to page size”选项
- 添加了 WIA 兼容选项“Retry on failure”和“Delay between scans”
- 添加了大多数路径环境变量支持
- 将 LICENSE 和 CONTRIBUTORS 文件添加到根目录（替代了大多数其他地方的版权声明）
- 添加了 Nynorsk 语言
- 错误修复

Changes in 5.3.3:
- 错误修复

Changes in 5.3.2:
- 添加了斯洛文尼亚语
- 修复了 AV 误报问题

Changes in 5.3.1:
- 添加了南非荷兰语和越南语

Changes in 5.3.0:
- 显著提高了多核心系统上 OCR 的速度
- 改进了 OCR 文本对齐
- Patch-T 现在支持所有扫描仪，适用于 WIA 和 TWAIN
- 改进并补充了一些错误信息的技术细节
- 调整了缩略图之间的间距以减少空间浪费
- 添加了拉脱维亚语
- 修复了 Windows XP 上的 OCR 问题（需要额外下载，可通过 appsettings.xml 中的 NoUpdatePrompt 禁用）
- 修复了当指定目录而非文件路径时自动保存和批处理默认文件名的问题

Changes in 5.2.1:
- 向 appsettings.xml 添加了 OcrTimeoutInSeconds 选项
- 错误修复

Changes in 5.2.0:
- 添加了复制/粘贴和拖放配置文件的功能
- 更改了 LockSystemProfiles 的行为，允许用户在管理员未指定时指定设备
- 向 appsettings.xml 添加了 NoUserProfiles、AlwaysRememberDevice 和 LockUnspecifiedDevices 选项
- 向 appsettings.xml 添加了 HideEmailButton 和 HidePrintButton 选项
- 向 appsettings.xml 添加了 SaveButtonDefaultAction 选项的 “PromptIfSelected” 值
- 添加了阿拉伯语、塞尔维亚语（拉丁 + 西里尔）和斯洛伐克语

Changes in 5.1.1:
- 更新了默认 appsettings.xml 以便更易编辑
- 错误修复

Changes in 5.1.0:
- 自定义页面尺寸现在可命名并在多个配置文件之间重用
- 在自定义旋转中添加了绘制对齐线的功能
- 在高级配置文件设置中添加了“恢复默认值”按钮
- 向 appsettings.xml 添加了 ComponentsPath 选项
- 向 appsettings.xml 添加了 SingleInstance 选项
- 现在可在 NAPS2.Console 的 --subject 和 --body 参数中使用占位符
- 错误修复

Changes in 5.0b3:
- 添加了保存通知（可在 appsettings.xml 中使用 DisableSaveNotifications 禁用）
- 添加了 PDF 和图像设置中的“跳过保存提示”选项。还将“默认文件名”更改为“默认文件路径”（现在可为文件名、文件夹或完整路径）
- 错误修复

Changes in 5.0b2:
- 添加了“Flip duplexed pages”兼容选项
- 向 appsettings.xml 添加了 DeleteAfterSaving 选项
- 错误修复

Changes in 5.0b1:
- 更新了 tesseract-ocr（从 3.02 到 3.04）
    - OCR 按钮会提示更新。可通过 appsettings.xml 中的 NoUpdatePrompt 标志禁用
    - 如果您已有旧版本，它仍会正常运行
- 更新了默认 TWAIN 实现
    - 在高级配置文件设置中选择“Old DSM”实现即可恢复
- 将配置文件设置中的默认水平对齐从左改为右，以匹配大多数扫描仪
    - 如果您部署自己的 appsettings.xml，则指定的对齐方式仍会继续用作默认值
- 向 appsettings.xml 添加了 LockSystemProfiles 标志，使管理员更好地控制用户配置文件
    - 见 www.naps2.com/doc-org-use.html#lock-system-profiles
- 添加了“Offset width based on alignment (WIA)”兼容选项（针对 ticket #124）
- 向安装程序添加了波斯语和韩语

Changes in 4.7.2:
- 修复了 TWAIN 问题

Changes in 4.7.1:
- 提高了 64 位系统上的内存能力
- 修复了 WIA 问题

Changes in 4.7.0:
- 为 NAPS2.Console 添加了使用自动保存设置 (-a/--autosave) 的选项
- 在预览窗口中添加了点击拖动滚动
- 改进了裁剪（现在可以点击拖动选择区域）
- 为某些 WIA 错误（例如设备忙）添加了更具描述性的错误消息
- 修复了左右工具栏布局中按钮对齐的问题
- 添加了韩语、立陶宛语和波斯语
- 各种性能改进
- 各种错误修复

Changes in 4.6.1:
- 错误修复

Changes in 4.6.0:
- 新功能：排除空白页（在配置文件设置的“高级”下）
- 为 NAPS2.Console 添加了重新排序的新选项（例如交错）
- appsettings.xml 中的键盘快捷键现在可自定义（并添加了一些默认快捷键）
- 导入时可选文件类型筛选
- 一次导入多个文件现在排序更好
- 修复了 WIA 扫描页面左侧被裁剪的问题
- 其他错误修复

Changes in 4.5.1:
- 改进了编辑和重新排列缩略图时的性能
- 拖动缩略图向上或向下时会自动滚动缩略图列表
- 拖动缩略图时显示指示器以显示放置位置
- 修复了泰语/他加禄语 OCR 语言下载
- 修复了小翻译问题

Changes in 4.5.0:
- 新的导入、保存等进度对话框，支持取消
- 更好的对比度实现
- 编辑和重新排序图像时，已选图像现在保持在视图中
- 点击“保存 PDF”、“保存图像”和“邮件 PDF”的默认操作可在 appsettings.xml 中配置（SaveAll、SaveSelected 或 AlwaysPrompt）
- 为 NAPS2.exe 添加了新的命令行选项，以启用/禁用便携版扫描仪物理“扫描”按钮的扫描（"/RegisterSti"、"/UnregisterSti" 和 "/Silent"）
- 改进了 TWAIN 错误日志记录
- 错误修复

Changes in 4.4.1:
- 主窗体中的工具栏位置会被记住
- 错误修复

Changes in 4.4.0:
- 新功能：当您按扫描仪上的物理“扫描”按钮时，NAPS2 可以启动并/或立即扫描（安装后需要重启）
- 在主窗口上下文菜单中添加了“删除”
- 修复了黑白图像旋转/裁剪后文件大小的问题
- 修复了 OCR 下载进度窗口中的取消问题
- 修复了默认配置文件逻辑问题
- 修复了各种翻译相关问题

Changes in 4.3.1:
- 错误修复

Changes in 4.3.0:
- 新功能：批量扫描（在 Scan 菜单下）
- 新功能：批量图像编辑（亮度/对比度/裁剪/自定义旋转）
- 添加了“Alternative Interleave”功能，用于以不同顺序交错双面页面
- 添加了芬兰语
- 错误修复

Changes in 4.2.3:
- 添加了希腊语和爱沙尼亚语
- 添加了命令行对多个 OCR 语言的支持（例如 "--ocrlang eng+fra"）
- 修复了导入某些 PDF 的问题
- 修复了旋转并以某些格式保存时导致黑色背景的问题
- 修复了导致重复关闭提示的问题
- 改进了导入大型 PDF 时的响应速度

Changes in 4.2.2:
- 修复了非英语语言 OCR 的问题
- 修复了 Move Up/Down 按钮缺少翻译的问题

Changes in 4.2.1:
- 修复了从 Profiles 窗口扫描时焦点丢失的问题
- 修复了 Native WIA 无法正常工作的问题

Changes in 4.2.0:
- 在预览窗口中添加了“删除”按钮
- 为预览窗口添加了新键盘快捷键：Esc（关闭）、Page Up（上一页）、Page Down（下一页）
- 为 PDF 元数据和 OCR 文本添加了 Unicode 支持
- 错误修复

Changes in 4.1.1:
- 新语言：罗马尼亚语
- 新语言：挪威语（Bokmål）
- 错误修复

Changes in 4.1.0:
- 将“关于”窗口中的网站链接更改为 www.naps2.com
- 将“Substitutions”改为“Placeholders”，以与其他软件保持一致
- 错误修复

Changes in 4.0b3:
- 新功能：缩略图可以调整大小，更易查看
- 新功能：保存时可在 GUI 和 NAPS2.Console 中使用替换（例如 "$(YYYY)-$(MM)-$(DD) $(nn).pdf" 来包含日期和递增编号）
- 新功能：图像设置（默认文件名、jpeg 质量），以及 PDF 设置中的默认文件名设置
- 错误修复

Changes in 4.0b2:
- 新功能：PDF 设置（元数据、加密）和电子邮件设置（可更改附件名称）
- 更改了独立/便携归档格式以便更易使用
- 使用 WIA 扫描多页时不再抢占其他应用程序焦点
- 在 NAPS2.Console 中使用 WIA 扫描时不再显示单独窗口
- 错误修复

Changes in 4.0b1:
- 将 Quick Scan 功能合并到工具栏
- 将之前的 Scan 功能合并到 Profiles 窗口
- 新功能：图像编辑 - 裁剪、亮度、对比度、自定义旋转
- 新功能：增强预览窗口 - 现在可以逐张浏览图像并编辑
- 新功能：直接从 NAPS2 打印扫描图像
- 新功能：尝试退出时如果有未保存更改则提示
- 新功能：保存图像时使用的文件类型会被记住
- 添加了更多键盘快捷键（Ctrl+S 保存所有为 PDF，Ctrl+O 导入，Ctrl+Enter 扫描）

Changes in 3.3.5:
- 错误修复：添加了缺失的 OCR 语言

Changes in 3.3.4:
- 新语言：土耳其语
- 错误修复：使用 OCR 生成的 PDF 现在应适用于所有阅读器的搜索
- 错误修复：修复了某些 TWAIN 设备在扫描失败时的问题

Changes in 3.3.3:
- 小错误修复

Changes in 3.3.2:
- 错误修复

Changes in 3.3.1:
- 错误修复：修复了 TWAIN 问题

Changes in 3.3.0:
- 新功能：具有预定义设置的 TWAIN
- 新功能：命令行界面的 OCR 选项
- 新语言：中文（台湾）

Changes in 3.2.1:
- 新语言：阿尔巴尼亚语
- 错误修复：增加 OCR 的时间限制

Changes in 3.2.0:
- 新功能：自定义页面尺寸
- 添加了内置 B5 和 B4 页面尺寸选项
- 添加了 400 和 800 dpi 选项

Changes in 3.1.1:
- 新语言：瑞典语
- 错误修复：安装程序中添加了荷兰语

Changes in 3.1.0:
- 新功能：一键扫描
- 新功能：可以一键反转全部或部分页面顺序
- 新语言：克罗地亚语、荷兰语
- 错误修复：防止下载损坏的 OCR 文件
- 错误修复：解决了从送纸器扫描时的某些问题

Changes in 3.0b1:
- 新功能：OCR（光学字符识别），使 PDF 文件可搜索
- 新功能：可导入 PDF 和图像文件（例如恢复之前的扫描会话）
- 新功能：仅保存所选页面
- 新功能：可以一键重新排序（交错）页面
- 新功能：添加了右键菜单并可以复制图像到剪贴板
- 新功能：在 WIA 设置中添加了 150dpi 选项
- 错误修复：黑白图像页面大小不正确
- 错误修复：部分机型的双面扫描
- 其他各种更改和错误修复

Changes in 2.6.3:
- 添加了保加利亚语翻译
- 添加了葡萄牙语翻译

Changes in 2.6.2:
- 添加了丹麦语翻译

Changes in 2.6.1:
- 修复了清除先前扫描图像后扫描时的错误
- 修复了 NAPS2.Console 帮助文本中的错误

Changes in 2.6:
- 添加了捷克语、法语和波兰语翻译
- 修复了 EXE 安装程序中加泰罗尼亚语翻译的问题

Changes in 2.5:
- 命令行界面 (naps2.console.exe) 可以发送电子邮件
- 更多窗口可调整大小，且所有窗口记住其大小和位置
- 如果之前意外关闭，NAPS2 将提供恢复扫描图像
- 大幅减少内存使用
- 添加了希伯来语和加泰罗尼亚语翻译
- 错误修复

Changes in 2.4:
- 现在可以在不指定设备的情况下创建配置文件（扫描时将选择设备）
- 组织现在可以在 appsettings.xml 中配置一些应用程序设置（见 Wiki）
- 更新了德语翻译
- 错误修复

Changes in 2.3:
- 添加了德语和意大利语翻译

Changes in 2.2:
- 添加了俄语翻译
- 更新了乌克兰语翻译
- 各种错误修复

Changes in 2.1:
- 添加了语言下拉列表
- 添加了西班牙语和乌克兰语翻译

Changes in 2.0:
- 针对 x64 上的 TWAIN 和原生 WIA 进行了重大错误修复
- 添加了命令行界面 (naps2.console.exe)
- 添加了错误报告的日志功能
- .NET 依赖从 3.5 Client Profile 更改为 4.0 Client Profile

Changes in 1.0b2:
- 向工具栏添加了 Clear 按钮
- 添加了 Ctrl+A 快捷键以选择所有缩略图
- 现在会记住并使用最后使用的配置文件作为默认
- 修复了使用“Black and White”选项扫描时的崩溃（感谢 Peter De Leeuw）
- 修复了尝试使用离线扫描仪时的崩溃（WIA）

Changes in 1.0b1:
- 现在需要 .NET framework 3.5（或更高版本）
- 新图标
- 更好的用户体验
- 管理员不再需要保存配置文件
