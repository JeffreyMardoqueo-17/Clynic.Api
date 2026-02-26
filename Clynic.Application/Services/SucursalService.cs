using FluentValidation;
using Clynic.Application.DTOs.Sucursales;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Application.Interfaces.Services;
using Clynic.Domain.Models;

namespace Clynic.Application.Services
{
    /// <summary>
    /// Servicio de Sucursales con la logica de negocio
    /// </summary>
    public class SucursalService : ISucursalService
    {
        private readonly ISucursalRepository _repository;
        private readonly IValidator<CreateSucursalDto> _createValidator;

        public SucursalService(
            ISucursalRepository repository,
            IValidator<CreateSucursalDto> createValidator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
        }

        public async Task<IEnumerable<SucursalResponseDto>> ObtenerTodasAsync()
        {
            var sucursales = await _repository.ObtenerTodasAsync();
            return sucursales.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<SucursalResponseDto>> ObtenerPorClinicaAsync(int idClinica)
        {
            if (idClinica <= 0)
                throw new ArgumentException("El ID de clínica debe ser mayor a cero.", nameof(idClinica));

            var sucursales = await _repository.ObtenerPorClinicaAsync(idClinica);
            return sucursales.Select(MapToResponseDto);
        }

        public async Task<SucursalResponseDto?> ObtenerPorIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a cero.", nameof(id));

            var sucursal = await _repository.ObtenerPorIdAsync(id);
            return sucursal != null ? MapToResponseDto(sucursal) : null;
        }

        public async Task<SucursalResponseDto> CrearAsync(CreateSucursalDto createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errores = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Errores de validacion: {errores}");
            }

            var sucursal = new Sucursal
            {
                IdClinica = createDto.IdClinica,
                Nombre = createDto.Nombre.Trim(),
                Direccion = createDto.Direccion.Trim(),
                Activa = true
            };

            var sucursalCreada = await _repository.CrearAsync(sucursal);
            return MapToResponseDto(sucursalCreada);
        }

        private static SucursalResponseDto MapToResponseDto(Sucursal sucursal)
        {
            return new SucursalResponseDto
            {
                Id = sucursal.Id,
                IdClinica = sucursal.IdClinica,
                Nombre = sucursal.Nombre,
                Direccion = sucursal.Direccion,
                Activa = sucursal.Activa
            };
        }
    }
}
