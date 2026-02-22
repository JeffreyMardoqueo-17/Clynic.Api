using Clynic.Domain.Models;

namespace Clynic.Application.Interfaces.Repositories
{
    public interface ICodigoVerificacionRepository
    {
        Task<CodigoVerificacion?> ObtenerCodigoValidoAsync(int idUsuario, string codigo, TipoCodigo tipo);
        Task<CodigoVerificacion> CrearAsync(CodigoVerificacion codigo);
        Task MarcarComoUsadoAsync(CodigoVerificacion codigo);
        Task InvalidarCodigosAnterioresAsync(int idUsuario, TipoCodigo tipo);
    }
}
