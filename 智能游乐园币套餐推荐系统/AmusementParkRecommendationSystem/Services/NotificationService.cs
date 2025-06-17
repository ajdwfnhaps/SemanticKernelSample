using AmusementParkRecommendationSystem.Models;

namespace AmusementParkRecommendationSystem.Services;

/// <summary>
/// 通知服务 - 发送短信和邮件邀请
/// </summary>
public class NotificationService
{
    private readonly HttpClient _httpClient;

    public NotificationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// 发送短信邀请
    /// </summary>
    public async Task<bool> SendSMSInvitationAsync(InvitationMessage invitation, string phoneNumber)
    {
        try
        {
            // 构建短信内容
            var smsContent = FormatSMSContent(invitation);
            
            // 模拟短信发送 - 实际使用时替换为真实的短信API
            Console.WriteLine($"[短信发送] 发送至: {phoneNumber}");
            Console.WriteLine($"[短信内容] {smsContent}");
            
            // 模拟发送延迟
            await Task.Delay(100);
            
            // 记录发送日志
            LogNotification("SMS", invitation.CustomerName, phoneNumber, smsContent, true);
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"短信发送失败: {ex.Message}");
            LogNotification("SMS", invitation.CustomerName, phoneNumber, invitation.Message, false, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 发送邮件邀请
    /// </summary>
    public async Task<bool> SendEmailInvitationAsync(InvitationMessage invitation, string email)
    {
        try
        {
            // 构建邮件内容
            var (subject, body) = FormatEmailContent(invitation);
            
            // 模拟邮件发送 - 实际使用时替换为真实的邮件发送服务
            Console.WriteLine($"[邮件发送] 发送至: {email}");
            Console.WriteLine($"[邮件主题] {subject}");
            Console.WriteLine($"[邮件内容] {body}");
            
            // 模拟发送延迟
            await Task.Delay(200);
            
            // 记录发送日志
            LogNotification("EMAIL", invitation.CustomerName, email, $"{subject}\n{body}", true);
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"邮件发送失败: {ex.Message}");
            LogNotification("EMAIL", invitation.CustomerName, email, invitation.Message, false, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 批量发送邀请
    /// </summary>
    public async Task<(int SuccessCount, int FailCount)> SendBatchInvitationsAsync(
        List<InvitationMessage> invitations, 
        Dictionary<int, string> customerContacts,
        NotificationType notificationType = NotificationType.SMS)
    {
        int successCount = 0;
        int failCount = 0;

        foreach (var invitation in invitations)
        {
            if (!customerContacts.TryGetValue(invitation.CustomerId, out var contact))
            {
                failCount++;
                continue;
            }

            bool success = notificationType switch
            {
                NotificationType.SMS => await SendSMSInvitationAsync(invitation, contact),
                NotificationType.Email => await SendEmailInvitationAsync(invitation, contact),
                _ => false
            };

            if (success)
                successCount++;
            else
                failCount++;

            // 防止发送频率过高
            await Task.Delay(500);
        }

        return (successCount, failCount);
    }

    /// <summary>
    /// 发送行程计划
    /// </summary>
    public async Task<bool> SendTripPlanAsync(TripPlan tripPlan, string contact, NotificationType type)
    {
        try
        {
            if (type == NotificationType.SMS)
            {
                var smsContent = FormatTripPlanSMS(tripPlan);
                Console.WriteLine($"[行程短信] 发送至: {contact}");
                Console.WriteLine($"[内容] {smsContent}");
            }
            else if (type == NotificationType.Email)
            {
                var (subject, body) = FormatTripPlanEmail(tripPlan);
                Console.WriteLine($"[行程邮件] 发送至: {contact}");
                Console.WriteLine($"[主题] {subject}");
                Console.WriteLine($"[内容] {body}");
            }

            await Task.Delay(200);
            LogNotification(type.ToString(), tripPlan.CustomerName, contact, "行程计划", true);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"行程计划发送失败: {ex.Message}");
            LogNotification(type.ToString(), tripPlan.CustomerName, contact, "行程计划", false, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 格式化短信内容
    /// </summary>
    private string FormatSMSContent(InvitationMessage invitation)
    {
        var offers = invitation.RecommendedOffers?.Any() == true 
            ? $"特惠：{string.Join("、", invitation.RecommendedOffers.Take(2))}" 
            : "";

        var content = $"【{invitation.StoreName}】{invitation.CustomerName}您好！" +
                     $"您距离我们仅{Math.Round(invitation.DistanceToStore)}米，" +
                     $"现有精彩项目等您体验！{offers} " +
                     $"详情回复TD或点击链接查看。";

        // 短信长度限制
        if (content.Length > 70)
        {
            content = content.Substring(0, 67) + "...";
        }

        return content;
    }

    /// <summary>
    /// 格式化邮件内容
    /// </summary>
    private (string Subject, string Body) FormatEmailContent(InvitationMessage invitation)
    {
        var subject = $"【{invitation.StoreName}】专属邀请 - 距离您仅{Math.Round(invitation.DistanceToStore)}米！";

        var offers = invitation.RecommendedOffers?.Any() == true
            ? string.Join("\n• ", invitation.RecommendedOffers.Select(o => o))
            : "暂无特别优惠";

        var body = $@"亲爱的{invitation.CustomerName}，

我们发现您正在{invitation.StoreName}附近，这里有丰富多彩的娱乐项目等着您！

📍 位置信息：
• 门店名称：{invitation.StoreName}
• 距离您：{Math.Round(invitation.DistanceToStore)}米
• 预计步行时间：{Math.Round(invitation.DistanceToStore / 83.33)}分钟

🎁 专属优惠：
• {offers}

🎪 精彩项目：
{invitation.Message}

我们诚挚邀请您前来体验，享受美好的欢乐时光！

如需了解更多信息，请回复此邮件或致电门店。

祝您游玩愉快！
{invitation.StoreName}团队
发送时间：{invitation.SendTime:yyyy年MM月dd日 HH:mm}";

        return (subject, body);
    }

    /// <summary>
    /// 格式化行程计划短信
    /// </summary>
    private string FormatTripPlanSMS(TripPlan tripPlan)
    {
        var attractions = tripPlan.Attractions?.Take(3).Select(a => a.Name) ?? new List<string>();
        var attractionText = attractions.Any() ? string.Join("、", attractions) : "精彩项目";

        return $"【行程计划】{tripPlan.CustomerName}您好！您的{tripPlan.PlannedDate:MM月dd日}行程已准备好：" +
               $"{attractionText}等{tripPlan.Attractions?.Count ?? 0}个项目。" +
               $"预算约{tripPlan.TotalBudget}元。详细计划请查看邮件。";
    }

    /// <summary>
    /// 格式化行程计划邮件
    /// </summary>
    private (string Subject, string Body) FormatTripPlanEmail(TripPlan tripPlan)
    {
        var subject = $"您的专属行程计划 - {tripPlan.PlannedDate:MM月dd日}";

        var attractionsText = tripPlan.Attractions?.Any() == true
            ? string.Join("\n", tripPlan.Attractions.Select(a => $"• {a.Name} ({a.EstimatedVisitDuration}分钟) - {a.TicketPrice}元"))
            : "暂无景点安排";

        var diningText = tripPlan.DiningOptions?.Any() == true
            ? string.Join("\n", tripPlan.DiningOptions.Select(d => $"• {d.Name} ({d.CuisineType}) - {d.PriceRange}"))
            : "暂无餐饮安排";

        var transportText = tripPlan.Transportation?.Any() == true
            ? string.Join("\n", tripPlan.Transportation.Select(t => $"• {t.FromLocation} → {t.ToLocation}: {t.TransportMode}({t.EstimatedDuration}分钟)"))
            : "暂无交通安排";

        var body = $@"亲爱的{tripPlan.CustomerName}，

您的{tripPlan.PlannedDate:yyyy年MM月dd日}专属行程计划已经准备好了！

⏰ 行程概览：
• 开始时间：{tripPlan.StartTime:HH:mm}
• 结束时间：{tripPlan.EndTime:HH:mm}
• 预算范围：{tripPlan.TotalBudget}元
• 预计总费用：{tripPlan.EstimatedTotalCost}元

🎪 推荐景点：
{attractionsText}

🍽️ 餐饮推荐：
{diningText}

🚗 交通安排：
{transportText}

🌤️ 天气提醒：
{tripPlan.WeatherCondition}

📝 温馨提示：
{tripPlan.Notes}

💡 AI专属建议：
{tripPlan.AIRecommendations}

祝您游玩愉快！有任何问题请随时联系我们。

计划生成时间：{tripPlan.GeneratedAt:yyyy年MM月dd日 HH:mm}";

        return (subject, body);
    }

    /// <summary>
    /// 记录通知日志
    /// </summary>
    private void LogNotification(string type, string customerName, string contact, string content, bool success, string? errorMessage = null)
    {
        var status = success ? "成功" : "失败";
        var error = errorMessage != null ? $" 错误: {errorMessage}" : "";
        
        Console.WriteLine($"[通知日志] {type} | {customerName} | {contact} | {status}{error}");
        
        // 实际应用中应该写入日志文件或数据库
    }

    /// <summary>
    /// 获取发送统计
    /// </summary>
    public Dictionary<string, object> GetNotificationStatistics()
    {
        // 模拟统计数据 - 实际应该从数据库获取
        return new Dictionary<string, object>
        {
            ["TotalSent"] = 156,
            ["SMSSent"] = 98,
            ["EmailSent"] = 58,
            ["SuccessRate"] = 0.94,
            ["LastSentTime"] = DateTime.Now.AddMinutes(-15)
        };
    }
}

/// <summary>
/// 通知类型枚举
/// </summary>
public enum NotificationType
{
    SMS,
    Email
}
