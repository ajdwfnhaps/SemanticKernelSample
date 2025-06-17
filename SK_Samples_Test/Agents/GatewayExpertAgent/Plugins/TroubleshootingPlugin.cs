using Microsoft.SemanticKernel;
using SK_Samples_Test.Agents.GatewayExpertAgent.Services;
using System.ComponentModel;

namespace SK_Samples_Test.Agents.GatewayExpertAgent.Plugins
{
    /// <summary>
    /// 故障排查插件 - 处理闸机故障诊断和解决方案
    /// </summary>
    public class TroubleshootingPlugin
    {
        private readonly SolutionGeneratorService _solutionGeneratorService;
        private readonly KnowledgeBaseService _knowledgeBaseService;

        public TroubleshootingPlugin(
            SolutionGeneratorService solutionGeneratorService,
            KnowledgeBaseService knowledgeBaseService)
        {
            _solutionGeneratorService = solutionGeneratorService;
            _knowledgeBaseService = knowledgeBaseService;
        }

        [KernelFunction("diagnose_gateway_issue")]
        [Description("诊断闸机故障并提供初步解决建议")]
        public async Task<string> DiagnoseGatewayIssueAsync(
            [Description("故障现象描述，如无法通行、读卡失败等")] string issueDescription)
        {
            var troubleshootingGuide = _solutionGeneratorService.GenerateTroubleshootingGuide(issueDescription);
            var relatedKnowledge = await _knowledgeBaseService.GetKnowledgeEntryAsync("troubleshooting_common_issues");

            return $@"
=== 故障诊断结果 ===

问题描述：{issueDescription}

{troubleshootingGuide}

{relatedKnowledge}

建议处理流程：
1. 确认故障现象和频率
2. 检查基础设施（电源、网络、硬件连接）
3. 查看系统日志和错误信息
4. 按照排查步骤逐项检查
5. 如问题持续，联系技术支持
";
        }

        [KernelFunction("get_emergency_solution")]
        [Description("获取紧急故障的快速解决方案")]
        public async Task<string> GetEmergencySolutionAsync(
            [Description("紧急故障描述")] string emergencyDescription)
        {
            await Task.Delay(50); // 模拟快速响应

            var quickSolutions = emergencyDescription.ToLower() switch
            {
                var desc when desc.Contains("无法通行") || desc.Contains("卡住") =>
                    @"紧急处理步骤：
1. 立即检查闸机门页是否被异物卡住
2. 按下紧急开关手动开启通道
3. 检查电源连接是否正常
4. 重启闸机设备
5. 如仍无法解决，切换到手动验证模式",

                var desc when desc.Contains("读卡失败") || desc.Contains("刷卡无反应") =>
                    @"紧急处理步骤：
1. 检查读卡器指示灯状态
2. 清洁读卡器表面
3. 测试其他卡片是否正常
4. 检查读卡器连接线
5. 重启读卡器模块",

                var desc when desc.Contains("人脸识别") || desc.Contains("摄像头") =>
                    @"紧急处理步骤：
1. 检查摄像头镜头是否清洁
2. 确认光线条件是否充足
3. 重启人脸识别模块
4. 检查摄像头连接线
5. 临时切换到卡片验证模式",

                var desc when desc.Contains("网络") || desc.Contains("连接") =>
                    @"紧急处理步骤：
1. 检查网线连接是否松动
2. 重启网络设备（路由器/交换机）
3. 切换到离线模式运行
4. 检查网络配置参数
5. 联系网络管理员",

                _ => @"通用紧急处理步骤：
1. 记录故障现象和时间
2. 重启闸机设备
3. 检查所有连接线
4. 切换到手动验证模式
5. 立即联系技术支持"
            };

            return $@"
=== 紧急故障处理 ===

故障：{emergencyDescription}

{quickSolutions}

⚠️ 重要提醒：
- 如果涉及人员安全，立即启用手动通行
- 保持现场秩序，避免人员聚集
- 详细记录故障现象以便后续分析
- 紧急情况下请拨打技术支持热线：400-XXX-XXXX
";
        }

        [KernelFunction("analyze_error_logs")]
        [Description("分析闸机系统错误日志")]
        public async Task<string> AnalyzeErrorLogsAsync(
            [Description("错误日志内容或错误代码")] string errorLogs)
        {
            await Task.Delay(100);

            var analysis = "";
            if (errorLogs.Contains("E001") || errorLogs.Contains("读卡器"))
            {
                analysis = "读卡器通信异常，建议检查读卡器连接和驱动程序";
            }
            else if (errorLogs.Contains("E002") || errorLogs.Contains("网络"))
            {
                analysis = "网络连接问题，检查网络配置和服务器连接";
            }
            else if (errorLogs.Contains("E003") || errorLogs.Contains("数据库"))
            {
                analysis = "数据库连接异常，检查数据库服务和连接字符串";
            }
            else if (errorLogs.Contains("E004") || errorLogs.Contains("权限"))
            {
                analysis = "权限验证失败，检查用户权限配置";
            }
            else
            {
                analysis = "请提供具体的错误代码或日志内容以进行详细分析";
            }

            return $@"
=== 错误日志分析 ===

日志内容：{errorLogs}

分析结果：{analysis}

建议操作：
1. 备份当前日志文件
2. 根据错误类型执行对应的修复步骤
3. 监控系统运行状态
4. 如问题重复出现，升级到技术支持团队

常见错误代码说明：
- E001: 读卡器通信异常
- E002: 网络连接失败
- E003: 数据库连接异常
- E004: 权限验证失败
- E005: 硬件设备故障
";
        }

        [KernelFunction("get_maintenance_checklist")]
        [Description("获取闸机日常维护检查清单")]
        public async Task<string> GetMaintenanceChecklistAsync()
        {
            await Task.Delay(50);

            return @"
=== 闸机日常维护检查清单 ===

每日检查项目：
□ 闸机外观是否正常，无明显损坏
□ 通道是否畅通，无异物阻挡
□ 指示灯是否正常工作
□ 读卡器表面是否清洁
□ 人脸识别摄像头镜头是否清洁

每周检查项目：
□ 清洁闸机表面和内部灰尘
□ 检查各部件连接是否牢固
□ 测试所有功能模块运行状态
□ 查看系统日志，排除异常记录
□ 备份重要配置和数据

每月检查项目：
□ 深度清洁设备内部
□ 检查机械部件润滑情况
□ 更新系统软件和驱动程序
□ 测试紧急开关和安全功能
□ 校准传感器和识别设备

季度检查项目：
□ 全面功能测试和性能评估
□ 检查电气安全和接地状况
□ 更新用户权限和数据库
□ 制定设备升级和更换计划
□ 培训操作人员和维护技能

维护记录：
建议建立维护日志，记录每次检查和维护的详细情况，以便追踪设备状态和预防性维护。
";
        }
    }
}