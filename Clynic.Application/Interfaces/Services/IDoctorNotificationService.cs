namespace Clynic.Application.Interfaces.Services
{
    public interface IDoctorNotificationService
    {
        Task NotifyQueueUpdatedAsync(int idDoctor, int idCita, string evento, string mensaje);
    }
}
