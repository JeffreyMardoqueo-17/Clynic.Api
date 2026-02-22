using System.Security.Cryptography;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Application.Interfaces.Services;
using Clynic.Domain.Models;

namespace Clynic.Application.Services
{
    public class VerificationCodeService : IVerificationCodeService
    {
        private readonly ICodigoVerificacionRepository _repository;

        public VerificationCodeService(ICodigoVerificacionRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public string GenerarCodigo(int longitud = 8)
        {
            const string caracteres = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var codigo = new char[longitud];
            
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[longitud];
            rng.GetBytes(bytes);

            for (int i = 0; i < longitud; i++)
            {
                codigo[i] = caracteres[bytes[i] % caracteres.Length];
            }

            return new string(codigo);
        }

        public async Task<CodigoVerificacion> CrearCodigoAsync(int idUsuario, TipoCodigo tipo, int minutosExpiracion = 15)
        {
            await _repository.InvalidarCodigosAnterioresAsync(idUsuario, tipo);

            var codigo = new CodigoVerificacion
            {
                IdUsuario = idUsuario,
                Codigo = GenerarCodigo(8),
                Tipo = tipo,
                FechaCreacion = DateTime.UtcNow,
                FechaExpiracion = DateTime.UtcNow.AddMinutes(minutosExpiracion),
                Usado = false
            };

            return await _repository.CrearAsync(codigo);
        }

        public async Task<CodigoVerificacion?> ValidarCodigoAsync(int idUsuario, string codigo, TipoCodigo tipo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return null;

            var codigoValido = await _repository.ObtenerCodigoValidoAsync(idUsuario, codigo.ToUpper().Trim(), tipo);
            
            return codigoValido;
        }

        public async Task MarcarComoUsadoAsync(CodigoVerificacion codigo)
        {
            if (codigo == null)
                throw new ArgumentNullException(nameof(codigo));

            codigo.Usado = true;
            codigo.FechaUso = DateTime.UtcNow;

            await _repository.MarcarComoUsadoAsync(codigo);
        }
    }
}
