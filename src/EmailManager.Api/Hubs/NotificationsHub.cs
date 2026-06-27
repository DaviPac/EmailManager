using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EmailManager.Api.Hubs;

[Authorize]
public class NotificationsHub : Hub
{
    
}