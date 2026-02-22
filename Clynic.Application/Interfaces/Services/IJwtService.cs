using Clynic.Domain.Models;

namespace Clynic.Application.Interfaces.Services
{
    public interface IJwtService
    {
        string GenerarToken(Usuario usuario);
        DateTime ObtenerFechaExpiracion();
    }
}
