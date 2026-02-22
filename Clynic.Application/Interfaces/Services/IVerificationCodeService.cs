using Clynic.Domain.Models;

namespace Clynic.Application.Interfaces.Services
{
    public interface IVerificationCodeService
    {
        string GenerarCodigo(int longitud = 8);
        Task<CodigoVerificacion> CrearCodigoAsync(int idUsuario, TipoCodigo tipo, int minutosExpiracion = 15);
        Task<CodigoVerificacion?> ValidarCodigoAsync(int idUsuario, string codigo, TipoCodigo tipo);
        Task MarcarComoUsadoAsync(CodigoVerificacion codigo);
    }
}
