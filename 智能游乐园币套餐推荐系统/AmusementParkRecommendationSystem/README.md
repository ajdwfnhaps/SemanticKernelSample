# 智能游乐园币套餐推荐系统

基于.NET 8和Microsoft Semantic Kernel构建的AI驱动的游乐园管理系统，能够根据会员的消费数据智能推荐最适合的币套餐。

## 功能特性

### 🎯 核心功能
- **智能推荐算法**: 使用AI分析会员消费模式，推荐最适合的币套餐
- **会员管理**: 完整的会员信息管理和消费历史跟踪
- **套餐管理**: 灵活的币套餐配置和管理系统
- **数据分析**: 深入的消费行为分析和统计

### 🤖 AI能力
- **Semantic Kernel集成**: 使用Microsoft Semantic Kernel框架
- **自然语言处理**: 智能分析和生成推荐理由
- **函数调用**: 通过AI函数获取实时业务数据
- **多模型支持**: 兼容OpenAI和Azure OpenAI服务

### 📊 业务逻辑
- **消费模式分析**: 分析会员的游戏偏好和消费频率
- **性价比计算**: 智能计算套餐的性价比和适用性
- **个性化推荐**: 基于会员等级、消费习惯和偏好的个性化推荐
- **节省分析**: 计算购买套餐相比零散消费的潜在节省

## 系统架构

```
AmusementParkRecommendationSystem/
├── Models/                     # 数据模型
│   └── Models.cs              # 会员、消费记录、套餐等模型
├── Services/                   # 业务服务
│   ├── DataService.cs         # 数据服务
│   └── AIRecommendationService.cs # AI推荐服务
├── Plugins/                    # Semantic Kernel插件
│   └── CoinPackageRecommendationPlugin.cs # 推荐插件
├── Program.cs                  # 主程序入口
└── appsettings.json           # 配置文件
```

## 快速开始

### 环境要求
- .NET 8.0 SDK
- OpenAI API密钥 或 Azure OpenAI服务

### 安装步骤

1. **安装依赖**
   ```bash
   dotnet restore
   ```

2. **配置API密钥**
   编辑 `appsettings.json` 文件，配置OpenAI或Azure OpenAI的API密钥：
   
   **OpenAI配置**:
   ```json
   {
     "OpenAI": {
       "ApiKey": "your-openai-api-key-here",
       "ModelId": "gpt-4",
       "Endpoint": "https://api.openai.com/v1"
     }
   }
   ```
   
   **Azure OpenAI配置**:
   ```json
   {
     "AzureOpenAI": {
       "ApiKey": "your-azure-openai-api-key-here",
       "Endpoint": "https://your-resource.openai.azure.com/",
       "DeploymentName": "gpt-4",
       "ApiVersion": "2024-02-01"
     }
   }
   ```

3. **构建项目**
   ```bash
   dotnet build
   ```

4. **运行应用**
   ```bash
   dotnet run
   ```

## 使用指南

### 主菜单选项
启动应用后，您可以选择以下功能：

1. **查看所有会员** - 显示所有会员的基本信息和统计数据
2. **查看会员详细信息** - 查看特定会员的详细消费历史和偏好
3. **AI智能推荐** - 使用AI分析并推荐最适合的币套餐
4. **查看所有币套餐** - 显示所有可用的币套餐信息
5. **退出系统**

### AI推荐功能
系统的核心功能，通过以下步骤工作：

1. **数据收集**: 分析会员的历史消费数据
2. **模式识别**: 识别消费频率、偏好游戏类型、平均消费金额等
3. **智能分析**: 使用AI模型分析最适合的套餐
4. **推荐生成**: 生成详细的推荐理由和替代选项
5. **结果展示**: 显示推荐套餐、置信度、节省金额等信息

### 示例数据
系统预置了10个示例会员和6个币套餐，包括：

**会员类型**:
- 普通会员：基础消费用户
- 白银会员：中等消费用户  
- 黄金会员：高消费VIP用户

**套餐类型**:
- 基础套餐：适合轻度用户
- 进阶套餐：适合中度用户
- 豪华套餐：适合重度用户
- 超值套餐：高性价比选择

## 开发指南

### VS Code任务
项目包含预配置的VS Code任务：

- `Ctrl+Shift+P` → `Tasks: Run Task` → `build` - 构建项目
- `Ctrl+Shift+P` → `Tasks: Run Task` → `run` - 运行项目
- `Ctrl+Shift+P` → `Tasks: Run Task` → `clean` - 清理项目
- `Ctrl+Shift+P` → `Tasks: Run Task` → `restore` - 还原依赖

### 扩展功能
要添加新功能，可以：

1. **添加新的数据模型**: 在 `Models/Models.cs` 中定义
2. **扩展业务逻辑**: 在 `Services/` 中添加新服务
3. **添加AI功能**: 在 `Plugins/` 中创建新的Semantic Kernel插件
4. **修改推荐算法**: 更新 `AIRecommendationService.cs` 中的逻辑
   ```

2. **配置API密钥**
   
   编辑 `appsettings.json` 文件，添加您的API密钥：

   **使用OpenAI:**
   ```json
   {
     "OpenAI": {
       "ApiKey": "your-actual-openai-api-key",
       "ModelId": "gpt-4",
       "Endpoint": "https://api.openai.com/v1"
     }
   }
   ```

   **使用Azure OpenAI:**
   ```json
   {
     "AzureOpenAI": {
       "ApiKey": "your-actual-azure-openai-api-key",
       "Endpoint": "https://your-resource.openai.azure.com/",
       "DeploymentName": "gpt-4",
       "ApiVersion": "2024-02-01"
     }
   }
   ```

3. **安装依赖包**
   ```bash
   dotnet restore
   ```

4. **运行项目**
   ```bash
   dotnet run
   ```

## 使用指南

### 主要功能

1. **查看所有会员**: 显示系统中所有会员的基本信息
2. **查看会员详情**: 查看特定会员的详细信息和消费历史
3. **智能推荐**: 为指定会员生成个性化的币套餐推荐
4. **套餐管理**: 查看所有可用的币套餐信息

### 示例数据

系统预置了以下示例数据：

**会员等级**:
- 普通会员
- 银卡会员
- 金卡会员
- VIP会员

**套餐类型**:
- 新手体验包 (50币 + 5赠送)
- 银卡专享包 (100币 + 15赠送)
- 金卡豪华包 (200币 + 40赠送)
- VIP至尊包 (500币 + 120赠送)
- 周末狂欢包 (150币 + 30赠送，限时)
- 月度畅玩包 (300币 + 60赠送)

**游戏类型**:
- 射击游戏
- 投篮游戏
- 娃娃机
- 电玩游戏
- 赛车游戏
- 音乐游戏

## 核心技术

### Microsoft Semantic Kernel
- **插件架构**: 使用插件系统扩展AI功能
- **函数调用**: 通过KernelFunction装饰器公开业务数据
- **提示工程**: 优化的提示词模板用于生成推荐

### AI推荐算法
```csharp
// 推荐评分计算示例
private double CalculateRecommendationScore(Member member, CoinPackage package)
{
    double score = 50; // 基础分数
    
    // 会员等级匹配 (+20分)
    if (package.TargetMembershipLevels.Contains(member.MembershipLevel))
        score += 20;
    
    // 性价比评分 (最高+15分)
    var pricePerCoin = (double)package.Price / totalCoins;
    if (pricePerCoin < 0.5) score += 15;
    
    // 折扣优势 (最高+15分)
    score += Math.Min(package.DiscountPercentage, 15);
    
    // 消费习惯匹配 (最高+10分)
    // ... 基于使用周期计算
    
    return Math.Min(score, 100);
}
```

## 配置说明

### appsettings.json 配置项

```json
{
  "OpenAI": {
    "ApiKey": "OpenAI API密钥",
    "ModelId": "使用的模型ID (推荐: gpt-4)",
    "Endpoint": "API端点URL"
  },
  "AzureOpenAI": {
    "ApiKey": "Azure OpenAI API密钥",
    "Endpoint": "Azure OpenAI端点URL",
    "DeploymentName": "部署名称",
    "ApiVersion": "API版本"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

## 扩展开发

### 添加新的推荐因素

1. **扩展会员模型**:
   ```csharp
   public class Member
   {
       // 现有属性...
       public List<string> Interests { get; set; } = new(); // 新增兴趣标签
   }
   ```

2. **更新推荐算法**:
   ```csharp
   [KernelFunction]
   [Description("分析会员兴趣匹配度")]
   public string AnalyzeInterestMatch(int memberId, int packageId)
   {
       // 实现兴趣匹配逻辑
   }
   ```

3. **增强AI提示词**:
   ```csharp
   var prompt = $"""
       考虑以下新因素进行推荐：
       - 会员兴趣: {member.Interests}
       - 季节性偏好
       - 社交游戏偏好
       """;
   ```

### 集成外部数据源

```csharp
public class ExternalDataService
{
    public async Task<WeatherInfo> GetWeatherAsync()
    {
        // 获取天气信息影响游戏推荐
    }
    
    public async Task<PromotionInfo> GetCurrentPromotionsAsync()
    {
        // 获取当前促销活动
    }
}
```

## 故障排除

### 常见问题

1. **API密钥错误**
   - 确保在appsettings.json中配置了正确的API密钥
   - 检查密钥是否有足够的权限和余额

2. **推荐结果不准确**
   - 检查示例数据是否符合实际业务场景
   - 调整推荐算法的权重参数

3. **性能问题**
   - 考虑缓存AI响应结果
   - 优化数据查询和处理逻辑

## 许可证

MIT License - 详见 LICENSE 文件

## 贡献指南

欢迎提交问题和拉取请求！请确保：
- 遵循现有的代码风格
- 添加适当的测试用例
- 更新相关文档

## 联系我们

如有问题或建议，请通过以下方式联系：
- 提交GitHub Issue
- 发送邮件至: [your-email@example.com]

---

**注意**: 这是一个演示项目，用于展示如何使用Semantic Kernel构建AI驱动的推荐系统。在生产环境中使用前，请确保进行充分的测试和安全评估。
