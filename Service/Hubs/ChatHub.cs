using Microsoft.AspNetCore.SignalR;

namespace WebApi.Hubs
{
    public class ChatHub : Hub
    {
        // It should be here wit same namespace of actual implementation
        // Actual implementation in the other project, but MT Needs the hub for the generic message type
    }
}
