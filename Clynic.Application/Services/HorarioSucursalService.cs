using Clynic.Application.DTOs.HorariosSucursal;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Application.Interfaces.Services;
using Clynic.Application.Rules;
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
        private readonly IAsuetoSucursalRepository _asuetoRepository;
        private readonly HorarioSucursalRules _rules;
        private readonly IValidator<CreateHorarioSucursalDto> _createValidator;
        private readonly IValidator<UpdateHorarioSucursalDto> _updateValidator;
        private readonly IValidator<CreateAsuetoSucursalDto> _createAsuetoValidator;

        public HorarioSucursalService(
            IHorarioSucursalRepository repository,
            IAsuetoSucursalRepository asuetoRepository,
            HorarioSucursalRules rules,
            IValidator<CreateHorarioSucursalDto> createValidator,
            IValidator<UpdateHorarioSucursalDto> updateValidator,
            IValidator<CreateAsuetoSucursalDto> createAsuetoValidator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _asuetoRepository = asuetoRepository ?? throw new ArgumentNullException(nameof(asuetoRepository));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
            _createAsuetoValidator = createAsuetoValidator ?? throw new ArgumentNullException(nameof(createAsuetoValidator));
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

        public async Task<HorarioSucursalResponseDto> ActualizarAsync(int id, UpdateHorarioSucursalDto updateDto)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a cero.", nameof(id));

            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));

            var horarioExistente = await _repository.ObtenerPorIdAsync(id);
            if (horarioExistente == null)
                throw new KeyNotFoundException($"No se encontro el horario con ID {id}");

            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                var errores = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Errores de validacion: {errores}");
            }

            var noCruza = await _rules.NoExisteCruceHorarioAsync(
                horarioExistente.IdSucursal,
                updateDto.DiaSemana,
                updateDto.HoraInicio,
                updateDto.HoraFin,
                id);

            if (!noCruza)
            {
                throw new ValidationException("Ya existe un horario que se cruza en la misma sucursal y dia.");
            }

            horarioExistente.DiaSemana = updateDto.DiaSemana;
            horarioExistente.HoraInicio = updateDto.HoraInicio;
            horarioExistente.HoraFin = updateDto.HoraFin;

            var horarioActualizado = await _repository.ActualizarAsync(horarioExistente);
            return MapToResponseDto(horarioActualizado);
        }

        public async Task<bool> EliminarAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a cero.", nameof(id));

            return await _repository.EliminarAsync(id);
        }

        public async Task<IEnumerable<AsuetoSucursalResponseDto>> ObtenerAsuetosPorSucursalAsync(int idSucursal)
        {
            if (idSucursal <= 0)
                throw new ArgumentException("El ID de la sucursal debe ser mayor a cero.", nameof(idSucursal));

            var asuetos = await _asuetoRepository.ObtenerPorSucursalAsync(idSucursal);
            return asuetos.Select(MapAsuetoToResponseDto);
        }

        public async Task<AsuetoSucursalResponseDto> CrearAsuetoAsync(CreateAsuetoSucursalDto createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            var validationResult = await _createAsuetoValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errores = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Errores de validacion: {errores}");
            }

            var asueto = new AsuetoSucursal
            {
                IdSucursal = createDto.IdSucursal,
                Fecha = createDto.Fecha,
                Motivo = string.IsNullOrWhiteSpace(createDto.Motivo) ? null : createDto.Motivo.Trim()
            };

            var asuetoCreado = await _asuetoRepository.CrearAsync(asueto);
            return MapAsuetoToResponseDto(asuetoCreado);
        }

        public async Task<bool> EliminarAsuetoAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a cero.", nameof(id));

            return await _asuetoRepository.EliminarAsync(id);
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

        private static AsuetoSucursalResponseDto MapAsuetoToResponseDto(AsuetoSucursal asueto)
        {
            return new AsuetoSucursalResponseDto
            {
                Id = asueto.Id,
                IdSucursal = asueto.IdSucursal,
                Fecha = asueto.Fecha,
                Motivo = asueto.Motivo,
                FechaCreacion = asueto.FechaCreacion
            };
        }
    }
}
