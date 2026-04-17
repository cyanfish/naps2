# 服务监听端口与浏览器 API 暴露开发任务清单

## 目标

- 在程序内部开发一个服务，监听用户可自定义的端口。
- 将所有需要的 API 暴露给网页程序调用。
- 该服务器可以单独打包为独立可执行程序运行。

> 备注：仓库中已有 `NAPS2.Escl.Server` 使用 `EmbedIO` 的实现，可作为服务启动与 Web API 暴露的参考。

---

## 1. 需求与架构确认

1. 确认需要暴露给网页程序的 API 范围。
   - 例如：扫描状态、扫描结果查询、开始/取消扫描、用户设置、日志查询等。
2. 确定服务器是否需要支持 HTTP 和/或 HTTPS。
3. 确定浏览器端访问时是否需要支持跨域（CORS）。
4. 确定独立服务器运行模式的启动参数和配置项。
5. 确定该服务的生命周期与主程序的耦合方式。

---

## 2. 技术方案与项目结构

1. 复用或扩展现有 `NAPS2.Escl.Server` 的 `EmbedIO` 服务架构。它已经包含：
   - `WebServer` 启动逻辑
   - 端口监听
   - `WebApiController` 控制器暴露 API
2. 在仓库中新增或修改以下模块：
   - `NAPS2.App.ApiServer`（建议新增独立项目）
   - `NAPS2.Lib` 中新增通用接口/配置类
   - GUI 设置页新增端口配置项
3. 通过配置类统一管理：
   - 监听端口
   - 是否启用 HTTPS
   - CORS 开关
   - 绑定地址（如 `0.0.0.0` / `localhost`）

---

## 3. 具体开发任务清单

### 3.1 新增通用服务接口与配置

- [ ] 定义 `IApiServer` 接口
  - `Task StartAsync()`
  - `Task StopAsync()`
  - `bool IsRunning { get; }`
  - `int Port { get; }`
- [ ] 创建 `ApiServerConfiguration` 类
  - `int Port`
  - `bool EnableHttps`
  - `bool EnableCors`
  - `string Host` / `string UrlPrefix`
  - `string? CertificatePath` / `string? CertificatePassword`
- [ ] 在共享库中放置配置与接口，供 GUI、命令行与独立服务器共享。

### 3.2 实现 Web API 服务器模块

- [ ] 新建 `NAPS2.App.ApiServer` 项目（或在现有项目中新增模块）
- [ ] 实现 `EmbedIO` `WebServer` 启动逻辑
  - 支持自定义端口
  - 支持 HTTP/HTTPS
  - 支持 CORS
- [ ] 创建基础 API 控制器
  - `ApiRootController` 或 `NapsApiController`
  - 提供 `/api/status`, `/api/health`, `/api/version` 等基础接口
- [ ] 定义并实现业务 API 控制器
  - 比如 `/api/scan/start`, `/api/scan/cancel`, `/api/scan/jobs`, `/api/settings`
- [ ] 实现请求路由与 JSON/XML 返回格式
- [ ] 处理未捕获异常并记录日志

### 3.3 端口配置与程序内设置

- [ ] 在主程序 GUI 中增加“API 服务端口”配置项
  - 可输入端口号
  - 验证端口合法性（1024-65535，未被占用）
- [ ] 支持保存并加载端口配置
- [ ] 在程序启动或服务启动时读取配置
- [ ] 支持“立即启动/停止 API 服务”按钮（如果适用）

### 3.4 单独打包和运行

- [ ] 为独立服务创建可执行入口 `Program.cs`
  - 从命令行参数或配置文件读取端口和运行模式
- [ ] 支持 `dotnet publish` 打包
  - 生成可单独运行的发布文件夹
- [ ] 提供示例运行方式
  - `dotnet NAPS2.App.ApiServer.dll --port 8080`
  - 或生成平台二进制可执行文件
- [ ] 若需要，增加安装/部署说明

### 3.5 测试与验证

- [ ] 编写单元测试
  - 对 `ApiServerConfiguration` 验证逻辑
  - 对 API 控制器行为测试
- [ ] 编写集成测试
  - 启动服务器后访问 `/api/health`、`/api/version`
  - 测试端口可配置性
  - 测试 CORS 响应头
- [ ] 验证独立服务器运行场景
  - 通过发布产物单独执行
  - 验证端口监听、启动/停止流程

### 3.6 文档与示例

- [ ] 编写开发文档
  - 新增 `README` 或 `docs` 中的 API 服务说明
- [ ] 编写使用文档
  - 如何配置端口
  - 如何在浏览器中调用 API
  - 如何单独打包运行服务
- [ ] 编写示例页面或客户端调用说明
  - Ajax/Fetch 调用示例
  - 必要时说明跨域设置

---

## 4. 交付内容建议

- `NAPS2.App.ApiServer` 项目（独立可运行）
- 共享接口与配置类
- GUI 端口配置支持
- 可自定义端口的 Web API 服务
- 单独打包发布方案
- 测试用例与使用文档

---

## 5. 开发优先级建议

1. 首先实现通用服务器配置与启动框架。
2. 其次实现端口自定义与基础路由。
3. 再实现 GUI 配置、独立运行入口与打包。
4. 最后补齐测试与文档。

---

## 6. 关键注意点

- 浏览器调用时必须处理 CORS，否则网页程序可能无法直接访问本地服务。
- 自定义端口时需要检查端口占用与权限。
- 单独打包运行时，服务应该保持与主程序逻辑解耦。
- 若支持 HTTPS，证书管理需要额外设计。
