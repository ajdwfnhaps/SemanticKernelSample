using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Baodian.Core.Logger;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Baodian.BLog;

namespace Baodian.AI.SemanticKernel.Samples
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
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
                
                // 3. 配置自定义日志 
                services.AddBaodianSLogger();
                
                // 4. 配置 Baodian Semantic Kernel
                services.AddBaodianSemanticKernel(configuration);
                
                // 5. 注册订单相关服务
                services.AddSingleton<OrderPlugin>();
                services.AddSingleton<OrderAgentSample>();
                
                // 6. 构建服务提供者
                var serviceProvider = services.BuildServiceProvider();
                
                // 7. 获取OrderAgentSample实例并运行
                var orderAgentSample = serviceProvider.GetRequiredService<OrderAgentSample>();
                await orderAgentSample.RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"程序运行出错：{ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
} 