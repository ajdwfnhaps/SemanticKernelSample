//using Baodian.AI.SemanticKernel.Services;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.SemanticKernel.ChatCompletion;
//using System.Threading.Tasks;

//namespace Baodian.AI.SemanticKernel.Controllers;

///// <summary>
///// 聊天控制器
///// </summary>
//[ApiController]
//[Route("api/[controller]")]
//public class ChatController : ControllerBase
//{
//    private readonly ChatService _chatService;

//    /// <summary>
//    /// 构造函数
//    /// </summary>
//    /// <param name="chatService">聊天服务</param>
//    public ChatController(ChatService chatService)
//    {
//        _chatService = chatService;
//    }

//    /// <summary>
//    /// 发送单条消息
//    /// </summary>
//    /// <param name="message">用户消息</param>
//    /// <returns>AI回复</returns>
//    [HttpPost("send")]
//    public async Task<IActionResult> SendMessage([FromBody] string message)
//    {
//        var response = await _chatService.SendMessageAsync(message);
//        return Ok(response);
//    }

//    /// <summary>
//    /// 发送带上下文的聊天消息
//    /// </summary>
//    /// <param name="messages">消息历史</param>
//    /// <returns>AI回复</returns>
//    [HttpPost("send-with-history")]
//    public async Task<IActionResult> SendMessageWithHistory([FromBody] ChatHistory messages)
//    {
//        var response = await _chatService.SendMessageWithHistoryAsync(messages);
//        return Ok(response);
//    }
//} 