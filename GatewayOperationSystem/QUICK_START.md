# 快速开始 - 无需 OpenAI

您现在可以使用多种免费的 AI 服务来运行这个游乐园闸机智能运营系统，无需 OpenAI API 密钥。

## 推荐方案：Ollama (完全免费，本地运行)

### 1. 安装 Ollama

```bash
# Windows: 下载安装包
# https://ollama.com/download/windows

# macOS
brew install ollama

# Linux
curl -fsSL https://ollama.com/install.sh | sh
```

### 2. 安装模型

```bash
# 安装小型模型 (推荐，约 2GB)
ollama pull llama3.2:3b

# 或安装更好的模型 (约 4GB)
ollama pull llama3.2:7b
```

### 3. 启动服务

```bash
ollama serve
```

### 4. 更新配置

项目已默认配置使用 Ollama，你只需要确保 `appsettings.json` 中的配置正确：

```json
{
  "SemanticKernel": {
    "DefaultModel": "llama3.2:3b",
    "Provider": "OpenAI-Compatible",
    "Models": [
      {
        "Provider": "OpenAI-Compatible",
        "ModelName": "llama3.2:3b",
        "ApiKey": "dummy-key",
        "Endpoint": "http://localhost:11434/v1",
        "MaxTokens": 2000,
        "Temperature": 0.7,
        "EnableStreaming": false
      }
    ]
  }
}
```

### 5. 运行项目

```bash
cd GatewayOperationSystem.API
dotnet run
```

## 替代方案

如果您不想安装 Ollama，可以使用以下云服务：

### 硅基流动（国内免费服务）

1. 注册账户: https://siliconflow.cn/
2. 获取 API 密钥
3. 更新配置：

```json
{
  "SemanticKernel": {
    "DefaultModel": "Qwen/Qwen2.5-7B-Instruct",
    "Provider": "OpenAI-Compatible",
    "Models": [
      {
        "Provider": "OpenAI-Compatible",
        "ModelName": "Qwen/Qwen2.5-7B-Instruct",
        "ApiKey": "你的硅基流动API密钥",
        "Endpoint": "https://api.siliconflow.cn/v1",
        "MaxTokens": 2000,
        "Temperature": 0.7,
        "EnableStreaming": false
      }
    ]
  }
}
```

### 零一万物

1. 注册账户: https://platform.lingyiwanwu.com/
2. 获取 API 密钥
3. 更新配置：

```json
{
  "SemanticKernel": {
    "DefaultModel": "yi-34b-chat-0205",
    "Provider": "OpenAI-Compatible",
    "Models": [
      {
        "Provider": "OpenAI-Compatible",
        "ModelName": "yi-34b-chat-0205",
        "ApiKey": "你的零一万物API密钥",
        "Endpoint": "https://api.lingyiwanwu.com/v1",
        "MaxTokens": 2000,
        "Temperature": 0.7,
        "EnableStreaming": false
      }
    ]
  }
}
```

## 功能测试

启动项目后，您可以访问：

- Swagger UI: http://localhost:5020/swagger
- 知识库 API: http://localhost:5020/api/knowledgebase
- AI 咨询 API: http://localhost:5020/api/gatewayai

## 注意事项

1. 首次运行 Ollama 模型可能需要一些时间来加载
2. 确保有足够的内存来运行本地模型 (至少 4GB)
3. 如果使用云服务，请注意免费额度限制

## 获取帮助

如果遇到问题，请检查：
1. Ollama 服务是否正在运行 (`ollama list` 查看已安装模型)
2. 配置文件中的端点地址是否正确
3. 防火墙设置是否允许本地连接

项目现在完全不依赖 OpenAI，您可以安全地使用各种免费替代方案。
