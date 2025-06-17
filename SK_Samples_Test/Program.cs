using Baodian.AI.SemanticKernel;
using Baodian.AI.SemanticKernel.Abstractions;
using Baodian.AI.SemanticKernel.Constants;
using Baodian.AI.SemanticKernel.Providers;
using Baodian.AI.SemanticKernel.Samples;
using Baodian.BLog;
using Baodian.Core.Logger;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using SK_Samples_Test.Agents.GatewayExpertAgent;
using SK_Samples_Test.Agents.GatewayExpertAgent.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SK_Samples_Test
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("======= 智能代理示例程序 =======");
            Console.WriteLine("正在初始化系统...");

            try
            {
                // 1. 构建配置
                IConfiguration configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .AddCommandLine(args)
                    .Build();

                // 2. 创建服务集合
                var services = new ServiceCollection();

                // 注册配置对象到DI容器
                services.AddSingleton<IConfiguration>(configuration);

                // 3. 配置自定义日志 
                services.AddHttpContextAccessor();
                services.AddHttpClient();
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                })
                .AddBaodianBLogger();

                // 4. 配置 Baodian Semantic Kernel
                services.AddBaodianSemanticKernel(configuration);

                // 5. 构建服务提供者
                var serviceProvider = services.BuildServiceProvider();
                
                // 6. 获取KernelFactory并创建DeepSeek Kernel
                var kernelFactory = serviceProvider.GetRequiredService<IKernelFactory>();
                kernelFactory.RegisterProvider(ModelConstants.AliQwenMax, serviceProvider.GetRequiredService<DeepSeekProvider>());

                // 7. 注册订单相关服务
                services.AddSingleton<OrderPlugin>();
                services.AddSingleton<OrderAgentSample>();
                
                // 8. 注册闸机专家代理相关服务
                services.AddSingleton<ConfigAnalysisService>();
                services.AddSingleton<SolutionGeneratorService>();
                services.AddSingleton<KnowledgeBaseService>();
                services.AddSingleton<SK_Samples_Test.Agents.GatewayExpertAgent.GatewayExpertAgent>();
                
                services.AddScoped<Kernel>(p =>
                {
                    return kernelFactory.CreateKernel(ModelConstants.AliQwenMax);
                });

                // 9. 重新构建服务提供者
                serviceProvider = services.BuildServiceProvider();
                Console.WriteLine("系统初始化完成！\n");

                // 主循环
                while (true)
                {
                    try
                    {
                        Console.WriteLine("======= 智能代理示例程序 =======");
                        Console.WriteLine("请选择运行模式:");
                        Console.WriteLine("0. 退出程序");
                        Console.WriteLine("1. 完整的OrderAgent示例 (自主执行)");
                        Console.WriteLine("2. 交互式OrderAgent示例 (与用户对话)");
                        Console.WriteLine("3. 宝点云闸机专家代理 (智能咨询)");
                        Console.Write("请输入选择 (0-3): ");

                        var choice = Console.ReadLine();
                        switch (choice)
                        {
                            case "0":
                                Console.WriteLine("程序已退出。");
                                return;

                            case "1":
                                Console.WriteLine("\n开始执行自主OrderAgent示例...");
                                var orderAgentSample = serviceProvider.GetRequiredService<OrderAgentSample>();
                                await orderAgentSample.RunAsync();
                                break;

                            case "2":
                                Console.WriteLine("\n开始交互式OrderAgent示例 (输入 'exit' 退出对话)...");
                                await RunInteractiveOrderAgent(serviceProvider);
                                break;

                            case "3":
                                Console.WriteLine("\n开始宝点云闸机专家代理 (输入 'exit' 退出对话)...");
                                await RunGatewayExpertAgent(serviceProvider);
                                break;

                            default:
                                Console.WriteLine("无效选择，请重新输入。");
                                continue;
                        }

                        Console.WriteLine("\n======= 任务执行完毕 =======");
                        Console.WriteLine("按 'q' 退出程序，按其他任意键返回主菜单...");
                        var key = Console.ReadKey().KeyChar;
                        Console.WriteLine();

                        if (key == 'q' || key == 'Q')
                        {
                            Console.WriteLine("程序已退出。");
                            break;
                        }

                        Console.Clear();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\n执行过程中出错：{ex.Message}");
                        Console.WriteLine("按任意键继续...");
                        Console.ReadKey();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"程序运行出错：{ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("按任意键退出...");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// 运行交互式OrderAgent示例
        /// </summary>
        private static async Task RunInteractiveOrderAgent(ServiceProvider serviceProvider)
        {
            try
            {
                var kernel = serviceProvider.GetRequiredService<Kernel>();
                var orderPlugin = serviceProvider.GetRequiredService<OrderPlugin>();

                if (!kernel.Plugins.Contains("OrderPlugin"))
                {
                    kernel.ImportPluginFromObject(orderPlugin, "OrderPlugin");
                    Console.WriteLine("OrderPlugin 插件已导入");
                }
                else
                {
                    Console.WriteLine("OrderPlugin 插件已存在，跳过导入");
                }

                var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();

                var chatHistory = new ChatHistory();
                chatHistory.AddSystemMessage(@"你是一个专业的订单处理助手。你必须使用可用的函数来帮助用户处理订单相关任务：
1. 当用户询问商品详情时，调用 GetProductDetailsAsync 函数
2. 当用户要添加商品到购物车时，调用 AddToCartAsync 函数  
3. 当用户要创建订单时，调用 CreateOrderAsync 函数
4. 当用户要处理支付时，调用 ProcessPaymentAsync 函数

重要：你必须调用相应的函数来完成用户的请求，不要只是提供建议。
还有当支付完成后要总结整个订单流程给客户。");

                Console.WriteLine("订单处理助手已启动！我可以帮您处理订单相关的任务。");
                Console.WriteLine("您可以说：'查询商品123的详情'、'添加商品到购物车'、'创建订单'等");

                while (true)
                {
                    Console.Write("\n您: ");
                    var userInput = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(userInput) ||
                        userInput.ToLower() == "exit" ||
                        userInput.ToLower() == "退出")
                    {
                        break;
                    }

                    chatHistory.AddUserMessage(userInput);

                    Console.Write("助手: ");

                    var result = await chatCompletion.GetChatMessageContentAsync(
                        chatHistory,
                        executionSettings: new Microsoft.SemanticKernel.Connectors.OpenAI.OpenAIPromptExecutionSettings()
                        {
                            ToolCallBehavior = Microsoft.SemanticKernel.Connectors.OpenAI.ToolCallBehavior.AutoInvokeKernelFunctions
                        },
                        kernel: kernel);

                    Console.WriteLine(result.Content);

                    chatHistory.Add(result);
                }

                Console.WriteLine("交互式对话已结束。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"交互式Agent运行出错：{ex.Message}");
            }
        }

        /// <summary>
        /// 运行闸机专家代理
        /// </summary>
        private static async Task RunGatewayExpertAgent(ServiceProvider serviceProvider)
        {
            try
            {
                var gatewayExpert = serviceProvider.GetRequiredService<SK_Samples_Test.Agents.GatewayExpertAgent.GatewayExpertAgent>();
                await gatewayExpert.StartInteractiveSessionAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"闸机专家代理运行出错：{ex.Message}");
            }
        }

     
    }
}