using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Baodian.AI.SemanticKernel.Plugins;

/// <summary>
/// 天气插件示例
/// </summary>
public class WeatherPlugin
{
    /// <summary>
    /// 获取天气信息
    /// </summary>
    /// <param name="city">城市名称</param>
    /// <returns>天气信息</returns>
    [KernelFunction("GetWeather")]
    [Description("获取指定城市的天气信息")]
    public async Task<string> GetWeatherAsync(string city)
    {
        // 这里应该是实际的天气API调用
        // 这里仅作为示例返回模拟数据
        await Task.Delay(100); // 模拟API调用延迟
        return $"城市 {city} 的天气：晴朗，温度 25°C";
    }

    /// <summary>
    /// 获取未来天气预报
    /// </summary>
    /// <param name="city">城市名称</param>
    /// <param name="days">预报天数</param>
    /// <returns>天气预报信息</returns>
    [KernelFunction("GetForecast")]
    [Description("获取指定城市的未来天气预报")]
    public async Task<string> GetForecastAsync(string city, int days = 3)
    {
        // 这里应该是实际的天气预报API调用
        // 这里仅作为示例返回模拟数据
        await Task.Delay(100); // 模拟API调用延迟
        return $"城市 {city} 未来 {days} 天的天气预报：\n" +
               "第1天：晴朗，温度 25°C\n" +
               "第2天：多云，温度 23°C\n" +
               "第3天：小雨，温度 20°C";
    }
} 