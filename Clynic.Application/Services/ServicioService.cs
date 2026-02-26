using Clynic.Application.DTOs.Servicios;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Application.Interfaces.Services;
using Clynic.Application.Rules;
using Clynic.Domain.Models;
using FluentValidation;

namespace Clynic.Application.Services
{
    public class ServicioService : IServicioService
    {
        private readonly IServicioRepository _repository;
        private readonly ServicioRules _rules;
        private readonly IValidator<CreateServicioDto> _createValidator;
        private readonly IValidator<UpdateServicioDto> _updateValidator;

        public ServicioService(
            IServicioRepository repository,
            ServicioRules rules,
            IValidator<CreateServicioDto> createValidator,
            IValidator<UpdateServicioDto> updateValidator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
        }

        public async Task<IEnumerable<ServicioResponseDto>> ObtenerPorClinicaAsync(int idClinica, string? nombre = null, bool incluirInactivos = false)
        {
            if (idClinica <= 0)
                throw new ArgumentException("El ID de la clínica debe ser mayor a cero.", nameof(idClinica));

            var servicios = await _repository.ObtenerPorClinicaAsync(idClinica, nombre, incluirInactivos);
            return servicios.Select(MapToResponseDto);
        }

        public async Task<ServicioResponseDto?> ObtenerPorIdAsync(int id, bool incluirInactivos = false)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a cero.", nameof(id));

            var servicio = await _repository.ObtenerPorIdAsync(id, incluirInactivos);
            return servicio != null ? MapToResponseDto(servicio) : null;
        }

        public async Task<ServicioResponseDto> CrearAsync(CreateServicioDto createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errores = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Errores de validación: {errores}");
            }

            var servicio = new Servicio
            {
                IdClinica = createDto.IdClinica,
                NombreServicio = createDto.NombreServicio.Trim(),
                DuracionMin = createDto.DuracionMin,
                PrecioBase = createDto.PrecioBase,
                Activo = true
            };

            var creado = await _repository.CrearAsync(servicio);
            return MapToResponseDto(creado);
        }

        public async Task<ServicioResponseDto?> ActualizarAsync(int id, UpdateServicioDto updateDto)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a cero.", nameof(id));

            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));

            var servicio = await _repository.ObtenerPorIdAsync(id, incluirInactivos: true);
            if (servicio == null)
                return null;

            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                var errores = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Errores de validación: {errores}");
            }

            if (!string.IsNullOrWhiteSpace(updateDto.NombreServicio))
            {
                var nombreNormalizado = updateDto.NombreServicio.Trim();
                var nombreUnico = await _rules.NombreEsUnicoAsync(nombreNormalizado, servicio.IdClinica, servicio.Id);
                if (!nombreUnico)
                    throw new ValidationException("Ya existe un servicio con ese nombre en la clínica.");

                servicio.NombreServicio = nombreNormalizado;
            }

            if (updateDto.DuracionMin.HasValue)
                servicio.DuracionMin = updateDto.DuracionMin.Value;

            if (updateDto.PrecioBase.HasValue)
                servicio.PrecioBase = updateDto.PrecioBase.Value;

            if (updateDto.Activo.HasValue)
                servicio.Activo = updateDto.Activo.Value;

            var actualizado = await _repository.ActualizarAsync(servicio);
            return MapToResponseDto(actualizado);
        }

        public async Task<bool> EliminarAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a cero.", nameof(id));

            return await _repository.EliminarAsync(id);
        }

        private static ServicioResponseDto MapToResponseDto(Servicio servicio)
        {
            return new ServicioResponseDto
            {
                Id = servicio.Id,
                IdClinica = servicio.IdClinica,
                NombreServicio = servicio.NombreServicio,
                DuracionMin = servicio.DuracionMin,
                PrecioBase = servicio.PrecioBase,
                Activo = servicio.Activo
            };
        }
    }
}
