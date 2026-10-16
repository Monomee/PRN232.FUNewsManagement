using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace HoangNV_SE1930_A01_BE.Hubs;

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
