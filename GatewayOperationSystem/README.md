# 游乐行业闸机运营管理系统 - 配置和部署指南

基于 Semantic Kernel + Milvus 的智能闸机运营解决方案

## 📋 系统概述

本系统专为游乐行业设计，提供智能化的闸机运营管理解决方案，包括：

- **运营建议**：基于场景的智能运营建议
- **故障诊断**：快速故障诊断和解决方案
- **维护指导**：详细的设备维护指南
- **高峰期管理**：人流管理和优化建议
- **应急响应**：紧急情况处理方案
- **配置优化**：闸机系统配置优化

## 🛠️ 技术架构

- **后端框架**：ASP.NET Core 8.0
- **AI 引擎**：Microsoft Semantic Kernel 1.5.0
- **向量数据库**：Milvus
- **关系数据库**：PostgreSQL
- **嵌入模型**：OpenAI Text Embedding
- **语言模型**：支持多种模型（OpenAI GPT、通义千问、DeepSeek等）

## 🚀 快速开始

### 1. 环境准备

#### 必需软件
- .NET 8.0 SDK
- PostgreSQL 14+
- Visual Studio 2022 或 VS Code

#### API密钥准备
- **Milvus API Key**: 在 [Milvus](https://www.milvus.io/) 注册并获取API密钥
- **OpenAI API Key**: 在 [OpenAI](https://platform.openai.com/) 获取API密钥
- **通义千问 API Key**: 在 [阿里云](https://dashscope.aliyun.com/) 获取API密钥（可选）
- **DeepSeek API Key**: 在 [DeepSeek](https://platform.deepseek.com/) 获取API密钥（可选）

### 2. Milvus 配置

#### 创建 Milvus 集合
1. 登录 [Milvus Console](https://www.milvus.io/)
2. 创建新集合：
   - **Collection Name**: `amusement-gateway-knowledge`
   - **Dimensions**: `1536` (对应 OpenAI text-embedding-3-small)
   - **Metric**: `cosine`
   - **Environment**: 选择合适的区域（如 `us-east-1-aws`）

#### 配置集合元数据
集合将存储以下元数据字段：
- `title`: 知识标题
- `content`: 知识内容
- `summary`: 摘要
- `category`: 分类
- `tags`: 标签
- `created_at`: 创建时间
- `updated_at`: 更新时间

### 3. 数据库配置

#### PostgreSQL 设置
```sql
-- 创建数据库
CREATE DATABASE gateway_operation_system;

-- 创建用户（可选）
CREATE USER gateway_user WITH PASSWORD 'your_secure_password';
GRANT ALL PRIVILEGES ON DATABASE gateway_operation_system TO gateway_user;
```

### 4. 项目配置

#### 更新 appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=gateway_operation_system;Username=postgres;Password=your_password"
  },
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "ModelId": "gpt-4"
  },
  "SemanticKernel": {
    "DefaultModel": "qwen-max",
    "Models": [
      {
        "ModelName": "qwen-max",
        "ApiKey": "your-qwen-api-key",
        "Endpoint": "https://dashscope.aliyuncs.com/compatible-mode/v1",
        "MaxTokens": 2000,
        "Temperature": 0.7
      }
    ]
  },
  "Milvus": {
    "ApiKey": "your-milvus-api-key",
    "CollectionName": "amusement-gateway-knowledge",
    "Environment": "us-east-1-aws"
  }
}
```

#### 开发环境配置 (appsettings.Development.json)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning",
      "GatewayOperationSystem": "Debug"
    }
  }
}
```

### 5. 编译和运行

```bash
# 还原包依赖
dotnet restore

# 编译项目
dotnet build

# 运行项目
cd GatewayOperationSystem.API
dotnet run
```

访问 API 文档：`https://localhost:7000/swagger`

### 6. 初始化知识库

系统启动后，调用以下API端点初始化游乐行业闸机知识库：

```http
POST /api/KnowledgeBase/batch-import-amusement
```

这将导入预置的游乐行业闸机运营知识。

## 📚 API 使用指南

### 核心功能 API

#### 1. 获取运营建议
```http
POST /api/GatewayAI/operational-advice
Content-Type: application/json

{
  "scenario": "主题公园入口高峰期",
  "issue": "排队过长影响游客体验"
}
```

#### 2. 搜索解决方案
```http
POST /api/GatewayAI/search-solutions
Content-Type: application/json

{
  "query": "闸机故障处理",
  "scenario": "ThemeParkEntrance"
}
```

#### 3. 生成维护指南
```http
POST /api/GatewayAI/maintenance-guide
Content-Type: application/json

{
  "deviceModel": "XG-2000",
  "issueDescription": "读卡器无法识别季票"
}
```

#### 4. 高峰期管理建议
```http
POST /api/GatewayAI/peak-time-management
Content-Type: application/json

{
  "expectedVisitors": 5000,
  "availableGateways": ["入口A", "入口B", "VIP通道"]
}
```

#### 5. 应急响应计划
```http
POST /api/GatewayAI/emergency-response
Content-Type: application/json

{
  "emergencyType": "设备故障导致大面积停机",
  "location": "主题公园正门"
}
```

### 知识库管理 API

#### 添加知识
```http
POST /api/KnowledgeBase
Content-Type: application/json

{
  "title": "新的运营指南",
  "content": "详细的操作步骤...",
  "summary": "简要描述",
  "tags": ["运营", "指南"],
  "category": "运营管理"
}
```

#### 搜索知识
```http
POST /api/KnowledgeBase/search
Content-Type: application/json

{
  "query": "高峰期管理",
  "topK": 5
}
```

## 🔧 高级配置

### 性能优化

#### Milvus 配置优化
- **批量操作**：使用批量upsert提高写入性能
- **索引优化**：根据数据量选择合适的pod类型
- **缓存策略**：实现查询结果缓存

#### Semantic Kernel 优化
- **模型选择**：根据成本和性能需求选择合适的模型
- **并发控制**：设置合理的并发限制
- **超时设置**：配置合理的超时时间

### 安全配置

#### API 安全
```csharp
// 添加到 Program.cs
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options => {
        // JWT 配置
    });

builder.Services.AddAuthorization();
```

#### 数据加密
- 配置数据库连接加密
- API 密钥安全存储
- 敏感日志信息脱敏

### 监控和日志

#### 添加 Application Insights
```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

#### 结构化日志
```csharp
builder.Services.AddLogging(builder => {
    builder.AddConsole();
    builder.AddDebug();
    builder.AddApplicationInsights();
});
```

## 🐳 Docker 部署

### Dockerfile
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GatewayOperationSystem.API/GatewayOperationSystem.API.csproj", "GatewayOperationSystem.API/"]
RUN dotnet restore "GatewayOperationSystem.API/GatewayOperationSystem.API.csproj"
COPY . .
WORKDIR "/src/GatewayOperationSystem.API"
RUN dotnet build "GatewayOperationSystem.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GatewayOperationSystem.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GatewayOperationSystem.API.dll"]
```

### docker-compose.yml
```yaml
version: '3.8'
services:
  gateway-api:
    build: .
    ports:
      - "8080:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=gateway_operation_system;Username=postgres;Password=password
    depends_on:
      - postgres
  
  postgres:
    image: postgres:14
    environment:
      POSTGRES_DB: gateway_operation_system
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: password
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

## 🔍 故障排除

### 常见问题

#### 1. Milvus 连接失败
- 检查API密钥是否正确
- 验证集合名称和环境配置
- 检查网络连接

#### 2. 嵌入生成失败
- 检查OpenAI API密钥
- 验证模型名称是否正确
- 检查API配额

#### 3. 向量数据库连接问题
- 检查连接字符串
- 确认数据库服务运行状态
- 验证用户权限

### 日志分析

查看关键日志：
```bash
# 实时查看日志
dotnet run --verbosity diagnostic

# 查看特定组件日志
tail -f logs/gateway-*.log | grep "Milvus\|SemanticKernel"
```

## 📈 性能基准

### 推荐配置

#### 生产环境
- **CPU**: 4核心以上
- **内存**: 8GB以上
- **网络**: 稳定的互联网连接
- **数据库**: Milvus Cloud/Standalone

#### Milvus 索引配置
- **维度**: 1536 (text-embedding-3-small)
- **索引类型**: HNSW
- **度量类型**: COSINE

### 性能指标

| 操作 | 平均响应时间 | 并发处理能力 |
|------|-------------|-------------|
| 知识搜索 | < 500ms | 100 req/s |
| 运营建议生成 | 2-5s | 20 req/s |
| 知识入库 | < 200ms | 50 req/s |

## 🤝 支持和贡献

### 技术支持
- GitHub Issues: [项目Issues页面]
- 邮箱支持: support@example.com
- 文档Wiki: [项目Wiki页面]

### 贡献指南
1. Fork 项目
2. 创建功能分支
3. 提交变更
4. 发起 Pull Request

## 📝 更新日志

### v1.0.0 (2024-12-xx)
- 初始版本发布
- 集成 Semantic Kernel 1.5.0
- 支持 Milvus 向量数据库
- 游乐行业专用知识库
- RESTful API 接口

---

**注意**: 请根据实际部署环境调整配置参数，并定期更新API密钥和安全设置。
