using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Clynic.Api.Hubs
{
    [Authorize(Roles = "Doctor,Nutricionista,Fisioterapeuta,Admin")]
    public class DoctorQueueHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userIdClaim = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId) && userId > 0)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"doctor:{userId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userIdClaim = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId) && userId > 0)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"doctor:{userId}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
