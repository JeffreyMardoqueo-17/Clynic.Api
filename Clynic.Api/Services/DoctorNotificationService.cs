using Clynic.Application.Interfaces.Services;
using Clynic.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Clynic.Api.Services
{
    public class DoctorNotificationService : IDoctorNotificationService
    {
        private readonly IHubContext<DoctorQueueHub> _hubContext;

        public DoctorNotificationService(IHubContext<DoctorQueueHub> hubContext)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        public async Task NotifyQueueUpdatedAsync(int idDoctor, int idCita, string evento, string mensaje)
        {
            if (idDoctor <= 0)
            {
                return;
            }

            await _hubContext.Clients
                .Group($"doctor:{idDoctor}")
                .SendAsync("doctor-queue-updated", new
                {
                    idDoctor,
                    idCita,
                    evento,
                    mensaje,
                    fecha = DateTime.UtcNow
                });
        }
    }
}
