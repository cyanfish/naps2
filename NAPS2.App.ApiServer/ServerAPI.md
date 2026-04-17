# NAPS2 App API Server 文档

## 概述

`NAPS2.App.ApiServer` 是一个独立的 API 服务模块，基于 `EmbedIO` 实现，可独立运行并对浏览器/前端暴露本地 API。当前版本已经补齐与 NAPS2.Sdk 的扫描能力对接，支持：

- 队列化的扫描作业
- 扫描设备枚举
- 扫描设备能力查询
- 异步扫描执行与取消
- 扫描结果 PDF 导出
- HTTP/HTTPS、CORS、端口配置
- 基础状态/健康检查

## 运行方式

### 命令行启动

```bash
dotnet run --project NAPS2.App.ApiServer/NAPS2.App.ApiServer.csproj -- --port=8080 --host=localhost
```

### 常用参数

- `--port=<port>`：API 服务监听端口，默认 `8080`
- `--host=<hostname>`：绑定主机，默认 `localhost`
- `--https[=true]`：启用 HTTPS
- `--cors[=true|false]`：启用/禁用 CORS，默认启用
- `--certificate=<path>`：HTTPS 证书路径
- `--certificatepassword=<pwd>`：HTTPS 证书密码

## GUI 集成与配置保存

API 服务配置已集成到 NAPS2 主程序的设置页面：

- `ApiServerHost`
- `ApiServerPort`
- `ApiServerEnableHttps`
- `ApiServerEnableCors`

这些配置字段已新增到共享配置类 `NAPS2.Config.CommonConfig` 中，并保存到用户配置范围。

可通过以下方法从主程序配置创建 API 服务配置对象：

```csharp
var apiConfig = ApiServerConfiguration.CreateFromConfig(naps2Config);
```

## API 端点

### GET /api/health

健康检查接口。

请求：

```bash
curl http://localhost:8080/api/health
```

响应示例：

```json
{
  "healthy": true,
  "timestamp": "2026-04-17T...Z"
}
```

### GET /api/status

返回服务运行状态、端口和 CORS/HTTPS 配置。

请求：

```bash
curl http://localhost:8080/api/status
```

响应示例：

```json
{
  "status": "running",
  "version": "1.0.0",
  "port": 8080,
  "urlPrefix": "http://localhost:8080/",
  "https": false,
  "cors": true
}
```

### GET /api/version

返回当前 API 服务版本信息。

请求：

```bash
curl http://localhost:8080/api/version
```

响应示例：

```json
{
  "version": "1.0.0"
}
```

### GET /api/scan/drivers

返回可用扫描驱动列表。

请求：

```bash
curl http://localhost:8080/api/scan/drivers
```

响应示例：

```json
{
  "drivers": ["Default", "Wia", "Twain", "Apple", "Sane", "Escl"]
}
```

### GET /api/scan/devices?driver=<driver>

返回当前平台上可用的扫描设备列表。可选 `driver` 参数控制目标驱动类型，默认使用平台默认驱动。

请求：

```bash
curl "http://localhost:8080/api/scan/devices?driver=Escl"
```

响应示例：

```json
{
  "driver": "Escl",
  "devices": [
    {
      "driver": "Escl",
      "id": "http://192.168.1.100",
      "name": "Office Scanner"
    }
  ]
}
```

### GET /api/scan/caps?driver=<driver>&deviceId=<id>&deviceName=<name>

查询指定扫描设备的能力。

请求：

```bash
curl "http://localhost:8080/api/scan/caps?driver=Escl&deviceId=http://192.168.1.100&deviceName=Office%20Scanner"
```

响应示例：

```json
{
  "success": true,
  "caps": { ... }
}
```

### POST /api/scan/start

启动一个新的扫描作业。请求体应为 `ScanOptions` 对象的 JSON，支持 SDK 中驱动、设备、页面、DPI、颜色、OCR 等选项。

请求示例：

```bash
curl -X POST http://localhost:8080/api/scan/start \
  -H "Content-Type: application/json" \
  -d '{
      "driver": "Escl",
      "device": { "driver": "Escl", "id": "http://192.168.1.100", "name": "Office Scanner" },
      "paperSource": "Feeder",
      "dpi": 300,
      "pageSize": "A4",
      "quality": 85
    }'
```

响应示例：

```json
{
  "success": true,
  "jobId": "abc123",
  "status": "running",
  "startedAt": "2026-04-17T...Z",
  "message": "Scan job started. Use /api/scan/jobs to monitor progress."
}
```

### POST /api/scan/cancel?jobId=<jobId>

取消指定扫描作业。

请求：

```bash
curl -X POST "http://localhost:8080/api/scan/cancel?jobId=abc123"
```

响应示例：

```json
{
  "success": true,
  "jobId": "abc123",
  "status": "running",
  "message": "Scan job cancellation requested."
}
```

### GET /api/scan/jobs

查询当前所有扫描作业状态。

请求：

```bash
curl http://localhost:8080/api/scan/jobs
```

响应示例：

```json
{
  "jobs": [
    {
      "jobId": "abc123",
      "status": "completed",
      "message": "Scan finished successfully.",
      "startedAt": "2026-04-17T...Z",
      "completedAt": "2026-04-17T...Z",
      "pagesScanned": 2
    }
  ]
}
```

### GET /api/scan/jobs/{jobId}

查询指定扫描作业详情。

请求：

```bash
curl http://localhost:8080/api/scan/jobs/abc123
```

响应示例：

```json
{
  "success": true,
  "job": { ... },
  "device": { ... },
  "driver": "Escl",
  "options": { ... }
}
```

### GET /api/scan/jobs/{jobId}/export?format=pdf

导出扫描结果为 PDF，并以 Base64 返回。

请求：

```bash
curl "http://localhost:8080/api/scan/jobs/abc123/export?format=pdf"
```

响应示例：

```json
{
  "success": true,
  "contentType": "application/pdf",
  "data": "JVBERi0xLjQKJc..."
}
```

## 浏览器访问与 CORS

若前端网页直接访问本地 API，建议启用 CORS：

```bash
dotnet run --project NAPS2.App.ApiServer/NAPS2.App.ApiServer.csproj -- --cors=true
```

Fetch 示例：

```js
fetch('http://localhost:8080/api/health')
  .then(r => r.json())
  .then(console.log)
  .catch(console.error);
```

## HTTPS 注意

启用 HTTPS 时需提供证书：

```bash
dotnet run --project NAPS2.App.ApiServer/NAPS2.App.ApiServer.csproj -- --port=8443 --https --certificate=./cert.pfx --certificatepassword=1234
```

如果未提供证书，服务仍可使用 HTTP。

## 发布与独立运行

生成发布版本：

```bash
dotnet publish NAPS2.App.ApiServer/NAPS2.App.ApiServer.csproj -c Release -o publish
```

运行发布产物：

```bash
dotnet publish/NAPS2.App.ApiServer.dll -- --port=8080
```

## 备注

当前 API 已直接对接 NAPS2.Sdk 的扫描功能，包括设备枚举、扫描能力查询、异步扫描作业、取消和 PDF 导出。后续可继续扩展为图像导出、OCR 结果查询，并增加前端示例页面。