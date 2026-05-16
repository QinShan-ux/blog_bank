using Microsoft.AspNetCore.SignalR;

namespace BlogBank.Api.Models;

public class ExportHub : Hub
{
    // 客户端连接时，把 connectionId 和 userId 关联起来
    // 框架会自动处理，只需继承 Hub 即可
    // 如果需要自定义连接/断开逻辑才重写
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        Console.WriteLine($"新连接 ConnectionId={Context.ConnectionId} UserId {userId}");
        await base.OnConnectedAsync();
    }
    
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}