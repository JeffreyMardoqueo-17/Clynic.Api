using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Clynic.Api.Hubs
{
    [Authorize(Roles = "Doctor,Admin,Recepcionista")]
    public class DoctorQueueHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userIdClaim = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId) && userId > 0)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"doctor:{userId}");
            }

            var idClinicaClaim = Context.User?.FindFirst("IdClinica")?.Value;
            if (int.TryParse(idClinicaClaim, out var idClinica) && idClinica > 0)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"clinic:{idClinica}");
            }

            var idSucursalClaim = Context.User?.FindFirst("IdSucursal")?.Value;
            if (int.TryParse(idSucursalClaim, out var idSucursal) && idSucursal > 0)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"sucursal:{idSucursal}");
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

            var idClinicaClaim = Context.User?.FindFirst("IdClinica")?.Value;
            if (int.TryParse(idClinicaClaim, out var idClinica) && idClinica > 0)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"clinic:{idClinica}");
            }

            var idSucursalClaim = Context.User?.FindFirst("IdSucursal")?.Value;
            if (int.TryParse(idSucursalClaim, out var idSucursal) && idSucursal > 0)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"sucursal:{idSucursal}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}

