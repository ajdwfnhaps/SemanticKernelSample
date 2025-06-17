using Microsoft.Extensions.Logging;

namespace SK_Samples_Test.Agents.GatewayExpertAgent.Services
{
    /// <summary>
    /// 知识库服务
    /// </summary>
    public class KnowledgeBaseService
    {
        private readonly ILogger<KnowledgeBaseService> _logger;
        private readonly Dictionary<string, string> _knowledgeBase;

        public KnowledgeBaseService(ILogger<KnowledgeBaseService> logger)
        {
            _logger = logger;
            _knowledgeBase = InitializeKnowledgeBase();
        }

        /// <summary>
        /// 获取知识库条目
        /// </summary>
        public async Task<string> GetKnowledgeEntryAsync(string key)
        {
            await Task.Delay(50); // 模拟异步操作
            
            _logger.LogInformation("查询知识库条目: {Key}", key);

            if (_knowledgeBase.TryGetValue(key, out var entry))
            {
                return entry;
            }

            return $"未找到关键字 '{key}' 对应的知识库条目。";
        }

        /// <summary>
        /// 搜索知识库
        /// </summary>
        public List<string> SearchKnowledge(string keyword)
        {
            var results = new List<string>();
            
            foreach (var kvp in _knowledgeBase)
            {
                if (kvp.Key.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    kvp.Value.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add($"{kvp.Key}: {kvp.Value.Substring(0, Math.Min(100, kvp.Value.Length))}...");
                }
            }

            return results;
        }

        /// <summary>
        /// 搜索知识库 - 异步版本
        /// </summary>
        public async Task<List<string>> SearchKnowledgeBaseAsync(string keyword)
        {
            await Task.Delay(50); // 模拟异步操作
            
            var results = new List<string>();
            
            // 模拟搜索逻辑
            var allEntries = _knowledgeBase.Keys.ToList();
            var matchingKeys = allEntries.Where(key => 
                key.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                _knowledgeBase[key].Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            foreach (var key in matchingKeys)
            {
                results.Add($"[{key}] {_knowledgeBase[key]}");
            }
            
            if (!results.Any())
            {
                results.Add($"未找到包含 '{keyword}' 的相关信息。请尝试使用其他关键词搜索。");
            }
            
            return results;
        }

        private Dictionary<string, string> InitializeKnowledgeBase()
        {
            return new Dictionary<string, string>
            {
                ["gateway_deployment_guide"] = @"
闸机部署指南：
1. 前期准备：确认部署位置、电源和网络条件
2. 设备安装：按照安装手册进行设备固定和连接
3. 系统配置：设置基本参数、访问控制和票务规则
4. 功能测试：验证各项功能正常工作
5. 培训交付：对操作人员进行培训并正式交付使用
",
                ["maintenance_best_practices"] = @"
维护最佳实践：
1. 日常维护：每日检查设备状态，清洁设备表面
2. 定期维护：每周检查网络连接，每月检查硬件状态
3. 数据备份：定期备份配置数据和日志文件
4. 性能监控：监控设备性能指标，及时发现问题
5. 应急处理：制定应急预案，确保快速响应故障
",
                ["security_configuration"] = @"
安全配置建议：
1. 访问控制：设置合适的权限级别和访问时间限制
2. 数据加密：启用数据传输和存储加密
3. 审计日志：记录所有访问和操作日志
4. 定期更新：及时更新系统软件和安全补丁
5. 监控报警：配置安全事件监控和报警机制
",
                ["troubleshooting_common_issues"] = @"
常见问题排查：
1. 设备无响应：检查电源、网络连接，重启设备
2. 读卡失败：清洁读卡器，检查卡片状态
3. 网络连接问题：检查网络配置，测试连通性
4. 系统运行缓慢：检查系统资源，优化配置
5. 功能异常：查看日志文件，检查配置参数
"
            };
        }

        /// <summary>
        /// 添加知识库条目
        /// </summary>
        public void AddKnowledgeEntry(string key, string content)
        {
            _knowledgeBase[key] = content;
            _logger.LogInformation("添加知识库条目: {Key}", key);
        }
    }
}