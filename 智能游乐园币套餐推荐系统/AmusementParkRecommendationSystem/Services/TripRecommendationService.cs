using AmusementParkRecommendationSystem.Models;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace AmusementParkRecommendationSystem.Services;

/// <summary>
/// 行程推荐服务 - 核心位置智能服务
/// </summary>
public class TripRecommendationService
{
    private readonly LocationService _locationService;
    private readonly POIService _poiService;
    private readonly AgentManager _agentManager;
    private readonly WeatherService _weatherService;

    public TripRecommendationService(
        LocationService locationService,
        POIService poiService,
        AgentManager agentManager,
        WeatherService weatherService)
    {
        _locationService = locationService;
        _poiService = poiService;
        _agentManager = agentManager;
        _weatherService = weatherService;
    }

    /// <summary>
    /// 生成个性化一日游行程计划
    /// </summary>
    public async Task<TripPlan> GenerateDayTripPlanAsync(
        CustomerLocationPreference customer,
        DateTime plannedDate,
        string? preferredStore = null,
        decimal? budget = null)
    {
        try
        {
            // 1. 获取客户附近的景点和餐厅
            var nearbyStores = await _locationService.GetNearbyStoresAsync(
                customer.CurrentLatitude, customer.CurrentLongitude, customer.PreferredRadius);

            var nearbyAttractions = await _poiService.GetRecommendedAttractionsAsync(
                customer.CurrentLatitude, customer.CurrentLongitude, customer.PreferredRadius * 2);

            var nearbyDining = await _poiService.GetRecommendedDiningAsync(
                customer.CurrentLatitude, customer.CurrentLongitude, customer.PreferredRadius * 2);

            // 2. 获取天气信息
            var weatherInfo = await _weatherService.GetWeatherInfoAsync(
                customer.CurrentLatitude, customer.CurrentLongitude, plannedDate);

            // 3. 使用AI智能体生成推荐
            var tripPlannerAgent = _agentManager.GetAgent("TripPlannerAgent");
            var diningAgent = _agentManager.GetAgent("DiningRecommendationAgent");
            var weatherAgent = _agentManager.GetAgent("WeatherAndRealtimeAgent");

            // 4. 构建上下文信息
            var contextData = new
            {
                Customer = new
                {
                    customer.Name,
                    customer.Age,
                    Preferences = customer.PreferredCategories,
                    Budget = budget,
                    Location = new { customer.CurrentLatitude, customer.CurrentLongitude }
                },
                NearbyStores = nearbyStores.Take(5),
                NearbyAttractions = nearbyAttractions.Take(10),
                NearbyDining = nearbyDining.Take(8),
                Weather = weatherInfo,
                PlannedDate = plannedDate.ToString("yyyy-MM-dd")
            };

            var contextJson = JsonSerializer.Serialize(contextData, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            // 5. 获取行程规划建议
            var tripPlanPrompt = $@"
请为客户制定详细的一日游行程计划。

客户信息和上下文：
{contextJson}

请制定一个从上午9点到晚上8点的详细行程安排，包括：
1. 最佳的游览顺序和时间安排
2. 推荐的景点和活动
3. 用餐时间和餐厅推荐
4. 交通路线和方式
5. 考虑天气因素的建议
6. 预算控制和费用估算
7. 应急备选方案

请提供结构化的行程计划。";

            var tripPlanResponse = await GetAgentResponseAsync(tripPlannerAgent, tripPlanPrompt);

            // 6. 获取餐饮推荐
            var diningPrompt = $@"
基于以下信息，推荐适合的餐厅：

客户和环境信息：
{contextJson}

请重点推荐：
1. 早餐/上午茶时间（9:00-11:00）
2. 午餐时间（12:00-14:00）  
3. 下午茶时间（15:00-16:00）
4. 晚餐时间（17:30-19:30）

每个时段推荐2-3个选择，考虑位置便利性、口味偏好和预算。";

            var diningResponse = await GetAgentResponseAsync(diningAgent, diningPrompt);

            // 7. 获取天气和实时信息建议
            var weatherPrompt = $@"
分析天气对行程的影响并提供建议：

相关信息：
{contextJson}

请提供：
1. 天气对各景点游览的影响分析
2. 基于天气的活动调整建议
3. 交通状况和最佳出行时间
4. 安全注意事项和携带物品建议
5. 天气变化的应对方案";

            var weatherResponse = await GetAgentResponseAsync(weatherAgent, weatherPrompt);

            // 8. 构建完整的行程计划
            var tripPlan = await BuildTripPlanAsync(
                customer, plannedDate, nearbyStores, nearbyAttractions, nearbyDining,
                weatherInfo, tripPlanResponse, diningResponse, weatherResponse, budget);

            return tripPlan;
        }
        catch (Exception ex)
        {
            throw new Exception($"生成行程计划失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 检查并邀请附近的客户
    /// </summary>
    public async Task<List<InvitationMessage>> CheckAndInviteNearbyCustomersAsync(
        int storeId, double invitationRadius = 2000)
    {
        var invitations = new List<InvitationMessage>();

        try
        {
            // 获取门店信息（实际应该从数据库获取）
            var stores = await _locationService.GetNearbyStoresAsync(39.865895, 116.497395, 100);
            var targetStore = stores.FirstOrDefault(s => s.Id == storeId);
            
            if (targetStore == null)
                return invitations;

            // 模拟获取客户位置数据（实际应该从数据库获取）
            var nearbyCustomers = GetMockCustomerLocations()
                .Where(customer => _locationService.IsCustomerInInvitationRange(
                    customer, targetStore, invitationRadius))
                .ToList();

            var locationAgent = _agentManager.GetAgent("LocationIntelligenceAgent");

            foreach (var customer in nearbyCustomers)
            {
                // 分析邀请时机和策略
                var analysisPrompt = $@"
分析客户邀请策略：

客户信息：
- 姓名：{customer.Name}
- 年龄：{customer.Age}
- 当前位置：({customer.CurrentLatitude}, {customer.CurrentLongitude})
- 偏好类别：{string.Join(", ", customer.PreferredCategories)}
- 首选距离：{customer.PreferredRadius}米

目标门店：
- 名称：{targetStore.Name}
- 地址：{targetStore.Address}
- 特色：{string.Join(", ", targetStore.Features)}
- 评分：{targetStore.Rating}

请分析：
1. 邀请的最佳时机和方式
2. 个性化的邀请内容建议
3. 突出客户可能感兴趣的特色
4. 预估客户响应的可能性
5. 建议的优惠或激励方案

请提供具体的邀请策略建议。";

                var analysisResponse = await GetAgentResponseAsync(locationAgent, analysisPrompt);

                var invitation = new InvitationMessage
                {
                    CustomerId = customer.CustomerId,
                    CustomerName = customer.Name,
                    StoreId = targetStore.Id,
                    StoreName = targetStore.Name,
                    Message = $"亲爱的{customer.Name}，您好！我们发现您正在{targetStore.Name}附近，" +
                             $"这里有{string.Join("、", targetStore.Features)}等精彩项目，" +
                             $"现在来玩还有特别优惠哦！距离您只有{Math.Round(_locationService.CalculateDistance(customer.CurrentLatitude, customer.CurrentLongitude, targetStore.Latitude, targetStore.Longitude))}米，" +
                             $"步行{Math.Round(_locationService.CalculateDistance(customer.CurrentLatitude, customer.CurrentLongitude, targetStore.Latitude, targetStore.Longitude) / 83.33)}分钟即可到达。快来享受欢乐时光吧！",
                    SendTime = DateTime.Now,
                    InvitationType = "LocationBased",
                    CustomerLatitude = customer.CurrentLatitude,
                    CustomerLongitude = customer.CurrentLongitude,
                    DistanceToStore = _locationService.CalculateDistance(
                        customer.CurrentLatitude, customer.CurrentLongitude,
                        targetStore.Latitude, targetStore.Longitude),
                    RecommendedOffers = GetRecommendedOffers(customer, targetStore),
                    AIAnalysis = analysisResponse,
                    EstimatedResponseProbability = CalculateResponseProbability(customer, targetStore)
                };

                invitations.Add(invitation);
            }

            return invitations;
        }
        catch (Exception ex)
        {
            throw new Exception($"检查邀请附近客户失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 获取智能体响应
    /// </summary>
    private async Task<string> GetAgentResponseAsync(Microsoft.SemanticKernel.Agents.ChatCompletionAgent? agent, string prompt)
    {
        if (agent == null)
            return "智能体不可用";

        try
        {
            var chatHistory = new ChatHistory();
            if (!string.IsNullOrEmpty(agent.Instructions))
            {
                chatHistory.AddSystemMessage(agent.Instructions);
            }
            chatHistory.AddUserMessage(prompt);

            var chatCompletionService = agent.Kernel.GetRequiredService<IChatCompletionService>();
            var responses = await chatCompletionService.GetChatMessageContentsAsync(chatHistory, null, agent.Kernel);
            return responses.FirstOrDefault()?.Content ?? "无响应";
        }
        catch (Exception ex)
        {
            return $"获取智能体响应失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 构建完整的行程计划
    /// </summary>
    private async Task<TripPlan> BuildTripPlanAsync(
        CustomerLocationPreference customer,
        DateTime plannedDate,
        List<StoreLocation> nearbyStores,
        List<AttractionOption> nearbyAttractions,
        List<DiningOption> nearbyDining,
        WeatherInfo weatherInfo,
        string tripPlanResponse,
        string diningResponse,
        string weatherResponse,
        decimal? budget)
    {
        var tripPlan = new TripPlan
        {
            CustomerId = customer.CustomerId,
            CustomerName = customer.Name,
            PlannedDate = plannedDate,
            StartTime = plannedDate.Date.AddHours(9),
            EndTime = plannedDate.Date.AddHours(20),
            TotalBudget = budget ?? 500m,
            WeatherCondition = weatherInfo.Condition,
            Description = $"为{customer.Name}定制的{plannedDate:MM月dd日}一日游行程",
            
            // 选择推荐的景点（前6个）
            Attractions = nearbyAttractions.Take(6).ToList(),
            
            // 选择推荐的餐厅（前4个）
            DiningOptions = nearbyDining.Take(4).ToList(),
            
            // 生成交通计划
            Transportation = await GenerateTransportationPlanAsync(customer, nearbyAttractions.Take(3).ToList()),
            
            // AI生成的详细建议
            AIRecommendations = $@"
=== 行程规划建议 ===
{tripPlanResponse}

=== 餐饮推荐 ===
{diningResponse}

=== 天气和实时信息 ===
{weatherResponse}
",
            
            GeneratedAt = DateTime.Now,
            EstimatedTotalCost = CalculateEstimatedCost(nearbyAttractions.Take(6).ToList(), nearbyDining.Take(4).ToList()),
            Notes = GetPlanningNotes(weatherInfo, customer)
        };

        return tripPlan;
    }

    /// <summary>
    /// 生成交通计划
    /// </summary>
    private async Task<List<TransportationPlan>> GenerateTransportationPlanAsync(
        CustomerLocationPreference customer, 
        List<AttractionOption> attractions)
    {
        var transportationPlans = new List<TransportationPlan>();
        
        if (!attractions.Any())
            return transportationPlans;

        var currentLat = customer.CurrentLatitude;
        var currentLon = customer.CurrentLongitude;
        var currentTime = customer.PreferredVisitTime?.Date.AddHours(9) ?? DateTime.Today.AddHours(9);

        // 添加从客户位置到第一个景点的交通
        var firstAttraction = attractions.First();
        var routeToFirst = await _locationService.CalculateRouteAsync(
            currentLat, currentLon, firstAttraction.Latitude, firstAttraction.Longitude, "walking");

        if (routeToFirst.HasValue)
        {
            transportationPlans.Add(new TransportationPlan
            {
                FromLocation = "起始位置",
                ToLocation = firstAttraction.Name,
                TransportMode = "步行",
                Distance = routeToFirst.Value.DistanceMeters,
                EstimatedDuration = routeToFirst.Value.DurationSeconds / 60,
                DepartureTime = currentTime,
                ArrivalTime = currentTime.AddSeconds(routeToFirst.Value.DurationSeconds),
                Cost = 0,
                Route = string.Join(" -> ", routeToFirst.Value.Route.Select(p => $"({p.Lat:F4},{p.Lon:F4})"))
            });
        }

        // 添加景点间的交通
        for (int i = 0; i < attractions.Count - 1; i++)
        {
            var from = attractions[i];
            var to = attractions[i + 1];
            
            var route = await _locationService.CalculateRouteAsync(
                from.Latitude, from.Longitude, to.Latitude, to.Longitude, "walking");

            if (route.HasValue)
            {
                var previousPlan = transportationPlans.LastOrDefault();
                var departureTime = previousPlan?.ArrivalTime.AddMinutes(from.EstimatedVisitDuration) ?? currentTime;

                transportationPlans.Add(new TransportationPlan
                {
                    FromLocation = from.Name,
                    ToLocation = to.Name,
                    TransportMode = route.Value.DistanceMeters > 1000 ? "公交/地铁" : "步行",
                    Distance = route.Value.DistanceMeters,
                    EstimatedDuration = route.Value.DurationSeconds / 60,
                    DepartureTime = departureTime,
                    ArrivalTime = departureTime.AddSeconds(route.Value.DurationSeconds),
                    Cost = route.Value.DistanceMeters > 1000 ? 5m : 0m,
                    Route = string.Join(" -> ", route.Value.Route.Select(p => $"({p.Lat:F4},{p.Lon:F4})"))
                });
            }
        }

        return transportationPlans;
    }

    /// <summary>
    /// 计算预估总费用
    /// </summary>
    private decimal CalculateEstimatedCost(List<AttractionOption> attractions, List<DiningOption> dining)
    {
        var attractionCost = attractions.Sum(a => a.TicketPrice);
        var diningCost = dining.Sum(d => EstimateDiningCost(d.PriceRange));
        var transportCost = 50m; // 估算交通费用
        
        return attractionCost + diningCost + transportCost;
    }

    /// <summary>
    /// 估算餐饮费用
    /// </summary>
    private decimal EstimateDiningCost(string priceRange)
    {
        return priceRange switch
        {
            var range when range.Contains("80-150") => 115m,
            var range when range.Contains("40-80") => 60m,
            var range when range.Contains("20-40") => 30m,
            var range when range.Contains("10-30") => 20m,
            _ => 50m
        };
    }

    /// <summary>
    /// 获取规划备注
    /// </summary>
    private string GetPlanningNotes(WeatherInfo weather, CustomerLocationPreference customer)
    {
        var notes = new List<string>();
        
        if (weather.Condition.Contains("雨"))
        {
            notes.Add("建议携带雨具，优先选择室内项目");
        }
        
        if (weather.Temperature < 5)
        {
            notes.Add("天气较冷，注意保暖，建议选择室内活动");
        }
        else if (weather.Temperature > 30)
        {
            notes.Add("天气炎热，注意防晒补水，避开正午时段");
        }
        
        if (customer.Age < 12)
        {
            notes.Add("为儿童客户，重点推荐亲子友好的项目");
        }
        else if (customer.Age > 60)
        {
            notes.Add("为老年客户，建议选择较为温和的项目");
        }
        
        notes.Add("建议提前查看各景点的开放时间和人流情况");
        notes.Add("携带身份证件以备检票需要");
        
        return string.Join("; ", notes);
    }

    /// <summary>
    /// 获取推荐优惠
    /// </summary>
    private List<string> GetRecommendedOffers(CustomerLocationPreference customer, StoreLocation store)
    {
        var offers = new List<string>();
        
        if (customer.Age < 12)
        {
            offers.Add("儿童票8折优惠");
        }
        else if (customer.Age > 60)
        {
            offers.Add("老年人专享7折优惠");
        }
        
        offers.Add("新客户首次游玩9折");
        offers.Add("今日限时特惠：买一送一小食");
        
        if (store.Features.Contains("水上乐园"))
        {
            offers.Add("水上项目套票优惠100元");
        }
        
        return offers;
    }

    /// <summary>
    /// 计算响应概率
    /// </summary>
    private double CalculateResponseProbability(CustomerLocationPreference customer, StoreLocation store)
    {
        var baseProb = 0.3; // 基础响应率30%
        
        // 距离越近，响应率越高
        var distance = _locationService.CalculateDistance(
            customer.CurrentLatitude, customer.CurrentLongitude,
            store.Latitude, store.Longitude);
        
        if (distance < 500) baseProb += 0.3;
        else if (distance < 1000) baseProb += 0.2;
        else if (distance < 2000) baseProb += 0.1;
        
        // 偏好匹配度影响
        var matchingFeatures = store.Features.Intersect(customer.PreferredCategories).Count();
        baseProb += matchingFeatures * 0.1;
        
        // 门店评分影响
        baseProb += (store.Rating - 4.0) * 0.1;
          return Math.Min(0.95, Math.Max(0.1, baseProb));
    }

    /// <summary>
    /// 生成个性化行程计划
    /// </summary>
    public async Task<TripPlan?> GeneratePersonalizedTripPlanAsync(int customerId, int? targetStoreId = null)
    {
        try
        {
            // 获取客户位置偏好
            var customers = GetMockCustomerLocations();
            var customer = customers.FirstOrDefault(c => c.CustomerId == customerId);
            if (customer == null)
            {
                return null;
            }        // 获取目标门店信息
        var dataService = new DataService(); // 应该通过依赖注入获取
        var stores = dataService.GetAllStoreLocations();
        var targetStore = targetStoreId.HasValue ? 
            stores.FirstOrDefault(s => s.Id == targetStoreId.Value) : 
            stores.OrderBy(s => _locationService.CalculateDistance(
                customer.CurrentLatitude, customer.CurrentLongitude, s.Latitude, s.Longitude))
            .FirstOrDefault();

            if (targetStore == null)
            {
                return null;
            }

            // 获取附近景点和餐饮
            var nearbyAttractions = await _poiService.GetRecommendedAttractionsAsync(
                targetStore.Latitude, targetStore.Longitude, 2000);

            var nearbyDining = await _poiService.GetRecommendedDiningAsync(
                targetStore.Latitude, targetStore.Longitude, 2000);            // 获取天气信息
            var weather = await _weatherService.GetWeatherInfoAsync(
                targetStore.Latitude, targetStore.Longitude, DateTime.Today.AddDays(1));

            // 计算到达时间和交通方案
            var distance = _locationService.CalculateDistance(
                customer.CurrentLatitude, customer.CurrentLongitude,
                targetStore.Latitude, targetStore.Longitude);

            var transportationType = distance < 1000 ? "步行" : distance < 5000 ? "地铁/公交" : "打车";
            var estimatedTravelTime = distance < 1000 ? (int)(distance / 83.33) : // 步行 5km/h
                                     distance < 5000 ? (int)(distance / 333.33) + 10 : // 地铁 20km/h + 等待
                                     (int)(distance / 500); // 打车 30km/h

            var tripPlan = new TripPlan
            {
                CustomerId = customerId,
                CustomerName = customer.Name,
                TargetStoreId = targetStore.Id,
                TargetStoreName = targetStore.Name,
                PlannedDate = DateTime.Today.AddDays(1),                StartTime = customer.PreferredVisitTime ?? DateTime.Today.AddHours(9),
                EndTime = (customer.PreferredVisitTime ?? DateTime.Today.AddHours(9)).AddHours(6),
                SuggestedStartTime = customer.PreferredVisitTime,
                EstimatedDuration = 6,
                EstimatedBudget = 500,
                TotalBudget = 500,
                EstimatedTotalCost = 450,                Transportation = new List<TransportationPlan>
                {
                    new TransportationPlan
                    {
                        TransportType = transportationType,
                        Route = $"从当前位置到{targetStore.Name}",
                        EstimatedDuration = estimatedTravelTime,
                        EstimatedCost = transportationType == "步行" ? 0 : 
                                       transportationType.Contains("地铁") ? 6 : 
                                       (decimal)(distance * 0.002)
                    }
                },
                Attractions = nearbyAttractions.Take(5).ToList(),
                DiningOptions = nearbyDining.Take(3).ToList(),
                WeatherCondition = $"{weather.Condition}, {weather.Temperature}°C",
                Notes = "建议提前预定热门项目，携带防晒用品",
                AIRecommendations = "根据您的偏好，推荐优先体验刺激项目，下午安排休闲活动",
                GeneratedAt = DateTime.Now,
                Suggestions = new List<string>
                {
                    $"建议{customer.PreferredVisitTime:HH:mm}到达，避开人流高峰",
                    "携带充电宝和防晒用品",
                    "提前下载园区地图和排队APP",
                    "合理安排用餐时间，避开12-14点高峰期"
                }
            };

            return tripPlan;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"生成行程计划时出错: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 获取附近景点推荐
    /// </summary>
    public async Task<List<AttractionOption>> GetNearbyAttractionsAsync(double latitude, double longitude, double radiusMeters)
    {
        try
        {
            return await _poiService.GetRecommendedAttractionsAsync(latitude, longitude, radiusMeters);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取附近景点时出错: {ex.Message}");
            return new List<AttractionOption>();
        }
    }

    /// <summary>
    /// 获取基于天气的建议
    /// </summary>
    public async Task<WeatherAdvice> GetWeatherBasedAdviceAsync(double latitude, double longitude)
    {
        try
        {
            var weather = await _weatherService.GetWeatherInfoAsync(latitude, longitude, DateTime.Today);
            
            var advice = new WeatherAdvice
            {
                CurrentWeather = weather.Condition,
                Temperature = weather.Temperature,
                Humidity = weather.Humidity,
                WindSpeed = weather.WindSpeed,
                Suggestions = new List<string>(),
                RecommendedActivities = new List<string>(),
                WarningMessages = new List<string>()
            };

            // 根据天气条件提供建议
            if (weather.Temperature < 10)
            {
                advice.Suggestions.Add("建议穿着保暖衣物，室内项目更适合");
                advice.RecommendedActivities.Add("室内游乐设施");
                advice.RecommendedActivities.Add("主题餐厅体验");
            }
            else if (weather.Temperature > 30)
            {
                advice.Suggestions.Add("建议携带防晒用品，多补充水分");
                advice.Suggestions.Add("选择有遮阴的区域休息");
                advice.RecommendedActivities.Add("水上项目");
                advice.RecommendedActivities.Add("室内冷气场所");
                advice.WarningMessages.Add("高温天气，注意防晒和补水");
            }
            else
            {
                advice.Suggestions.Add("天气宜人，适合户外活动");
                advice.RecommendedActivities.Add("过山车等刺激项目");
                advice.RecommendedActivities.Add("园区徒步游览");
            }

            if (weather.Condition.Contains("雨"))
            {
                advice.Suggestions.Add("建议携带雨具，选择室内项目");
                advice.RecommendedActivities.Clear();
                advice.RecommendedActivities.Add("室内游乐设施");
                advice.RecommendedActivities.Add("主题展览");
                advice.WarningMessages.Add("雨天路滑，注意安全");
            }

            if (int.TryParse(weather.WindSpeed, out int windSpeed) && windSpeed > 20)
            {
                advice.WarningMessages.Add("风力较大，部分高空项目可能暂停");
            }

            return advice;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取天气建议时出错: {ex.Message}");
            return new WeatherAdvice
            {
                CurrentWeather = "未知",
                Temperature = 20,
                Humidity = 60,
                WindSpeed = "5",
                Suggestions = new List<string> { "无法获取天气信息，建议关注实时天气" },
                RecommendedActivities = new List<string> { "根据实际天气情况选择活动" },
                WarningMessages = new List<string>()
            };
        }
    }

    /// <summary>
    /// 获取模拟客户位置数据
    /// </summary>
    private List<CustomerLocationPreference> GetMockCustomerLocations()
    {
        return new List<CustomerLocationPreference>
        {
            new()
            {
                CustomerId = 1,
                Name = "张小明",
                Age = 25,
                CurrentLatitude = 39.866500,
                CurrentLongitude = 116.497800,
                PreferredCategories = new() { "过山车", "刺激项目" },
                PreferredRadius = 3000,
                PreferredVisitTime = DateTime.Today.AddHours(14)
            },
            new()
            {
                CustomerId = 2,
                Name = "李小红",
                Age = 8,
                CurrentLatitude = 39.865200,
                CurrentLongitude = 116.496900,
                PreferredCategories = new() { "儿童项目", "亲子活动" },
                PreferredRadius = 2000,
                PreferredVisitTime = DateTime.Today.AddHours(10)
            },
            new()
            {
                CustomerId = 3,
                Name = "王大爷",
                Age = 65,
                CurrentLatitude = 39.866800,
                CurrentLongitude = 116.498200,
                PreferredCategories = new() { "景点", "文化体验" },
                PreferredRadius = 1500,
                PreferredVisitTime = DateTime.Today.AddHours(9)
            }
        };
    }
}
