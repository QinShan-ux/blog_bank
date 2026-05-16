using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BlogBank.Api.Swagger;

/// <summary>
/// 为登录接口在 Swagger UI 中预填默认示例数据。
/// </summary>
public class LoginRequestExampleFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var info = context.MethodInfo.DeclaringType;
        var controllerName = info.Name;
        var functionName = context.MethodInfo.Name;
        if (functionName != "Login") return;


        if (operation.RequestBody?.Content.TryGetValue("application/json", out var mediaType) == true)
        {
            mediaType.Example = new OpenApiObject
            {
                ["username"] = new OpenApiString("superadmin"),
                ["password"] = new OpenApiString("Admin@123")
            };
        }
    }
}