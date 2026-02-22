using Clynic.Application.DTOs.HorariosSucursal;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Application.Interfaces.Services;
using Clynic.Domain.Models;
using FluentValidation;

namespace Clynic.Application.Services
{
    /// <summary>
    /// Servicio de Horarios de Sucursal con la logica de negocio
    /// </summary>
    public class HorarioSucursalService : IHorarioSucursalService
    {
        private readonly IHorarioSucursalRepository _repository;
        private readonly IValidator<CreateHorarioSucursalDto> _createValidator;

        public HorarioSucursalService(
            IHorarioSucursalRepository repository,
            IValidator<CreateHorarioSucursalDto> createValidator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
        }

        public async Task<IEnumerable<HorarioSucursalResponseDto>> ObtenerTodosAsync()
        {
            var horarios = await _repository.ObtenerTodosAsync();
            return horarios.Select(MapToResponseDto);
        }

        public async Task<HorarioSucursalResponseDto?> ObtenerPorIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a cero.", nameof(id));

            var horario = await _repository.ObtenerPorIdAsync(id);
            return horario != null ? MapToResponseDto(horario) : null;
        }

        public async Task<IEnumerable<HorarioSucursalResponseDto>> ObtenerPorSucursalAsync(int idSucursal)
        {
            if (idSucursal <= 0)
                throw new ArgumentException("El ID de la sucursal debe ser mayor a cero.", nameof(idSucursal));

            var horarios = await _repository.ObtenerPorSucursalAsync(idSucursal);
            return horarios.Select(MapToResponseDto);
        }

        public async Task<HorarioSucursalResponseDto> CrearAsync(CreateHorarioSucursalDto createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errores = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Errores de validacion: {errores}");
            }

            var horario = new HorarioSucursal
            {
                IdSucursal = createDto.IdSucursal,
                DiaSemana = createDto.DiaSemana,
                HoraInicio = createDto.HoraInicio,
                HoraFin = createDto.HoraFin
            };

            var horarioCreado = await _repository.CrearAsync(horario);
            return MapToResponseDto(horarioCreado);
        }

        private static HorarioSucursalResponseDto MapToResponseDto(HorarioSucursal horario)
        {
            return new HorarioSucursalResponseDto
            {
                Id = horario.Id,
                IdSucursal = horario.IdSucursal,
                DiaSemana = horario.DiaSemana,
                HoraInicio = horario.HoraInicio,
                HoraFin = horario.HoraFin
            };
        }
    }
}
