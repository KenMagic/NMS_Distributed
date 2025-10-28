using Microsoft.AspNetCore.SignalR;

namespace FUNewsManagement_FE.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendNotification(string title, string url)
        {
            // Gửi cho tất cả client
            await Clients.All.SendAsync("ReceiveNotification", title, url);
        }
    }
}
