using AmusementParkRecommendationSystem.Filter;
using AmusementParkRecommendationSystem.Models;
using AmusementParkRecommendationSystem.Plugins;
using AmusementParkRecommendationSystem.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Diagnostics;
using Baodian.AI.SemanticKernel;

namespace AmusementParkRecommendationSystem;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== 智能游乐园币套餐推荐系统 ===");
        Console.WriteLine();

        try
        {        // 配置服务
            var serviceProvider = ConfigureServices();            // 获取服务
            var dataService = serviceProvider.GetRequiredService<DataService>();
            var aiService = serviceProvider.GetRequiredService<AIRecommendationService>(); var enhancedAiService = serviceProvider.GetRequiredService<EnhancedAIRecommendationService>();
            var agentManager = serviceProvider.GetRequiredService<AgentManager>();
            var tripRecommendationService = serviceProvider.GetRequiredService<TripRecommendationService>();
            var telemetryDemoService = serviceProvider.GetRequiredService<TelemetryDemoService>();

            // 显示集成的智能体列表
            await DisplayIntegratedAgentsAsync(agentManager);

            while (true)
            {
                Console.WriteLine("请选择操作:");
                Console.WriteLine("=== 基础功能 ===");
                Console.WriteLine("1. 查看所有会员");
                Console.WriteLine("2. 查看会员详细信息");
                Console.WriteLine("3. 为会员推荐币套餐");
                Console.WriteLine("4. 查看所有币套餐");
                Console.WriteLine();
                Console.WriteLine("=== AI智能分析 (Agent增强) ===");
                Console.WriteLine("5. 智能业务分析");
                Console.WriteLine("6. 定价优化分析");
                Console.WriteLine("7. 客户细分分析");
                Console.WriteLine("8. 运营建议生成");
                Console.WriteLine("9. 客户生命周期价值分析");
                Console.WriteLine("10. 个性化深度推荐");
                Console.WriteLine(); Console.WriteLine("=== 位置智能服务 ===");
                Console.WriteLine("11. 查看门店位置信息");
                Console.WriteLine("12. 客户位置邀请服务");
                Console.WriteLine("13. 智能行程规划");
                Console.WriteLine("14. 附近景点推荐");
                Console.WriteLine("15. 天气与实时建议");
                Console.WriteLine();
                Console.WriteLine("=== OpenTelemetry 监控演示 ===");
                Console.WriteLine("16. OpenTelemetry 追踪演示");
                Console.WriteLine("17. 查看当前追踪信息");
                Console.WriteLine();
                Console.WriteLine("18. NL2SQL：自然语言转MySQL语句");
                Console.WriteLine();
                Console.WriteLine("0. 退出系统");
                Console.Write("请输入选项 (0-18): ");

                var choice = Console.ReadLine();
                Console.WriteLine(); switch (choice)
                {
                    case "1":
                        ShowAllMembers(dataService);
                        break;
                    case "2":
                        ShowMemberDetails(dataService);
                        break;
                    case "3":
                        await RecommendCoinPackageAsync(aiService, dataService);
                        break;
                    case "4":
                        ShowAllCoinPackages(dataService);
                        break;
                    case "5":
                        await PerformIntelligentBusinessAnalysisAsync(enhancedAiService);
                        break;
                    case "6":
                        await PerformPricingOptimizationAsync(enhancedAiService);
                        break;
                    case "7":
                        await PerformCustomerSegmentationAsync(enhancedAiService);
                        break;
                    case "8":
                        await GenerateOperationalRecommendationsAsync(enhancedAiService);
                        break;
                    case "9":
                        await AnalyzeCustomerLifetimeValueAsync(enhancedAiService, dataService);
                        break;
                    case "10":
                        await GeneratePersonalizedRecommendationAsync(enhancedAiService, dataService);
                        break;
                    case "11":
                        await ShowStoreLocationsAsync(dataService);
                        break;
                    case "12":
                        await InviteNearbyCustomersAsync(tripRecommendationService, dataService);
                        break;
                    case "13":
                        await GenerateTripPlanAsync(tripRecommendationService, dataService);
                        break;
                    case "14":
                        await RecommendNearbyAttractionsAsync(tripRecommendationService);
                        break;
                    case "15":
                        await ShowWeatherAndRealtimeAdviceAsync(tripRecommendationService);
                        break;
                    case "16":
                        await DemonstrateOpenTelemetryAsync(telemetryDemoService);
                        break;
                    case "17":
                        ShowCurrentTraceInfo(telemetryDemoService);
                        break;

                    case "18":
                        await NL2SQLAsync(serviceProvider);
                        break;
                    case "0":
                        Console.WriteLine("感谢使用智能推荐系统，再见！");
                        return;

                    default:
                        Console.WriteLine("无效选项，请重新选择。");
                        break;
                }
                Console.WriteLine();
                Console.WriteLine("按 Enter 键继续...");
                Console.ReadLine();
                Console.Clear();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"系统出现错误: {ex.Message}");
            Console.WriteLine("请检查配置文件中的API密钥设置。");
        }
    }

    /// <summary>
    /// 配置依赖注入服务
    /// </summary>
    private static ServiceProvider ConfigureServices()
    {
        // 加载配置
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        var services = new ServiceCollection();

        // 注册配置
        services.AddSingleton<IConfiguration>(configuration);

        // 添加OpenTelemetry服务
        services.AddOpenTelemetryServices(configuration);
        services.AddSemanticKernelTracing();
        services.AddAliyunQwen235BChatService(configuration);        // 添加AI HttpClient工厂

        // 添加日志服务
        services.AddLogging(builder =>
        {
            builder.AddConsole();
        });

        // 添加数据服务
        services.AddSingleton<DataService>();        // 注册插件
        services.AddSingleton<CoinPackageRecommendationPlugin>();

        // 注册OpenTelemetry相关过滤器
        services.AddSingleton<OpenTelemetryFunctionFilter>();
        services.AddSingleton<OpenTelemetryPromptFilter>();

        // 添加百炼服务
        services.AddSingleton<BailianChatService>();        // 配置Kernel插件 - 修复循环依赖问题
        services.AddSingleton<Kernel>(serviceProvider =>
        {
            var kernelBuilder = Kernel.CreateBuilder();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            // 添加日志过滤器
            kernelBuilder.Services.AddSingleton<IFunctionInvocationFilter>(_ => new LoggingFilter(logger));

            // 添加OpenTelemetry过滤器
            kernelBuilder.Services.AddSingleton<IFunctionInvocationFilter>(serviceProvider.GetRequiredService<OpenTelemetryFunctionFilter>());
            kernelBuilder.Services.AddSingleton<IPromptRenderFilter>(serviceProvider.GetRequiredService<OpenTelemetryPromptFilter>());

            // 添加SafePromptFilter
            kernelBuilder.Services.AddSingleton<IPromptRenderFilter, SafePromptFilter>();

            // 优先尝试使用百炼服务
            var bailianService = serviceProvider.GetRequiredService<BailianChatService>();
            var bailianApiKey = configuration["Bailian:ApiKey"];
            if (!string.IsNullOrEmpty(bailianApiKey) && bailianApiKey != "your-bailian-api-key-here")
            {
                // 添加百炼服务
                bailianService.AddBailianToKernel(kernelBuilder);
                Console.WriteLine("[成功] 使用 阿里云百炼 服务");
            }
            // 尝试使用Deepseek服务
            else if (TryAddDeepseekService(kernelBuilder, configuration))
            {
                Console.WriteLine("[成功] 使用 Deepseek 服务");
            }
            // 尝试使用Azure OpenAI服务
            else if (TryAddAzureOpenAIService(kernelBuilder, configuration))
            {
                Console.WriteLine("[成功] 使用 Azure OpenAI 服务");
            }
            else
            {
                // 没有配置有效的API密钥，使用模拟服务
                Console.WriteLine("[警告] 未配置有效的API密钥，将使用备用推荐算法");
                Console.WriteLine("   要使用AI功能，请在appsettings.json中配置百炼、Deepseek或Azure OpenAI密钥");
            }

            var kernel = kernelBuilder.Build();
            var dataService = serviceProvider.GetRequiredService<DataService>();
            var plugin = new CoinPackageRecommendationPlugin(dataService);
            kernel.Plugins.AddFromObject(plugin, "CoinPackageRecommendation");
            return kernel;
        });

        // 注册 IChatCompletionService - 从 Kernel 中获取
        services.AddSingleton<IChatCompletionService>(serviceProvider =>
        {
            var kernel = serviceProvider.GetRequiredService<Kernel>();
            return kernel.GetRequiredService<IChatCompletionService>();
        });        // 添加AI推荐服务
        services.AddSingleton<AIRecommendationService>();        // 添加智能体管理器
        services.AddSingleton<AgentManager>();

        // 配置全局HttpClient设置 - 防止AI服务调用超时
        services.ConfigureHttpClientDefaults(builder =>
        {
            builder.ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromMinutes(5); // 设置为5分钟超时
                client.DefaultRequestHeaders.Add("User-Agent", "AmusementParkRecommendationSystem/1.0");
            });
        });

        // 创建专用的AI服务HttpClient工厂
        services.AddSingleton<Func<HttpClient>>(serviceProvider =>
        {
            return () => new HttpClient()
            {
                Timeout = TimeSpan.FromMinutes(5)
            };
        });        // 添加增强AI推荐服务 (Agent-based)
        services.AddSingleton<EnhancedAIRecommendationService>();

        // 添加内存缓存服务 - MapApiService 需要
        services.AddMemoryCache();        // 添加位置服务相关组件
        services.AddSingleton<MapApiService>();
        services.AddSingleton<POIService>();
        services.AddSingleton<WeatherService>();
        services.AddSingleton<NotificationService>();
        services.AddSingleton<LocationService>();
        services.AddSingleton<TripRecommendationService>();

        // 添加OpenTelemetry演示服务
        services.AddSingleton<TelemetryDemoService>();

        // This filter outputs information about auto function invocation and returns overridden result



        return services.BuildServiceProvider();
    }    /// <summary>
         /// 尝试添加Deepseek服务
         /// </summary>
    private static bool TryAddDeepseekService(IKernelBuilder kernelBuilder, IConfiguration configuration)
    {
        var deepseekApiKey = configuration["Deepseek:ApiKey"];

        if (!string.IsNullOrEmpty(deepseekApiKey) && deepseekApiKey != "your-deepseek-api-key-here")
        {
            // 使用统一的HttpClient工厂
            var httpClient = AIHttpClientFactory.CreateConfiguredClient();

            kernelBuilder.AddOpenAIChatCompletion(
                modelId: configuration["Deepseek:ModelId"] ?? "deepseek-chat",
                apiKey: deepseekApiKey,
                endpoint: new Uri(configuration["Deepseek:Endpoint"] ?? "https://api.deepseek.com/v1"),
                httpClient: httpClient);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 尝试添加Azure OpenAI服务
    /// </summary>
    private static bool TryAddAzureOpenAIService(IKernelBuilder kernelBuilder, IConfiguration configuration)
    {
        var azureOpenAiApiKey = configuration["AzureOpenAI:ApiKey"];

        if (!string.IsNullOrEmpty(azureOpenAiApiKey) && azureOpenAiApiKey != "your-azure-openai-api-key-here")
        {
            // 使用统一的HttpClient工厂
            var httpClient = AIHttpClientFactory.CreateConfiguredClient();

            kernelBuilder.AddAzureOpenAIChatCompletion(
                deploymentName: configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4",
                endpoint: configuration["AzureOpenAI:Endpoint"] ?? "",
                apiKey: azureOpenAiApiKey,
                apiVersion: configuration["AzureOpenAI:ApiVersion"],
                httpClient: httpClient);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 显示所有会员
    /// </summary>
    private static void ShowAllMembers(DataService dataService)
    {
        var members = dataService.GetAllMembers();

        Console.WriteLine("=== 所有会员列表 ===");
        Console.WriteLine();
        Console.WriteLine($"{"ID",-4} {"姓名",-10} {"会员等级",-8} {"年龄",-4} {"总消费",-10} {"访问次数",-8} {"最后访问",-12}");
        Console.WriteLine(new string('-', 65));

        foreach (var member in members)
        {
            Console.WriteLine($"{member.Id,-4} {member.Name,-10} {member.MembershipLevel,-8} {member.Age,-4} {member.TotalSpent,-10:F2} {member.VisitCount,-8} {member.LastVisit,-12:yyyy-MM-dd}");
        }
    }

    /// <summary>
    /// 显示会员详细信息
    /// </summary>
    private static void ShowMemberDetails(DataService dataService)
    {
        Console.Write("请输入会员ID: ");
        if (int.TryParse(Console.ReadLine(), out int memberId))
        {
            var member = dataService.GetMember(memberId);
            if (member != null)
            {
                Console.WriteLine();
                Console.WriteLine("=== 会员详细信息 ===");
                Console.WriteLine($"ID: {member.Id}");
                Console.WriteLine($"姓名: {member.Name}");
                Console.WriteLine($"会员等级: {member.MembershipLevel}");
                Console.WriteLine($"年龄: {member.Age} 岁");
                Console.WriteLine($"性别: {member.Gender}");
                Console.WriteLine($"注册日期: {member.RegistrationDate:yyyy-MM-dd}");
                Console.WriteLine($"总消费金额: ¥{member.TotalSpent:F2}");
                Console.WriteLine($"访问次数: {member.VisitCount} 次");
                Console.WriteLine($"最后访问: {member.LastVisit:yyyy-MM-dd}");
                Console.WriteLine($"偏好活动: {string.Join(", ", member.PreferredActivities)}");

                // 显示最近消费记录
                var recentRecords = member.ConsumptionHistory
                    .OrderByDescending(r => r.Date)
                    .Take(5)
                    .ToList();

                if (recentRecords.Any())
                {
                    Console.WriteLine();
                    Console.WriteLine("最近5次消费记录:");
                    Console.WriteLine($"{"日期",-12} {"游戏类型",-10} {"游戏币",-8} {"金额",-8} {"获胜",-6}");
                    Console.WriteLine(new string('-', 50));

                    foreach (var record in recentRecords)
                    {
                        Console.WriteLine($"{record.Date:yyyy-MM-dd} {record.ActivityType,-10} {record.CoinsUsed,-8} ¥{record.AmountSpent,-6:F2} {(record.Won ? "是" : "否"),-6}");
                    }
                }
            }
            else
            {
                Console.WriteLine("会员不存在！");
            }
        }
        else
        {
            Console.WriteLine("无效的会员ID！");
        }
    }

    /// <summary>
    /// 为会员推荐币套餐
    /// </summary>
    private static async Task RecommendCoinPackageAsync(AIRecommendationService aiService, DataService dataService)
    {
        Console.Write("请输入会员ID: ");
        if (int.TryParse(Console.ReadLine(), out int memberId))
        {
            var member = dataService.GetMember(memberId);
            if (member != null)
            {
                Console.WriteLine();
                Console.WriteLine($"正在为会员 {member.Name}（{member.MembershipLevel}）生成智能推荐...");
                Console.WriteLine();

                try
                {
                    var recommendation = await aiService.RecommendCoinPackageAsync(memberId);

                    Console.WriteLine("=== AI智能推荐结果 ===");
                    Console.WriteLine();
                    Console.WriteLine($"推荐套餐: {recommendation.RecommendedPackage.Name}");
                    Console.WriteLine($"套餐价格: ¥{recommendation.RecommendedPackage.Price}");
                    Console.WriteLine($"游戏币数量: {recommendation.RecommendedPackage.CoinCount} + {recommendation.RecommendedPackage.BonusCoins} (赠送)");
                    Console.WriteLine($"总游戏币: {recommendation.RecommendedPackage.CoinCount + recommendation.RecommendedPackage.BonusCoins}");
                    Console.WriteLine($"推荐置信度: {recommendation.ConfidenceScore:F1}%");
                    Console.WriteLine($"潜在节省: ¥{recommendation.PotentialSavings:F2}");
                    Console.WriteLine();

                    Console.WriteLine("推荐理由:");
                    Console.WriteLine(recommendation.Reason);
                    Console.WriteLine();

                    Console.WriteLine("购买好处:");
                    foreach (var benefit in recommendation.Benefits)
                    {
                        Console.WriteLine($"• {benefit}");
                    }

                    if (recommendation.AlternativePackages.Any())
                    {
                        Console.WriteLine();
                        Console.WriteLine("备选套餐:");
                        foreach (var alternative in recommendation.AlternativePackages)
                        {
                            Console.WriteLine($"• {alternative.Name} - ¥{alternative.Price} ({alternative.CoinCount + alternative.BonusCoins} 游戏币)");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"推荐生成失败: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("会员不存在！");
            }
        }
        else
        {
            Console.WriteLine("无效的会员ID！");
        }
    }

    /// <summary>
    /// 显示所有币套餐
    /// </summary>
    private static void ShowAllCoinPackages(DataService dataService)
    {
        var packages = dataService.GetAvailableCoinPackages();

        Console.WriteLine("=== 所有币套餐 ===");
        Console.WriteLine();

        foreach (var package in packages)
        {
            Console.WriteLine($"套餐名称: {package.Name}");
            Console.WriteLine($"价格: ¥{package.Price}");
            Console.WriteLine($"游戏币: {package.CoinCount} + {package.BonusCoins} (赠送) = {package.CoinCount + package.BonusCoins} 总计");
            Console.WriteLine($"折扣: {package.DiscountPercentage}%");
            Console.WriteLine($"描述: {package.Description}");
            Console.WriteLine($"套餐类型: {package.PackageType}");

            if (package.TargetMembershipLevels.Any())
            {
                Console.WriteLine($"适用会员: {string.Join(", ", package.TargetMembershipLevels)}");
            }
            else
            {
                Console.WriteLine("适用会员: 所有会员");
            }

            if (package.IsLimitedTime)
            {
                Console.WriteLine($"限时优惠至: {package.ValidUntil:yyyy-MM-dd}");
            }
            Console.WriteLine($"性价比: {(double)(package.CoinCount + package.BonusCoins) / (double)package.Price:F2} 币/元");
            Console.WriteLine(new string('-', 50));
        }
    }

    #region 增强AI功能方法

    /// <summary>
    /// 执行智能业务分析
    /// </summary>
    private static async Task PerformIntelligentBusinessAnalysisAsync(EnhancedAIRecommendationService enhancedService)
    {
        Console.WriteLine("=== 智能业务分析 ===");
        Console.WriteLine();
        Console.WriteLine("请选择分析类型:");
        Console.WriteLine("1. 收入分析 (revenue)");
        Console.WriteLine("2. 客户分析 (customer)");
        Console.WriteLine("3. 运营分析 (operational)");
        Console.WriteLine("4. 全面分析 (comprehensive)");
        Console.Write("请选择 (1-4): ");

        var choice = Console.ReadLine();
        var analysisType = choice switch
        {
            "1" => "revenue",
            "2" => "customer",
            "3" => "operational",
            "4" => "comprehensive",
            _ => "comprehensive"
        };

        Console.WriteLine();
        Console.WriteLine("正在进行智能业务分析，请稍候...");
        Console.WriteLine();

        try
        {
            var result = await enhancedService.PerformIntelligentBusinessAnalysisAsync(analysisType);

            Console.WriteLine($"=== {result.AnalysisType.ToUpper()} 分析结果 ===");
            Console.WriteLine($"生成时间: {result.GeneratedAt:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine();

            Console.WriteLine("[洞察] 关键洞察:");
            foreach (var insight in result.Insights)
            {
                Console.WriteLine($"• {insight}");
            }
            Console.WriteLine();

        }
        catch (Exception ex)
        {
            Console.WriteLine($"分析失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 执行定价优化分析
    /// </summary>
    private static async Task PerformPricingOptimizationAsync(EnhancedAIRecommendationService enhancedService)
    {
        Console.WriteLine("=== 定价优化分析 ===");
        Console.WriteLine();
        Console.WriteLine("正在分析当前定价策略并生成优化建议...");
        Console.WriteLine();

        try
        {
            var results = await enhancedService.OptimizePricingAsync();

            Console.WriteLine("[定价] 定价优化建议:");
            Console.WriteLine();

            foreach (var result in results)
            {
                Console.WriteLine($"套餐ID: {result.PackageId}");
                Console.WriteLine($"当前价格: ¥{result.CurrentPrice}");
                Console.WriteLine($"建议价格: ¥{result.RecommendedPrice}");
                Console.WriteLine($"价格变化: {result.PriceChangePercentage:F1}%");
                Console.WriteLine($"预期收入影响: {result.ExpectedRevenueImpact:F1}%");
                Console.WriteLine($"建议理由: {result.Reasoning}");
                Console.WriteLine(new string('-', 40));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"定价分析失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 执行客户细分分析
    /// </summary>
    private static async Task PerformCustomerSegmentationAsync(EnhancedAIRecommendationService enhancedService)
    {
        Console.WriteLine("=== 客户细分分析 ===");
        Console.WriteLine();
        Console.WriteLine("正在分析客户群体特征...");
        Console.WriteLine();

        try
        {
            var results = await enhancedService.PerformCustomerSegmentationAsync();
            Console.WriteLine("[客户群体] 客户群体分析结果:");
            Console.WriteLine();

            foreach (var segment in results)
            {
                Console.WriteLine($"[群体] 客户群体: {segment.SegmentName}");
                Console.WriteLine($"客户数量: {segment.MemberCount} 人");
                Console.WriteLine($"平均消费: ¥{segment.AverageSpending}");
                Console.WriteLine();

                Console.WriteLine("群体特征:");
                foreach (var characteristic in segment.Characteristics)
                {
                    Console.WriteLine($"  • {characteristic}");
                }
                Console.WriteLine(); Console.WriteLine("推荐行动:");
                foreach (var action in segment.RecommendedActions)
                {
                    Console.WriteLine($"  [行动] {action}");
                }
                Console.WriteLine(new string('-', 50));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"客户细分分析失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 生成运营建议
    /// </summary>
    private static async Task GenerateOperationalRecommendationsAsync(EnhancedAIRecommendationService enhancedService)
    {
        Console.WriteLine("=== 运营建议生成 ===");
        Console.WriteLine();
        Console.WriteLine("正在基于业务数据生成运营优化建议...");
        Console.WriteLine();

        try
        {
            var recommendations = await enhancedService.GenerateOperationalRecommendationsAsync();

            Console.WriteLine("[优化] 运营优化建议:");
            Console.WriteLine();

            foreach (var rec in recommendations)
            {
                Console.WriteLine($"[类别] 类别: {rec.Category}");
                Console.WriteLine($"优先级: {rec.Priority}");
                Console.WriteLine($"标题: {rec.Title}");
                Console.WriteLine($"描述: {rec.Description}");
                Console.WriteLine($"预期影响: {rec.ExpectedImpact}");
                Console.WriteLine(); Console.WriteLine("实施步骤:");
                foreach (var step in rec.ImplementationSteps)
                {
                    Console.WriteLine($"  [步骤] {step}");
                }
                Console.WriteLine(new string('-', 50));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"运营建议生成失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 分析客户生命周期价值
    /// </summary>
    private static async Task AnalyzeCustomerLifetimeValueAsync(EnhancedAIRecommendationService enhancedService, DataService dataService)
    {
        Console.WriteLine("=== 客户生命周期价值分析 ===");
        Console.WriteLine(); Console.Write("请输入会员ID: ");
        if (int.TryParse(Console.ReadLine(), out int memberId))
        {
            var member = dataService.GetMember(memberId);
            if (member != null)
            {
                Console.WriteLine();
                Console.WriteLine($"正在分析会员 {member.Name} 的生命周期价值...");
                Console.WriteLine();

                try
                {
                    var result = await enhancedService.AnalyzeCustomerLifetimeValueAsync(memberId);

                    Console.WriteLine("[分析] 客户价值分析结果:");
                    Console.WriteLine($"会员ID: {result.MemberId}");
                    Console.WriteLine($"当前价值: ¥{result.CurrentValue}");
                    Console.WriteLine($"预测生命周期价值: ¥{result.PredictedLifetimeValue}");
                    Console.WriteLine($"流失风险评分: {result.RiskScore:F2} (0-1, 越低越好)");
                    Console.WriteLine($"保留概率: {result.RetentionProbability:F2} (0-1, 越高越好)");
                    Console.WriteLine();

                    Console.WriteLine("[行动] 推荐行动:");
                    foreach (var action in result.RecommendedActions)
                    {
                        Console.WriteLine($"• {action}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"分析失败: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("会员不存在！");
            }
        }
        else
        {
            Console.WriteLine("无效的会员ID！");
        }
    }

    /// <summary>
    /// 生成个性化深度推荐
    /// </summary>
    private static async Task GeneratePersonalizedRecommendationAsync(EnhancedAIRecommendationService enhancedService, DataService dataService)
    {
        Console.WriteLine("=== 个性化深度推荐 ===");
        Console.WriteLine(); Console.Write("请输入会员ID: ");
        if (int.TryParse(Console.ReadLine(), out int memberId))
        {
            var member = dataService.GetMember(memberId);
            if (member != null)
            {
                Console.WriteLine();
                Console.WriteLine("请选择推荐目标:");
                Console.WriteLine("1. 套餐推荐 (默认)");
                Console.WriteLine("2. 消费建议");
                Console.WriteLine("3. 活动推荐");
                Console.WriteLine("4. 综合建议");
                Console.Write("请选择 (1-4, 默认1): ");

                var choice = Console.ReadLine();
                var goal = choice switch
                {
                    "2" => "生成消费习惯优化建议",
                    "3" => "推荐适合的游乐园活动",
                    "4" => "提供综合的会员服务建议",
                    _ => ""
                };

                Console.WriteLine();
                Console.WriteLine($"正在为会员 {member.Name} 生成个性化推荐...");
                Console.WriteLine();

                try
                {
                    var recommendation = await enhancedService.GeneratePersonalizedRecommendationAsync(memberId, goal);

                    Console.WriteLine("[推荐] 个性化推荐结果:");
                    Console.WriteLine();
                    Console.WriteLine(recommendation);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"推荐生成失败: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("会员不存在！");
            }
        }
        else
        {
            Console.WriteLine("无效的会员ID！");
        }
    }

    /// <summary>
    /// 显示集成的智能体列表
    /// </summary>
    private static async Task DisplayIntegratedAgentsAsync(AgentManager agentManager)
    {
        Console.WriteLine("[系统] 智能体系统初始化完成");
        Console.WriteLine("=".PadRight(50, '='));
        Console.WriteLine();

        // 获取智能体统计信息
        var stats = agentManager.GetAgentStatistics();
        var availableAgents = stats["AvailableAgents"] as List<string> ?? new List<string>();

        Console.WriteLine($"[状态] 系统状态: {stats["Status"]}");
        Console.WriteLine($"[配置] 智能体总数: {stats["TotalAgents"]}");
        Console.WriteLine($"[时间] 初始化时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine();

        Console.WriteLine("[智能体] 集成的专业智能体:");
        Console.WriteLine("-".PadRight(50, '-'));        // 定义智能体的中文名称和描述
        var agentDescriptions = new Dictionary<string, (string ChineseName, string Description)>
        {
            ["BusinessAnalyst"] = ("业务分析师", "深度分析业务数据，识别关键趋势和机会"),
            ["PricingStrategist"] = ("定价策略专家", "优化定价结构，最大化收入和客户满意度"),
            ["CustomerAnalyst"] = ("客户分析师", "客户细分与行为分析，个性化营销策略"),
            ["OperationsExpert"] = ("运营优化专家", "提升运营效率，优化客户体验"),
            ["PersonalizationExpert"] = ("个性化服务专家", "生成高度个性化的产品和服务推荐"),
            ["DataScientist"] = ("数据科学家", "复杂数据分析、统计建模和预测"),
            ["CustomerSuccessManager"] = ("客户成功经理", "提升客户满意度，预防流失，优化生命周期价值"),
            ["LocationIntelligenceAgent"] = ("位置智能专家", "基于地理位置分析客户行为和推荐策略"),
            ["TripPlannerAgent"] = ("行程规划专家", "根据客户位置和偏好定制最佳游览路线"),
            ["DiningRecommendationAgent"] = ("餐饮推荐专家", "根据客户特征和位置提供个性化餐饮建议"),
            ["WeatherAndRealtimeAgent"] = ("天气与实时信息专家", "分析天气状况和实时数据为游客提供即时游玩建议")
        }; foreach (var agentName in availableAgents)
        {
            if (agentDescriptions.TryGetValue(agentName, out var info))
            {
                Console.WriteLine($"[就绪] {info.ChineseName} ({agentName})");
                Console.WriteLine($"   └─ {info.Description}");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine($"[就绪] {agentName}");
                Console.WriteLine($"   └─ 专业智能体服务");
                Console.WriteLine();
            }
        }

        Console.WriteLine("[启动] 所有智能体已就绪，可开始智能分析服务！");
        Console.WriteLine("=".PadRight(50, '='));
        Console.WriteLine();

        // 短暂延迟以便用户阅读        await Task.Delay(1500);
    }

    #endregion

    #region 位置智能服务方法

    /// <summary>
    /// 显示门店位置信息
    /// </summary>
    private static async Task ShowStoreLocationsAsync(DataService dataService)
    {
        Console.WriteLine("=== 门店位置信息 ===");
        var stores = dataService.GetAllStoreLocations();

        if (!stores.Any())
        {
            Console.WriteLine("暂无门店信息。");
            return;
        }

        foreach (var store in stores)
        {
            Console.WriteLine($"[{store.Id}] {store.Name}");
            Console.WriteLine($"   地址: {store.Address}");
            Console.WriteLine($"   坐标: {store.Latitude:F6}, {store.Longitude:F6}");
            Console.WriteLine($"   营业时间: {store.BusinessHours}");
            Console.WriteLine($"   联系电话: {store.ContactPhone}");
            Console.WriteLine($"   评分: {store.Rating:F1}/5.0"); Console.WriteLine($"   特色项目: {string.Join(", ", store.Features)}");
            Console.WriteLine($"   描述: {store.Description}");
            Console.WriteLine();
        }

        // 添加模拟异步操作以消除编译警告
        await Task.Delay(10);
    }

    /// <summary>
    /// 客户位置邀请服务
    /// </summary>
    private static async Task InviteNearbyCustomersAsync(TripRecommendationService tripService, DataService dataService)
    {
        Console.WriteLine("=== 客户位置邀请服务 ===");

        // 显示可用门店
        var stores = dataService.GetAllStoreLocations();
        Console.WriteLine("可用门店:");
        foreach (var store in stores)
        {
            Console.WriteLine($"[{store.Id}] {store.Name} - {store.Address}");
        }

        Console.Write("请输入门店ID: ");
        if (!int.TryParse(Console.ReadLine(), out int storeId))
        {
            Console.WriteLine("无效的门店ID。");
            return;
        }

        Console.Write("请输入邀请半径(米，默认2000): ");
        var radiusInput = Console.ReadLine();
        double radius = 2000;
        if (!string.IsNullOrEmpty(radiusInput) && double.TryParse(radiusInput, out var customRadius))
        {
            radius = customRadius;
        }

        try
        {
            Console.WriteLine($"正在检查门店 {storeId} 附近 {radius}米范围内的客户...");
            var invitations = await tripService.CheckAndInviteNearbyCustomersAsync(storeId, radius);

            if (!invitations.Any())
            {
                Console.WriteLine("附近暂无符合条件的客户。");
                return;
            }

            Console.WriteLine($"找到 {invitations.Count} 位附近客户，已发送邀请:");
            foreach (var invitation in invitations)
            {
                Console.WriteLine($"客户: {invitation.CustomerName} (ID: {invitation.CustomerId})");
                Console.WriteLine($"距离: {invitation.Distance:F0}米");
                Console.WriteLine($"预计响应率: {invitation.EstimatedResponseRate:P1}");
                Console.WriteLine($"邀请内容: {invitation.Message}");
                Console.WriteLine($"发送方式: {string.Join(", ", invitation.DeliveryMethods)}");
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"邀请服务出现错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 智能行程规划
    /// </summary>
    private static async Task GenerateTripPlanAsync(TripRecommendationService tripService, DataService dataService)
    {
        Console.WriteLine("=== 智能行程规划 ===");

        // 显示客户偏好
        var customerPreferences = dataService.GetAllCustomerLocationPreferences();
        Console.WriteLine("可用客户:");
        foreach (var customer in customerPreferences.Take(10))
        {
            Console.WriteLine($"[{customer.CustomerId}] {customer.Name} (年龄: {customer.Age})");
        }

        Console.Write("请输入客户ID: ");
        if (!int.TryParse(Console.ReadLine(), out int customerId))
        {
            Console.WriteLine("无效的客户ID。");
            return;
        }

        Console.Write("请输入目标门店ID (可选): ");
        var storeInput = Console.ReadLine();
        int? targetStoreId = null;
        if (!string.IsNullOrEmpty(storeInput) && int.TryParse(storeInput, out var storeId))
        {
            targetStoreId = storeId;
        }

        try
        {
            Console.WriteLine("正在生成个性化行程计划...");
            var tripPlan = await tripService.GeneratePersonalizedTripPlanAsync(customerId, targetStoreId);

            if (tripPlan == null)
            {
                Console.WriteLine("无法为该客户生成行程计划。");
                return;
            }

            Console.WriteLine($"=== {tripPlan.CustomerName} 的专属行程计划 ===");
            Console.WriteLine($"目标门店: {tripPlan.TargetStoreName}");
            Console.WriteLine($"建议游览时间: {tripPlan.SuggestedStartTime:yyyy-MM-dd HH:mm}");
            Console.WriteLine($"预计总时长: {tripPlan.EstimatedDuration}小时");
            Console.WriteLine($"预算建议: ¥{tripPlan.EstimatedBudget}");
            Console.WriteLine(); Console.WriteLine("=== 交通方案 ===");
            if (tripPlan.Transportation.Any())
            {
                var mainTransport = tripPlan.Transportation.First();
                Console.WriteLine($"推荐交通方式: {mainTransport.TransportType}");
                Console.WriteLine($"路线: {mainTransport.Route}");
                Console.WriteLine($"预计用时: {mainTransport.EstimatedDuration}分钟");
                Console.WriteLine($"预计费用: ¥{mainTransport.EstimatedCost}");
            }
            Console.WriteLine();

            if (tripPlan.Attractions.Any())
            {
                Console.WriteLine("=== 推荐景点 ===");
                foreach (var attraction in tripPlan.Attractions)
                {
                    Console.WriteLine($"• {attraction.Name}");
                    Console.WriteLine($"  类别: {attraction.Category}");
                    Console.WriteLine($"  营业时间: {attraction.OpeningHours}");
                    Console.WriteLine($"  推荐理由: {attraction.RecommendationReason}");
                    Console.WriteLine();
                }
            }

            if (tripPlan.DiningOptions.Any())
            {
                Console.WriteLine("=== 美食推荐 ===");
                foreach (var dining in tripPlan.DiningOptions)
                {
                    Console.WriteLine($"• {dining.Name}");
                    Console.WriteLine($"  类型: {dining.CuisineType}");
                    Console.WriteLine($"  价位: {dining.PriceRange}");
                    Console.WriteLine($"  推荐理由: {dining.RecommendationReason}");
                    Console.WriteLine();
                }
            }

            Console.WriteLine("=== 行程建议 ===");
            foreach (var suggestion in tripPlan.Suggestions)
            {
                Console.WriteLine($"• {suggestion}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"行程规划出现错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 附近景点推荐
    /// </summary>
    private static async Task RecommendNearbyAttractionsAsync(TripRecommendationService tripService)
    {
        Console.WriteLine("=== 附近景点推荐 ===");

        Console.Write("请输入当前位置纬度: ");
        if (!double.TryParse(Console.ReadLine(), out double latitude))
        {
            Console.WriteLine("无效的纬度值。");
            return;
        }

        Console.Write("请输入当前位置经度: ");
        if (!double.TryParse(Console.ReadLine(), out double longitude))
        {
            Console.WriteLine("无效的经度值。");
            return;
        }

        Console.Write("请输入搜索半径(米，默认5000): ");
        var radiusInput = Console.ReadLine();
        double radius = 5000;
        if (!string.IsNullOrEmpty(radiusInput) && double.TryParse(radiusInput, out var customRadius))
        {
            radius = customRadius;
        }

        try
        {
            Console.WriteLine($"正在搜索位置 ({latitude:F6}, {longitude:F6}) 附近 {radius}米范围内的景点...");
            var attractions = await tripService.GetNearbyAttractionsAsync(latitude, longitude, radius);

            if (!attractions.Any())
            {
                Console.WriteLine("附近暂无推荐景点。");
                return;
            }

            Console.WriteLine($"找到 {attractions.Count} 个推荐景点:");
            foreach (var attraction in attractions)
            {
                Console.WriteLine($"• {attraction.Name}");
                Console.WriteLine($"  类别: {attraction.Category}");
                Console.WriteLine($"  地址: {attraction.Address}");
                Console.WriteLine($"  距离: {attraction.DistanceFromStore:F1}公里");
                Console.WriteLine($"  营业时间: {attraction.OpeningHours}");
                if (!string.IsNullOrEmpty(attraction.Phone))
                {
                    Console.WriteLine($"  联系电话: {attraction.Phone}");
                }
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"景点推荐出现错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 天气与实时建议
    /// </summary>
    private static async Task ShowWeatherAndRealtimeAdviceAsync(TripRecommendationService tripService)
    {
        Console.WriteLine("=== 天气与实时建议 ===");

        Console.Write("请输入位置纬度 (默认北京: 39.9042): ");
        var latInput = Console.ReadLine();
        double latitude = 39.9042;
        if (!string.IsNullOrEmpty(latInput) && double.TryParse(latInput, out var customLat))
        {
            latitude = customLat;
        }

        Console.Write("请输入位置经度 (默认北京: 116.4074): ");
        var lonInput = Console.ReadLine();
        double longitude = 116.4074;
        if (!string.IsNullOrEmpty(lonInput) && double.TryParse(lonInput, out var customLon))
        {
            longitude = customLon;
        }

        try
        {
            Console.WriteLine($"正在获取位置 ({latitude:F4}, {longitude:F4}) 的天气信息和建议...");
            var advice = await tripService.GetWeatherBasedAdviceAsync(latitude, longitude);

            Console.WriteLine("=== 当前天气状况 ===");
            Console.WriteLine($"天气: {advice.CurrentWeather}");
            Console.WriteLine($"温度: {advice.Temperature}°C");
            Console.WriteLine($"湿度: {advice.Humidity}%");
            Console.WriteLine($"风速: {advice.WindSpeed} km/h");
            Console.WriteLine();

            Console.WriteLine("=== 出行建议 ===");
            foreach (var suggestion in advice.Suggestions)
            {
                Console.WriteLine($"• {suggestion}");
            }
            Console.WriteLine();

            if (advice.RecommendedActivities.Any())
            {
                Console.WriteLine("=== 推荐活动 ===");
                foreach (var activity in advice.RecommendedActivities)
                {
                    Console.WriteLine($"• {activity}");
                }
                Console.WriteLine();
            }

            if (advice.WarningMessages.Any())
            {
                Console.WriteLine("=== 注意事项 ===");
                foreach (var warning in advice.WarningMessages)
                {
                    Console.WriteLine($"⚠️ {warning}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"天气服务出现错误: {ex.Message}");
        }
    }

    #endregion

    #region OpenTelemetry 演示方法

    /// <summary>
    /// 演示OpenTelemetry功能
    /// </summary>
    private static async Task DemonstrateOpenTelemetryAsync(TelemetryDemoService telemetryDemoService)
    {
        Console.WriteLine("=== OpenTelemetry 追踪演示 ===");
        Console.WriteLine();
        Console.WriteLine("请选择演示类型:");
        Console.WriteLine("1. 基本追踪演示");
        Console.WriteLine("2. 嵌套追踪演示");
        Console.WriteLine("3. 错误处理演示");
        Console.WriteLine("4. 自定义指标演示");
        Console.WriteLine("5. 所有演示");
        Console.Write("请选择 (1-5): ");

        var choice = Console.ReadLine();
        Console.WriteLine();

        try
        {
            switch (choice)
            {
                case "1":
                    await DemoBasicTracing(telemetryDemoService);
                    break;
                case "2":
                    await DemoNestedTracing(telemetryDemoService);
                    break;
                case "3":
                    await DemoErrorHandling(telemetryDemoService);
                    break;
                case "4":
                    DemoCustomMetrics(telemetryDemoService);
                    break;
                case "5":
                    await DemoAllTracingFeatures(telemetryDemoService);
                    break;
                default:
                    Console.WriteLine("无效选项，使用基本追踪演示。");
                    await DemoBasicTracing(telemetryDemoService);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"演示过程中出现错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 基本追踪演示
    /// </summary>
    private static async Task DemoBasicTracing(TelemetryDemoService telemetryDemoService)
    {
        Console.WriteLine("[演示] 基本追踪功能");
        Console.WriteLine();

        var result = await telemetryDemoService.DemonstrateTracingAsync("基本追踪演示");
        Console.WriteLine($"演示结果: {result}");
        Console.WriteLine();
        Console.WriteLine("✓ 基本追踪演示完成！");
        Console.WriteLine("  - 生成了追踪span");
        Console.WriteLine("  - 记录了操作标签");
        Console.WriteLine("  - 测量了操作耗时");
    }

    /// <summary>
    /// 嵌套追踪演示
    /// </summary>
    private static async Task DemoNestedTracing(TelemetryDemoService telemetryDemoService)
    {
        Console.WriteLine("[演示] 嵌套追踪功能");
        Console.WriteLine();

        var result1 = await telemetryDemoService.DemonstrateTracingAsync("父级操作");
        Console.WriteLine($"父级操作结果: {result1}");
        Console.WriteLine();

        var result2 = await telemetryDemoService.DemonstrateTracingAsync("子级操作");
        Console.WriteLine($"子级操作结果: {result2}");
        Console.WriteLine();

        Console.WriteLine("✓ 嵌套追踪演示完成！");
        Console.WriteLine("  - 创建了父子关系的span");
        Console.WriteLine("  - 记录了嵌套操作的层次结构");
    }

    /// <summary>
    /// 错误处理演示
    /// </summary>
    private static async Task DemoErrorHandling(TelemetryDemoService telemetryDemoService)
    {
        Console.WriteLine("[演示] 错误处理追踪");
        Console.WriteLine();

        try
        {
            // 模拟一个会抛出错误的操作
            await Task.Run(() =>
            {
                throw new InvalidOperationException("这是一个演示错误");
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"捕获到演示错误: {ex.Message}");

            // 使用遥测服务记录错误信息
            telemetryDemoService.RecordCustomMetrics("error_count", 1, new Dictionary<string, object>
            {
                ["error_type"] = ex.GetType().Name,
                ["operation"] = "演示错误处理"
            });
        }

        Console.WriteLine();
        Console.WriteLine("✓ 错误处理演示完成！");
        Console.WriteLine("  - 记录了错误状态");
        Console.WriteLine("  - 生成了错误指标");
        Console.WriteLine("  - 保持了追踪上下文");
    }

    /// <summary>
    /// 自定义指标演示
    /// </summary>
    private static void DemoCustomMetrics(TelemetryDemoService telemetryDemoService)
    {
        Console.WriteLine("[演示] 自定义指标功能");
        Console.WriteLine();

        // 记录一些自定义指标
        telemetryDemoService.RecordCustomMetrics("user_action_count", 1, new Dictionary<string, object>
        {
            ["action_type"] = "demo_execution",
            ["user_type"] = "administrator"
        });

        telemetryDemoService.RecordCustomMetrics("system_performance", 0.85, new Dictionary<string, object>
        {
            ["metric_type"] = "cpu_usage",
            ["system"] = "recommendation_engine"
        });

        telemetryDemoService.RecordCustomMetrics("business_kpi", 127.5, new Dictionary<string, object>
        {
            ["kpi_type"] = "revenue_per_user",
            ["period"] = "daily"
        });

        Console.WriteLine("✓ 自定义指标演示完成！");
        Console.WriteLine("  - 记录了用户行为指标");
        Console.WriteLine("  - 记录了系统性能指标");
        Console.WriteLine("  - 记录了业务KPI指标");
    }

    /// <summary>
    /// 所有追踪功能演示
    /// </summary>
    private static async Task DemoAllTracingFeatures(TelemetryDemoService telemetryDemoService)
    {
        Console.WriteLine("[演示] 完整的OpenTelemetry功能展示");
        Console.WriteLine();

        Console.WriteLine("1. 基本追踪...");
        await DemoBasicTracing(telemetryDemoService);
        Console.WriteLine();

        Console.WriteLine("2. 嵌套追踪...");
        await DemoNestedTracing(telemetryDemoService);
        Console.WriteLine();

        Console.WriteLine("3. 错误处理...");
        await DemoErrorHandling(telemetryDemoService);
        Console.WriteLine();

        Console.WriteLine("4. 自定义指标...");
        DemoCustomMetrics(telemetryDemoService);
        Console.WriteLine();

        Console.WriteLine("🎉 所有OpenTelemetry功能演示完成！");
        Console.WriteLine("现在可以在您的监控系统中查看生成的追踪数据和指标。");
    }

    /// <summary>
    /// 显示当前追踪信息
    /// </summary>
    private static void ShowCurrentTraceInfo(TelemetryDemoService telemetryDemoService)
    {
        Console.WriteLine("=== 当前追踪信息 ===");
        Console.WriteLine();

        var traceInfo = telemetryDemoService.GetCurrentTraceInfo();
        Console.WriteLine($"追踪状态: {traceInfo}");
        Console.WriteLine();

        Console.WriteLine("OpenTelemetry 配置信息:");
        Console.WriteLine("- 追踪导出器: OTLP");
        Console.WriteLine("- 导出端点: http://tracing-analysis-dc-sz.aliyuncs.com:8090");
        Console.WriteLine("- 指标导出器: Prometheus");
        Console.WriteLine("- 采样率: 100%");
        Console.WriteLine();

        Console.WriteLine("支持的追踪功能:");
        Console.WriteLine("✓ HTTP客户端调用追踪");
        Console.WriteLine("✓ Semantic Kernel函数调用追踪");
        Console.WriteLine("✓ 自定义业务操作追踪");
        Console.WriteLine("✓ 错误和异常追踪");
        Console.WriteLine("✓ 性能指标收集");
        Console.WriteLine("✓ 分布式追踪上下文传播");
    }

    #endregion

    /// <summary>
    /// 18. NL2SQL：自然语言转MySQL语句
    /// </summary>
    private static async Task NL2SQLAsync(IServiceProvider serviceProvider)
    {
        var qwen = serviceProvider.GetRequiredService<Baodian.AI.SemanticKernel.Services.AliyunQwen235BChatService>();
        Console.WriteLine("请输入要转换为SQL的自然语言问题（如：查询所有会员的姓名和余额）：");
        var input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("输入不能为空。");
            return;
        }
        // 构造 prompt
        var systemPrompt = "你是一个SQL专家，请将用户的自然语言问题转换为MySQL可执行的SQL语句，只返回SQL，不要解释。";
        var messages = new Baodian.AI.SemanticKernel.Services.AliyunQwen235BChatService.QwenMessage[]
        {
            new() { Role = "system", Content = systemPrompt },
            new() { Role = "user", Content = input }
        };
        var sql = await qwen.ChatAsync(messages);
        Console.WriteLine("\n【生成的SQL语句】：");
        Console.WriteLine(sql);
    }
}
