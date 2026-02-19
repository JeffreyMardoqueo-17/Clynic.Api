using FluentValidation;
using Clynic.Application.DTOs.Clinicas;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Application.Interfaces.Services;
using Clynic.Domain.Models;

namespace Clynic.Application.Services
{
    /// <summary>
    /// Servicio de Clínicas con la lógica de negocio
    /// </summary>
    public class ClinicaService : IClinicaService
    {
        private readonly IClinicaRepository _repository;
        private readonly IValidator<CreateClinicaDto> _createValidator;

        public ClinicaService(
            IClinicaRepository repository,
            IValidator<CreateClinicaDto> createValidator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
        }

        public async Task<IEnumerable<ClinicaResponseDto>> ObtenerTodasAsync()
        {
            var clinicas = await _repository.ObtenerTodasAsync();
            return clinicas.Select(MapToResponseDto);
        }

        public async Task<ClinicaResponseDto?> ObtenerPorIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a cero.", nameof(id));

            var clinica = await _repository.ObtenerPorIdAsync(id);
            return clinica != null ? MapToResponseDto(clinica) : null;
        }

        public async Task<ClinicaResponseDto> CrearAsync(CreateClinicaDto createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            // Validar con FluentValidation
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errores = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Errores de validación: {errores}");
            }

            // Mapear DTO a entidad
            var clinica = new Clinica
            {
                Nombre = createDto.Nombre.Trim(),
                Telefono = createDto.Telefono.Trim(),
                Direccion = createDto.Direccion.Trim(),
                Activa = true,
                FechaCreacion = DateTime.UtcNow
            };

            // Guardar en la base de datos
            var clinicaCreada = await _repository.CrearAsync(clinica);

            // Retornar DTO de respuesta
            return MapToResponseDto(clinicaCreada);
        }

        #region Métodos de Mapeo

        /// <summary>
        /// Mapea una entidad Clinica a un DTO de respuesta
        /// </summary>
        private static ClinicaResponseDto MapToResponseDto(Clinica clinica)
        {
            return new ClinicaResponseDto
            {
                Id = clinica.Id,
                Nombre = clinica.Nombre,
                Telefono = clinica.Telefono,
                Direccion = clinica.Direccion,
                Activa = clinica.Activa,
                FechaCreacion = clinica.FechaCreacion
            };
        }

        #endregion
    }
}
