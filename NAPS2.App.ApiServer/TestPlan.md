# NAPS2 App ApiServer 测试流程

## 1. 环境准备

1. 确保已安装 .NET 9 SDK。
2. 在仓库根目录执行：
   ```bash
   dotnet restore
   ```

## 2. 单元测试

1. 运行 API 服务配置和启动测试：
   ```bash
   dotnet test NAPS2.App.ApiServer.Tests/NAPS2.App.ApiServer.Tests.csproj
   ```
2. 验证测试是否通过。

## 3. 本地启动验证

1. 在项目目录启动服务：
   ```bash
   dotnet run --project NAPS2.App.ApiServer/NAPS2.App.ApiServer.csproj -- --port=8080 --host=localhost
   ```
2. 浏览器或命令行访问健康检查接口：
   ```bash
   curl http://localhost:8080/api/health
   ```
3. 访问状态和版本接口：
   ```bash
   curl http://localhost:8080/api/status
   curl http://localhost:8080/api/version
   ```

## 4. HTTPS 启用验证

1. 使用有效证书路径启动服务：
   ```bash
   dotnet run --project NAPS2.App.ApiServer/NAPS2.App.ApiServer.csproj -- --port=8443 --https --certificate=./cert.pfx --certificatepassword=yourpassword
   ```
2. 访问 HTTPS 版本的健康接口：
   ```bash
   curl -k https://localhost:8443/api/health
   ```

## 5. CORS 验证

1. 启用 CORS 并启动服务：
   ```bash
   dotnet run --project NAPS2.App.ApiServer/NAPS2.App.ApiServer.csproj -- --port=8081 --cors=true
   ```
2. 在浏览器控制台发送 Fetch 请求，并检查响应头是否包含 `Access-Control-Allow-Origin`。

## 6. API 功能验证

1. 启动服务后，调用扫描相关接口：
   ```bash
   curl -X POST http://localhost:8080/api/scan/start
   curl -X POST "http://localhost:8080/api/scan/cancel?jobId=<jobId>"
   curl http://localhost:8080/api/scan/jobs
   ```
2. 获取当前服务配置信息：
   ```bash
   curl http://localhost:8080/api/settings
   ```

## 7. 发布验证

1. 生成独立发布产物：
   ```bash
   dotnet publish NAPS2.App.ApiServer/NAPS2.App.ApiServer.csproj -c Release -o publish
   ```
2. 运行发布可执行或 DLL：
   ```bash
   dotnet publish/NAPS2.App.ApiServer.dll -- --port=8080
   ```
3. 再次验证 `/api/health`、`/api/status`、`/api/version`。
