using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Presentation.Hubs
{
    /// <summary>
    /// Real-time notification hub.
    /// Each authenticated user is placed in a group keyed by their userId (NameIdentifier claim).
    /// Push method: "ReceiveNotification" with a StudentNotificationDto payload.
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // UserIdentifier is resolved from the NameIdentifier claim by the default IUserIdProvider
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);

            await base.OnConnectedAsync();
        }
    }
}
