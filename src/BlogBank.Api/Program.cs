using System.Reflection;
using System.Text;
using BlogBank.Api.Filters;
using BlogBank.Api.Job;
using BlogBank.Api.Middlewares;
using BlogBank.Api.Models;
using BlogBank.Api.profile;
using BlogBank.Api.Swagger;
using BlogBank.Api.Tool;
using BlogBank.Infrastructure.Data;
using BlogBank.Infrastructure.Entities;
using BlogBank.Infrastructure.Extensions;
using BlogBank.Infrastructure.Services;
using BlogBank.Service.Extensions;
using BlogBank.Service.Interfaces;
using BlogBank.Service.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", 
        optional: true, reloadOnChange: true);

// 使用 ReloadOnChange 方式创建 Logger
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .Destructure.With<SensitiveDestructuringPolicy>()
        .ReadFrom.Configuration(context.Configuration)  // 自动感知配置变化、
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});
builder.Services.AddControllers(options =>
{
    // 注册过滤器
    options.Filters.Add<AuditAttribute>();
    options.Filters.Add<DataMaskFilter>();
    options.Filters.Add<OperationLogFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // 自动加载输出目录中所有项目的 XML 注释文件
    foreach (var xmlFile in Directory.GetFiles(AppContext.BaseDirectory, "*.xml"))
        c.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);

    c.OperationFilter<LoginRequestExampleFilter>();

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "输入 JWT Token（不需要 'Bearer ' 前缀）"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            []
        }
    });
});
builder.Services.AddMiniProfiler(options =>
{
    options.RouteBasePath = "/profiler";  // 访问 /profiler/results 看结果
}).AddEntityFramework();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<ExportProcessor>();
builder.Services.AddScoped<IStorageService, LocalStorageService>();

# region mq
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMQ"));

// 从配置创建连接
builder.Services.AddSingleton<IConnection>(sp =>
{
    var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

    var factory = new ConnectionFactory
    {
        HostName         = options.Host,
        Port             = options.Port,
        UserName         = options.UserName,
        Password         = options.Password,
        VirtualHost      = "/",
        AutomaticRecoveryEnabled = true   // 断线自动重连
    };

    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

builder.Services.AddSingleton<RabbitMqInitializer>();

// app.Run() 之前调用，保证队列在消费者启动前已创建
builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
builder.Services.AddHostedService<ExportWorker>();
# endregion
builder.Services.AddApplicationServices();
builder.Services.AddHttpContextAccessor();

var jwtKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew                = TimeSpan.Zero
        };
        // SignalR 的 token 通过 query string 传，不是 Header
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Query["access_token"];
                var path  = ctx.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(token) && path.StartsWithSegments("/hubs"))
                    ctx.Token = token;

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAutoMapper(Assembly.Load("BlogBank.Api"));
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://127.0.0.1:8848",   // 你的前端地址
                "http://localhost:8848",
                "http://localhost:4200")    // 如有其他前端地址也加上
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();          // 允许携带 Cookie/凭证
    });
});

var app = builder.Build();
var initializer = app.Services.GetRequiredService<RabbitMqInitializer>();
await initializer.InitializeAsync();
// 根据配置决定是否自动应用数据库迁移
if (app.Configuration.GetValue<bool>("AutoMigrate"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

// 根据配置决定是否执行初始数据填充
if (bool.TryParse(app.Configuration["Seed:Enabled"], out var seedEnabled) && seedEnabled)
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    try
    {
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var seedLogger = scope.ServiceProvider.GetRequiredService<ILogger<DataSeeder>>();
        seedLogger.LogError(ex, "Seed 执行失败");
    }
}
app.UseMiniProfiler();

if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();  // ← 必须有
app.UseCors();
app.UseAuthentication();
app.UseMiddleware<WhitelistMiddleware>();
app.UseMiddleware<TokenVersionMiddleware>();
app.UseAuthorization();
app.MapHub<ExportHub>("/hubs/export");
app.MapControllers();

app.Run();
