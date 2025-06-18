#pragma warning disable SKEXP0010
using Baodian.AI.SemanticKernel;
using Baodian.AI.SemanticKernel.Milvus.Extensions;
using Baodian.AI.SemanticKernel.Milvus.Services;
using GatewayOperationSystem.Core.Configuration;
using GatewayOperationSystem.Core.Services;
using Microsoft.OpenApi.Models;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();

// 配置 Milvus
builder.Services.Configure<MilvusSettings>(
    builder.Configuration.GetSection("Milvus"));
//builder.Services.AddScoped<IMilvusService, MilvusVectorService>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "游乐行业闸机运营管理系统 API",
        Version = "v1",
        Description = "基于 Semantic Kernel + Milvus 的智能闸机运营解决方案"
    });
});

// 配置 Semantic Kernel
builder.Services.AddBaodianSemanticKernel(builder.Configuration);

// 配置 Milvus 向量数据库
builder.Services.AddMilvus((Action<Baodian.AI.SemanticKernel.Milvus.Configuration.MilvusOptions>)(options =>
{
    options.Port = int.Parse(builder.Configuration.GetValue<string>("Milvus:Port") ?? "9091");
    options.Database = builder.Configuration.GetValue<string>("Milvus:DatabaseName") ?? "default";
    options.Username = builder.Configuration.GetValue<string>("Milvus:Username") ?? "";
    options.Password = builder.Configuration.GetValue<string>("Milvus:Password") ?? "";
    options.ApiKey = builder.Configuration.GetValue<string>("Milvus:ApiKey") ?? "";
    options.Token = builder.Configuration.GetValue<string>("Milvus:Token") ?? "";
    options.EnableSsl = builder.Configuration.GetValue("Milvus:EnableSsl", false);
    options.Endpoint= builder.Configuration.GetValue<string>("Milvus:Endpoint") ?? "localhost";
    options.CollectionName = builder.Configuration.GetValue<string>("Milvus:CollectionName") ?? "default_collection";
    options.Dimension = builder.Configuration.GetValue<int>("Milvus:Dimension", 768);
    options.VectorDimension = builder.Configuration.GetValue<int>("Milvus:VectorDimension", 1536);
    options.IndexType = builder.Configuration.GetValue<string>("Milvus:IndexType") ?? "HNSW";
    options.MetricType = builder.Configuration.GetValue<string>("Milvus:MetricType") ?? "COSINE";
    options.Timeout = builder.Configuration.GetValue<int>("Milvus:Timeout", 30);
    options.Retry.Enabled = builder.Configuration.GetValue<bool>("Milvus:Retry:Enabled", true);
    options.Retry.MaxRetries = builder.Configuration.GetValue<int>("Milvus:Retry:MaxRetries", 3);

}));

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

// 初始化 768维度的 DB_Gate_Knowledge 集合
using (var scope = app.Services.CreateScope())
{
    var collectionService = scope.ServiceProvider.GetRequiredService<CollectionService>();
    try
    {
        var conName = "DB_Gate_Knowledge";
        // 检查集合是否存在，如果不存在则创建
        var exists = await collectionService.ExistsAsync(conName);
        if (!exists)
        {
            var createRequest = new Baodian.AI.SemanticKernel.Milvus.Models.CreateCollectionRequest
            {
                CollectionName = conName,
                Description = "Gateway Knowledge Base Collection with 768 dimensions",
                EnableDynamicField = true,
                Dimension = 768,
                Fields = new List<Baodian.AI.SemanticKernel.Milvus.Models.FieldSchema>
                {
                    new Baodian.AI.SemanticKernel.Milvus.Models.FieldSchema
                    {
                        Name = "id",
                        DataType = "Int64",
                        IsPrimaryKey = true,
                        AutoId = true,
                        TypeParams = new Dictionary<string, object> { { "max_length", 36 } }
                    },
                    new Baodian.AI.SemanticKernel.Milvus.Models.FieldSchema
                    {
                        Name = "vector",
                        DataType = "FloatVector",
                        IsPrimaryKey = false,
                        AutoId = false,
                        TypeParams = new Dictionary<string, object> { { "dim", 768 } }
                    }
                }
            };
            
            await collectionService.CreateCollectionAsync(createRequest);
            await collectionService.LoadCollectionAsync(conName);
            Console.WriteLine("Milvus 768维度集合(DB_Gate_Knowledge)创建并加载成功");
        }
        else
        {
            // 确保集合已加载
            await collectionService.LoadCollectionAsync("DB_Gate_Knowledge");
            Console.WriteLine("Milvus 768维度集合(DB_Gate_Knowledge)已存在并加载成功");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Milvus 集合初始化失败: {ex.Message}");
    }
}

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
    .UseBaodianAIAliyunBailian()
    .UseBaodianAIDeepSeek();

app.MapControllers();

app.Run();


