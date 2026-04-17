# NAPS2 Web 扫描客户端

这是一个基于 HTML + JavaScript 的静态 Web 客户端，用于访问 `NAPS2.App.ApiServer` 提供的扫描 API。

## 功能覆盖

- 服务器健康检查、服务状态、版本信息和当前配置
- 列出支持的扫描驱动程序
- 列出并选择扫描设备
- 查询设备扫描能力
- 填写扫描参数并启动扫描作业
- 刷新扫描作业列表、查看作业详情
- 取消扫描作业
- 导出已完成作业为 PDF 下载

## 运行方法

1. 启动 API 服务：

```bash
cd /workspaces/naps2
dotnet run --project NAPS2.App.ApiServer/NAPS2.App.ApiServer.csproj -- --port=8080 --host=localhost
```

2. 在另一个终端中启动静态文件服务器：

```bash
cd /workspaces/naps2/NAPS2.App.ApiServer.WebApp
python3 -m http.server 5500
```

3. 在浏览器中打开：

```text
http://localhost:5500
```

## 注意

- API Server 默认启用了 CORS，可直接从浏览器访问。
- 如果使用其他端口，请在页面顶部修改 `API 地址` 字段为相应地址，例如：`http://localhost:8080/api`。
- 该网页客户端是对主程序核心扫描流程的 Web 版本演示，适合验证设备枚举、扫描启动、作业监控和 PDF 导出。
