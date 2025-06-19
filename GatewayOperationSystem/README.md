# 百炼 AI + Semantic Kernel + Milvus 智能系统

基于百炼 text-embedding-v4 + Semantic Kernel + Milvus 的智能解决方案，集成 Memory 模块，提供高效的 RAG（检索增强生成）能力。

## 🚀 项目架构

```
GatewayOperationSystem/
├── Baodian.AI.SemanticKernel/          # 核心 AI 库
│   ├── Memory/                         # Memory 模块
│   │   ├── MilvusMemoryStore.cs       # Milvus 内存存储实现
│   │   └── SemanticMemoryService.cs   # 语义内存服务
│   ├── Milvus/                        # Milvus 集成
│   ├── Services/                      # 核心服务
│   ├── Controllers/                   # API 控制器
│   └── Configuration/                 # 配置类
├── GatewayOperationSystem.API/        # Web API 项目
└── GatewayOperationSystem.Core/       # 核心业务逻辑
```

## 🎯 核心特性

### Memory 模块
- **统一的存储抽象**：支持灵活切换向量数据库
- **优化的 LLM 集成**：简化 RAG 实现流程
- **元数据管理**：完整的文档和向量元数据管理
- **混合检索能力**：支持语义搜索和关键词搜索
- **缓存与性能优化**：内置缓存机制，提升检索性能

### 百炼 text-embedding-v4 集成
- 支持 1536 维度的向量嵌入
- 高质量的中文语义理解
- 优化的向量检索性能

### Milvus 向量数据库
- 高性能向量存储和检索
- 支持多种索引类型（HNSW、IVF 等）
- 分布式架构，支持大规模数据

## 📦 安装和配置

### 1. 环境要求
- .NET 8.0 或更高版本
- Milvus 向量数据库（云服务或本地部署）

### 2. 配置设置

在 `appsettings.json` 中配置以下服务：

```json
{
  "SemanticKernel": {
    "DefaultModel": "qwen-max",
    "Provider": "AliyunBailian",
    "Models": [
      {
        "ModelName": "qwen-max",
        "Provider": "AliyunBailian",
        "ApiKey": "your-api-key",
        "Endpoint": "https://dashscope.aliyuncs.com/compatible-mode/v1"
      }
    ]
  },
  "Milvus": {
    "Endpoint": "your-milvus-endpoint",
    "Port": 443,
    "Database": "your-database",
    "Username": "your-username",
    "Password": "your-password",
    "EnableSsl": true
  },
  "AliyunEmbedding": {
    "ApiKey": "your-api-key",
    "Endpoint": "https://dashscope.aliyuncs.com/compatible-mode/v1/embeddings",
    "Model": "text-embedding-v4",
    "Dimension": 1536
  }
}
```

### 3. 服务注册

在 `Program.cs` 中注册服务：

```csharp
// 配置完整的 AI 服务（包括 Memory 模块）
builder.Services.AddBaodianAI(builder.Configuration);
```

## 🔧 API 使用

### Memory 模块 API

#### 1. 存储文档
```http
POST /api/memory/store
Content-Type: application/json

{
  "collectionName": "knowledge_base",
  "documentId": "doc_001",
  "content": "文档内容...",
  "description": "文档描述",
  "chunkSize": 1000,
  "chunkOverlap": 200
}
```

#### 2. 语义搜索
```http
POST /api/memory/search
Content-Type: application/json

{
  "collectionName": "knowledge_base",
  "query": "搜索查询",
  "limit": 5,
  "minRelevanceScore": 0.7
}
```

#### 3. 混合搜索
```http
POST /api/memory/hybrid-search
Content-Type: application/json

{
  "collectionName": "knowledge_base",
  "query": "搜索查询",
  "limit": 5,
  "minRelevanceScore": 0.7
}
```

#### 4. 创建 RAG 上下文
```http
POST /api/memory/rag-context
Content-Type: application/json

{
  "collectionName": "knowledge_base",
  "query": "用户问题",
  "maxResults": 5,
  "minRelevanceScore": 0.7
}
```

#### 5. 管理集合
```http
GET /api/memory/collections                    # 获取所有集合
GET /api/memory/collection/{collectionName}    # 获取集合信息
DELETE /api/memory/collection/{collectionName} # 删除集合
DELETE /api/memory/document/{collectionName}/{documentId} # 删除文档
```

## 💡 使用示例

### 1. 存储知识库文档

```csharp
// 注入服务
var memoryService = serviceProvider.GetRequiredService<SemanticMemoryService>();

// 存储文档
var recordIds = await memoryService.StoreDocumentAsync(
    collectionName: "company_knowledge",
    documentId: "employee_handbook",
    content: "员工手册内容...",
    description: "公司员工手册",
    chunkSize: 1000,
    chunkOverlap: 200
);
```

### 2. 语义搜索

```csharp
// 执行搜索
var results = await memoryService.SearchAsync(
    collectionName: "company_knowledge",
    query: "请假流程是什么？",
    limit: 5,
    minRelevanceScore: 0.7
);

// 处理结果
foreach (var result in results)
{
    Console.WriteLine($"相关度: {result.Relevance:F3}");
    Console.WriteLine($"内容: {result.Metadata.Text}");
    Console.WriteLine($"来源: {result.Metadata.ExternalSourceName}");
}
```

### 3. 创建 RAG 上下文

```csharp
// 创建 RAG 上下文
var ragContext = await memoryService.CreateRagContextAsync(
    collectionName: "company_knowledge",
    query: "如何申请年假？",
    maxResults: 3,
    minRelevanceScore: 0.7
);

// 使用上下文进行 LLM 对话
var prompt = $@"
基于以下相关信息回答问题：

{ragContext.Context}

问题：{ragContext.Query}

请根据上述信息提供准确的回答。
";
```

## 🔄 项目重构说明

### 删除的不必要文件
- `EmbeddingServices_Usage.md` - 使用文档已整合到 README
- `Milvus_Usage_Guide.html` - 使用文档已整合到 README
- `Samples/` 目录下的示例文件 - 已整合到 API 控制器
- `Local/MockServices.cs` - 移除 Mock 服务
- `Plugins/` 目录下的插件 - 简化插件架构
- `Controllers/ChatController.cs` - 功能已整合到 Memory 控制器
- `GatewayOperationSystem.Infrastructure/` - 空项目已删除

### 新增的核心功能
- **Memory 模块**：完整的向量存储和检索功能
- **MilvusMemoryStore**：实现 IMemoryStore 接口
- **SemanticMemoryService**：高级 RAG 功能
- **MemoryController**：完整的 API 接口
- **配置优化**：支持百炼 text-embedding-v4

### 架构优化
- 统一的服务注册方式
- 简化的配置结构
- 清晰的模块分离
- 完整的错误处理
- 详细的日志记录

## 📈 性能优化

1. **向量维度优化**：使用百炼 text-embedding-v4 的 1536 维度
2. **批量操作**：支持批量存储和检索
3. **缓存机制**：内置内存缓存
4. **异步处理**：全异步 API 设计
5. **连接池**：HTTP 连接复用

## 🔒 安全考虑

- API Key 安全存储
- HTTPS 加密传输
- 输入验证和清理
- 错误信息脱敏
- 访问控制机制

## 📝 许可证

本项目采用 MIT 许可证。

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

## 📞 支持

如有问题，请通过以下方式联系：
- 提交 GitHub Issue
- 发送邮件至项目维护者
