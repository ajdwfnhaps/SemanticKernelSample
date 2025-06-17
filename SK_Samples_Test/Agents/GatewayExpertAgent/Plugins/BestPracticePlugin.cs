using Microsoft.SemanticKernel;
using SK_Samples_Test.Agents.GatewayExpertAgent.Services;
using System.ComponentModel;

namespace SK_Samples_Test.Agents.GatewayExpertAgent.Plugins
{
    /// <summary>
    /// 最佳实践插件 - 提供闸机使用最佳实践和知识库服务
    /// </summary>
    public class BestPracticePlugin
    {
        private readonly KnowledgeBaseService _knowledgeBaseService;

        public BestPracticePlugin(KnowledgeBaseService knowledgeBaseService)
        {
            _knowledgeBaseService = knowledgeBaseService;
        }

        [KernelFunction("get_best_practices")]
        [Description("获取闸机部署和使用的最佳实践建议")]
        public async Task<string> GetBestPracticesAsync(
            [Description("实践领域，如部署、维护、安全、性能等")] string practiceArea)
        {
            var knowledgeEntry = await _knowledgeBaseService.GetKnowledgeEntryAsync("maintenance_best_practices");
            
            var practices = practiceArea.ToLower() switch
            {
                var area when area.Contains("部署") || area.Contains("安装") =>
                    @"部署最佳实践：

📍 选址和布局：
• 选择平整、坚固的安装地面
• 确保足够的通行空间（建议≥2米宽）
• 避免强光直射和强磁场干扰
• 预留维护和检修空间

🔌 电源和网络：
• 使用专用电源线路，避免与大功率设备共线
• 配置UPS不间断电源，确保稳定供电
• 网络采用有线连接，确保稳定性
• 预留备用网络接口

🔧 安装规范：
• 严格按照厂家安装手册操作
• 确保设备水平安装，误差<2mm
• 所有连接线路使用标准接头
• 做好防水防尘保护措施",

                var area when area.Contains("维护") || area.Contains("保养") =>
                    @"维护最佳实践：

🕐 定期维护计划：
• 每日：外观检查、功能测试
• 每周：清洁保养、连接检查
• 每月：深度清洁、软件更新
• 每季：全面检修、性能评估

🧹 清洁保养：
• 使用专用清洁剂，避免腐蚀性化学品
• 定期清洁读卡器和摄像头
• 保持机械部件润滑良好
• 及时清理异物和灰尘

📊 运行监控：
• 建立设备运行日志
• 监控通行数据和异常记录
• 定期备份配置和数据
• 跟踪设备性能指标",

                var area when area.Contains("安全") =>
                    @"安全最佳实践：

🔒 权限管理：
• 实施最小权限原则
• 定期审查和更新用户权限
• 使用强密码策略
• 启用多因素身份认证

🛡️ 数据安全：
• 加密敏感数据传输和存储
• 定期备份重要数据
• 建立数据访问审计日志
• 制定数据泄露应急预案

⚠️ 物理安全：
• 防止设备被恶意破坏
• 安装监控和报警系统
• 控制设备机房访问
• 定期检查安全设施",

                var area when area.Contains("性能") =>
                    @"性能优化最佳实践：

⚡ 系统优化：
• 定期清理系统日志和临时文件
• 优化数据库查询和索引
• 监控系统资源使用情况
• 升级硬件配置（必要时）

📈 通行效率：
• 合理设置识别敏感度
• 优化通行速度参数
• 配置高峰期处理策略
• 实施负载均衡（多设备）

🔧 故障预防：
• 建立设备健康监控
• 设置预警阈值和报警
• 实施预防性维护
• 准备备用设备和配件",

                _ => @"通用最佳实践：
• 遵循厂家操作手册
• 建立标准操作流程
• 定期培训操作人员
• 保持技术文档更新
• 与供应商保持良好合作关系"
            };

            return $@"
=== 闸机{practiceArea}最佳实践 ===

{practices}

{knowledgeEntry}

💡 关键提醒：
1. 始终以用户安全为第一优先级
2. 定期评估和改进现有流程
3. 保持与技术发展同步更新
4. 建立完善的文档记录体系
";
        }

        [KernelFunction("search_knowledge_base")]
        [Description("搜索闸机相关知识库信息")]
        public async Task<List<string>> SearchKnowledgeBaseAsync(
            [Description("搜索关键词")] string searchTerm)
        {
            return await _knowledgeBaseService.SearchKnowledgeBaseAsync(searchTerm);
        }

        [KernelFunction("get_knowledge_entry")]
        [Description("获取特定的知识库条目")]
        public async Task<string> GetKnowledgeEntryAsync(
            [Description("知识库条目的关键字")] string keyword)
        {
            return await _knowledgeBaseService.GetKnowledgeEntryAsync(keyword);
        }

        [KernelFunction("get_industry_standards")]
        [Description("获取闸机行业标准和规范要求")]
        public async Task<string> GetIndustryStandardsAsync(
            [Description("标准类型，如安全标准、技术规范、行业要求等")] string standardType)
        {
            await Task.Delay(100);

            var standards = standardType.ToLower() switch
            {
                var type when type.Contains("安全") =>
                    @"安全标准规范：

🏛️ 国家标准：
• GB 50348-2018《安全防范工程技术标准》
• GA/T 394-2018《出入口控制系统技术要求》
• GB/T 31488-2015《安全防范视频监控摄像设备》

🔒 安全要求：
• 设备应具备防雷、防静电保护
• 电气安全符合GB 4943.1标准
• 机械安全符合GB 4208防护等级要求
• 电磁兼容符合GB/T 17626标准

⚡ 供电要求：
• 工作电压：AC 220V±10%, 50Hz±2%
• 功耗：≤200W（单通道）
• 备用电源：≥4小时连续工作
• 接地电阻：≤4Ω",

                var type when type.Contains("技术") || type.Contains("性能") =>
                    @"技术规范标准：

📊 性能指标：
• 识别准确率：≥99.7%
• 通行速度：30-45人/分钟
• 响应时间：≤0.2秒
• 误识率：≤0.1%

🌡️ 环境适应：
• 工作温度：-25℃ ~ +70℃
• 存储温度：-40℃ ~ +85℃
• 相对湿度：≤95%（无凝露）
• 防护等级：IP65（室外型）

📡 通信接口：
• 网络接口：10/100M自适应以太网
• 串口：RS485、RS232
• 协议支持：TCP/IP、HTTP、SNMP
• 数据格式：XML、JSON",

                var type when type.Contains("行业") =>
                    @"行业规范要求：

🏢 应用场景标准：
• 办公建筑：JGJ 67-2019《办公建筑设计标准》
• 学校建筑：GB 50099-2011《中小学校设计规范》
• 工业建筑：GB 50187-2012《工业企业总平面设计规范》

🚪 通道设计：
• 通道宽度：≥550mm（单人通道）
• 通道高度：≥1800mm
• 残疾人通道：≥800mm宽度
• 紧急疏散：应设置紧急开启装置",

                _ => @"通用标准要求：
• 产品认证：3C认证、CE认证
• 质量管理：ISO 9001体系
• 环境管理：ISO 14001体系
• 信息安全：ISO 27001体系"
            };

            return $@"
=== 闸机{standardType}标准规范 ===

{standards}

📋 合规检查清单：
□ 设备是否通过相关认证
□ 安装是否符合标准要求
□ 性能指标是否达标
□ 安全防护措施是否到位
□ 维护保养是否规范

注意：具体项目应根据当地法规和项目需求进行调整。
";
        }

        [KernelFunction("get_training_materials")]
        [Description("获取闸机操作和维护培训材料")]
        public async Task<string> GetTrainingMaterialsAsync(
            [Description("培训对象，如操作员、管理员、维护人员等")] string trainingTarget)
        {
            await Task.Delay(100);

            var materials = trainingTarget.ToLower() switch
            {
                var target when target.Contains("操作员") || target.Contains("用户") =>
                    @"操作员培训内容：

📚 基础知识：
• 闸机基本工作原理
• 设备组成和功能介绍
• 安全操作规程
• 应急处理流程

🖥️ 系统操作：
• 管理软件界面介绍
• 用户权限管理
• 通行记录查询
• 报表生成和导出

🔧 日常维护：
• 设备外观检查
• 清洁保养方法
• 简单故障排除
• 维护记录填写

📞 应急处理：
• 常见问题快速处理
• 紧急开关使用方法
• 联系技术支持流程
• 安全事故处理",

                var target when target.Contains("管理员") || target.Contains("超级") =>
                    @"管理员培训内容：

🏗️ 系统管理：
• 完整系统架构理解
• 高级配置和参数设置
• 用户权限分级管理
• 系统集成和对接

📊 数据管理：
• 数据库管理和备份
• 统计分析和报表定制
• 数据导入导出
• 历史数据清理

🛡️ 安全管理：
• 系统安全策略制定
• 密码策略和权限审计
• 安全日志监控
• 应急预案制定

⚙️ 高级功能：
• 多设备集中管理
• 远程监控和诊断
• 系统性能优化
• 软件升级管理",

                var target when target.Contains("维护") || target.Contains("技术") =>
                    @"维护人员培训内容：

🔧 硬件维护：
• 设备拆装和组装
• 硬件故障诊断
• 电路原理和检修
• 机械部件维护

💻 软件维护：
• 系统安装和配置
• 驱动程序更新
• 软件故障排除
• 系统性能调优

🔍 故障诊断：
• 故障现象分析
• 测试工具使用
• 维修流程和方法
• 备件管理和更换

📚 技术支持：
• 客户问题分析
• 远程技术支持
• 现场服务规范
• 技术文档编写",

                _ => @"通用培训内容：
• 产品知识和技术原理
• 安全操作和防护措施
• 标准操作流程
• 问题处理和上报机制
• 持续学习和技能提升"
            };

            return $@"
=== {trainingTarget}培训材料 ===

{materials}

📖 培训方式建议：
1. 理论学习：产品手册、技术文档
2. 实践操作：现场演示、动手练习
3. 案例分析：典型问题和解决方案
4. 考核评估：理论考试、实操测试
5. 持续教育：定期更新、技能提升

📅 培训计划：
• 新员工入职培训：3-5天
• 定期技能提升：每季度1次
• 新功能培训：软件更新后
• 应急演练：每半年1次

💡 培训效果评估：
- 理论知识掌握程度
- 实际操作技能水平
- 问题解决能力
- 客户服务质量
";
        }
    }
}