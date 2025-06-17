using Microsoft.SemanticKernel;
using SK_Samples_Test.Agents.GatewayExpertAgent.Services;
using System.ComponentModel;

namespace SK_Samples_Test.Agents.GatewayExpertAgent.Plugins
{
    /// <summary>
    /// 闸机配置插件 - 处理闸机配置分析和建议
    /// </summary>
    public class GatewayConfigPlugin
    {
        private readonly ConfigAnalysisService _configAnalysisService;
        private readonly SolutionGeneratorService _solutionGeneratorService;

        public GatewayConfigPlugin(
            ConfigAnalysisService configAnalysisService,
            SolutionGeneratorService solutionGeneratorService)
        {
            _configAnalysisService = configAnalysisService;
            _solutionGeneratorService = solutionGeneratorService;
        }

        [KernelFunction("analyze_gateway_config")]
        [Description("分析闸机配置需求并提供专业建议")]
        public async Task<string> AnalyzeGatewayConfigAsync(
            [Description("用户的配置需求描述，包括场景、功能要求等")] string requirements)
        {
            var analysis = await _configAnalysisService.AnalyzeConfigAsync(requirements);
            return $@"
=== 闸机配置分析 ===

{analysis}

建议：
1. 根据您的需求，我已经为您分析了最适合的配置方案
2. 请确保硬件环境满足推荐配置要求
3. 如需更详细的配置指导，请提供具体的部署环境信息
";
        }

        [KernelFunction("generate_config_solution")]
        [Description("基于分析结果生成详细的配置解决方案")]
        public async Task<string> GenerateConfigSolutionAsync(
            [Description("配置需求分析结果")] string analysisResult)
        {
            var solution = await _solutionGeneratorService.GenerateSolutionAsync(analysisResult);
            return $@"
=== 配置解决方案 ===

{solution}

实施步骤：
1. 硬件准备和环境检查
2. 软件安装和初始配置
3. 功能模块配置和测试
4. 系统集成和上线部署
";
        }

        [KernelFunction("recommend_gateway_hardware")]
        [Description("根据使用场景推荐合适的闸机硬件配置")]
        public async Task<string> RecommendGatewayHardwareAsync(
            [Description("使用场景描述，如办公楼、学校、工厂等")] string scenario)
        {
            await Task.Delay(100);
            
            var recommendations = scenario.ToLower() switch
            {
                var s when s.Contains("办公楼") || s.Contains("写字楼") =>
                    @"推荐配置：
• 闸机类型：三辊闸或翼闸
• 读卡器：支持IC卡、身份证、手机NFC
• 人脸识别：高精度双目摄像头
• 网络：千兆以太网接口
• 电源：220V交流电，支持UPS备电",

                var s when s.Contains("学校") || s.Contains("校园") =>
                    @"推荐配置：
• 闸机类型：摆闸或翼闸（通行速度快）
• 读卡器：校园一卡通兼容
• 人脸识别：支持活体检测
• 访客管理：临时卡发放系统
• 数据接口：与校园管理系统对接",

                var s when s.Contains("工厂") || s.Contains("工业") =>
                    @"推荐配置：
• 闸机类型：三辊闸（安全性高）
• 读卡器：工业级RFID
• 人脸识别：防尘防水设计
• 材质：不锈钢防腐蚀
• 环境适应：宽温度范围，防护等级IP65",

                _ => @"通用推荐配置：
• 闸机类型：翼闸（通用性好）
• 读卡器：多格式兼容
• 人脸识别：标准配置
• 通讯接口：TCP/IP + RS485
• 管理软件：基础版本"
            };

            return $@"
=== 硬件配置推荐 ===

场景：{scenario}

{recommendations}

选型要点：
1. 根据日通行量选择合适的通行速度
2. 考虑环境因素（室内/室外、温湿度等）
3. 预留扩展接口以便后续功能升级
4. 选择有良好售后服务的知名品牌
";
        }

        [KernelFunction("configure_access_control")]
        [Description("配置闸机访问控制策略")]
        public async Task<string> ConfigureAccessControlAsync(
            [Description("访问控制需求，如时间段、人员类型、权限级别等")] string accessRequirements)
        {
            await Task.Delay(100);

            return $@"
=== 访问控制配置 ===

需求：{accessRequirements}

建议配置方案：

1. 时间控制策略：
   - 工作时间：周一至周五 08:00-18:00 正常通行
   - 休息时间：18:00-08:00 仅授权人员通行
   - 节假日：特殊时间段设置

2. 人员分类管理：
   - 正式员工：全时段通行权限
   - 临时人员：限定时间和区域
   - 访客：需陪同或预约通行
   - VIP用户：优先通行通道

3. 权限级别设置：
   - 管理员：系统配置和监控权限
   - 操作员：日常操作和查询权限
   - 普通用户：仅通行权限

4. 安全策略：
   - 反传递功能：防止尾随通行
   - 超时报警：长时间滞留提醒
   - 异常记录：失败尝试日志
   - 备用验证：多重身份确认

配置步骤：
1. 登录闸机管理系统
2. 进入权限管理模块
3. 创建用户组和权限模板
4. 分配具体权限和时间段
5. 测试配置效果并调整
";
        }

        [KernelFunction("setup_integration")]
        [Description("配置闸机与第三方系统的集成")]
        public async Task<string> SetupIntegrationAsync(
            [Description("需要集成的系统类型，如ERP、门禁、考勤等")] string systemType)
        {
            await Task.Delay(100);

            var integrationGuide = systemType.ToLower() switch
            {
                var s when s.Contains("erp") || s.Contains("人事") =>
                    @"ERP系统集成配置：
1. 数据同步设置：
   - 人员信息自动同步
   - 部门组织架构对接
   - 权限变更实时更新

2. 接口配置：
   - API接口地址配置
   - 数据格式和字段映射
   - 同步频率设置

3. 安全设置：
   - 数据传输加密
   - 接口访问权限控制
   - 日志审计记录",

                var s when s.Contains("考勤") || s.Contains("attendance") =>
                    @"考勤系统集成配置：
1. 考勤规则设置：
   - 上下班时间定义
   - 迟到早退判定规则
   - 加班和请假处理

2. 数据对接：
   - 通行记录自动转换
   - 考勤统计报表生成
   - 异常考勤提醒

3. 报表配置：
   - 考勤汇总报表
   - 个人考勤明细
   - 部门统计分析",

                var s when s.Contains("门禁") || s.Contains("access") =>
                    @"门禁系统集成配置：
1. 设备联动：
   - 多个门禁点统一管理
   - 区域权限设置
   - 联动报警功能

2. 权限同步：
   - 统一身份认证
   - 权限自动下发
   - 权限变更通知

3. 监控集成：
   - 视频监控联动
   - 报警信息推送
   - 事件记录查询",

                _ => @"通用集成配置：
1. 通讯协议设置
2. 数据格式定义
3. 接口测试验证
4. 异常处理机制
5. 维护和监控"
            };

            return $@"
=== 系统集成配置 ===

集成系统：{systemType}

{integrationGuide}

集成步骤：
1. 需求分析和接口设计
2. 测试环境搭建和调试
3. 数据格式验证和测试
4. 生产环境部署
5. 运行监控和维护

注意事项：
- 确保网络连通性和安全性
- 做好数据备份和恢复方案
- 建立异常处理和报警机制
- 定期检查集成状态和数据一致性
";
        }
    }
}