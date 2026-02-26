namespace Clynic.Application.Rules
{
    public class CitaRules
    {
        public bool FechaAgendamientoValida(DateTime fechaHoraInicio)
        {
            return fechaHoraInicio >= DateTime.UtcNow.AddMinutes(-1);
        }

        public bool TieneServicios(IEnumerable<int>? idsServicios)
        {
            return idsServicios != null && idsServicios.Any();
        }
    }
}
