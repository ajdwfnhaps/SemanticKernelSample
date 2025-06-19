#pragma warning disable SKEXP0010
using Baodian.AI.SemanticKernel;
using static Baodian.AI.SemanticKernel.ServiceCollectionExtensions;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "百炼 AI + Semantic Kernel + Milvus 智能系统 API",
        Version = "v2.0",
        Description = "基于百炼 text-embedding-v4 + Semantic Kernel + Milvus 的智能解决方案，集成 Memory 模块"
    });
});

// 配置完整的 AI 服务（包括 Memory 模块）
builder.Services.AddBaodianAI(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 启用静态文件服务
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseAuthorization()
   .UseBaodianAIDeepSeek();

app.MapControllers();

app.Run();


