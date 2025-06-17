# 免费 AI 服务配置指南

本项目已配置为支持多种免费的 AI 服务，无需依赖 OpenAI。以下是可用的选项：

## 1. Ollama (推荐 - 完全本地运行)

### 安装步骤：
1. 下载并安装 Ollama: https://ollama.com/
2. 安装模型: `ollama pull llama3.2:3b`
3. 启动服务: `ollama serve`

### 配置示例：
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

## 2. LM Studio (图形界面本地运行)

### 安装步骤：
1. 下载 LM Studio: https://lmstudio.ai/
2. 下载模型（推荐 Llama 3.2 3B）
3. 启动本地服务器

### 配置示例：
```json
{
  "SemanticKernel": {
    "DefaultModel": "llama-3.2-3b-instruct",
    "Provider": "OpenAI-Compatible",
    "Models": [
      {
        "Provider": "OpenAI-Compatible",
        "ModelName": "llama-3.2-3b-instruct",
        "ApiKey": "lm-studio",
        "Endpoint": "http://localhost:1234/v1",
        "MaxTokens": 2000,
        "Temperature": 0.7,
        "EnableStreaming": false
      }
    ]
  }
}
```

## 3. 国产免费服务

### 硅基流动（推荐）
- 网站: https://siliconflow.cn/
- 提供免费的 API 调用额度
- 支持多种开源模型

### 配置示例：
```json
{
  "SemanticKernel": {
    "DefaultModel": "Qwen/Qwen2.5-7B-Instruct",
    "Provider": "OpenAI-Compatible",
    "Models": [
      {
        "Provider": "OpenAI-Compatible",
        "ModelName": "Qwen/Qwen2.5-7B-Instruct",
        "ApiKey": "your-siliconflow-api-key",
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
- 网站: https://platform.lingyiwanwu.com/
- 提供免费额度

### 配置示例：
```json
{
  "SemanticKernel": {
    "DefaultModel": "yi-34b-chat-0205",
    "Provider": "OpenAI-Compatible",
    "Models": [
      {
        "Provider": "OpenAI-Compatible",
        "ModelName": "yi-34b-chat-0205",
        "ApiKey": "your-yi-api-key",
        "Endpoint": "https://api.lingyiwanwu.com/v1",
        "MaxTokens": 2000,
        "Temperature": 0.7,
        "EnableStreaming": false
      }
    ]
  }
}
```

## 4. 其他选项

### Azure OpenAI（企业用户）
如果您有 Azure 订阅，可以使用 Azure OpenAI：

```json
{
  "SemanticKernel": {
    "DefaultModel": "gpt-35-turbo",
    "Provider": "AzureOpenAI",
    "Models": [
      {
        "Provider": "AzureOpenAI",
        "ModelName": "gpt-35-turbo",
        "ApiKey": "your-azure-openai-key",
        "Endpoint": "https://your-resource.openai.azure.com/",
        "MaxTokens": 2000,
        "Temperature": 0.7,
        "EnableStreaming": false
      }
    ]
  }
}
```

## 推荐方案

1. **最佳本地方案**: Ollama（完全免费，无网络依赖）
2. **最佳云端方案**: 硅基流动（国内服务，免费额度）
3. **最简单方案**: LM Studio（图形界面，易于使用）

## 使用说明

1. 选择上述任意一种方案
2. 更新 `appsettings.json` 中的配置
3. 重启应用程序
4. 系统将自动使用新的 AI 服务

所有方案都与项目完全兼容，无需修改代码。
