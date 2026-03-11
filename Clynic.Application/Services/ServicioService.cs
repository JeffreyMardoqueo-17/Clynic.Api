using Clynic.Application.DTOs.Servicios;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Application.Interfaces.Services;
using Clynic.Application.Rules;
using Clynic.Domain.Models;
using Clynic.Domain.Models.Enums;
using FluentValidation;

namespace Clynic.Application.Services
{
    public class ServicioService : IServicioService
    {
        private readonly IServicioRepository _repository;
        private readonly IEspecialidadRepository _especialidadRepository;
        private readonly ICitaRepository _citaRepository;
        private readonly ServicioRules _rules;
        private readonly IValidator<CreateServicioDto> _createValidator;
        private readonly IValidator<UpdateServicioDto> _updateValidator;

        public ServicioService(
            IServicioRepository repository,
            IEspecialidadRepository especialidadRepository,
            ICitaRepository citaRepository,
            ServicioRules rules,
            IValidator<CreateServicioDto> createValidator,
            IValidator<UpdateServicioDto> updateValidator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _especialidadRepository = especialidadRepository ?? throw new ArgumentNullException(nameof(especialidadRepository));
            _citaRepository = citaRepository ?? throw new ArgumentNullException(nameof(citaRepository));
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

            var especialidadExiste = await _especialidadRepository.ExisteActivaAsync(createDto.IdClinica, createDto.IdEspecialidad);
            if (!especialidadExiste)
            {
                throw new ValidationException("La especialidad indicada no existe o está inactiva para la clínica.");
            }

            var servicio = new Servicio
            {
                IdClinica = createDto.IdClinica,
                IdEspecialidad = createDto.IdEspecialidad,
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

            if (updateDto.IdEspecialidad.HasValue)
            {
                var especialidadExiste = await _especialidadRepository.ExisteActivaAsync(servicio.IdClinica, updateDto.IdEspecialidad.Value);
                if (!especialidadExiste)
                {
                    throw new ValidationException("La especialidad indicada no existe o está inactiva para la clínica.");
                }

                servicio.IdEspecialidad = updateDto.IdEspecialidad.Value;
            }

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

        public async Task<IReadOnlyCollection<CapacidadEspecialidadDiaDto>> ObtenerCapacidadPorEspecialidadAsync(
            int idClinica,
            DateTime fechaDesde,
            DateTime fechaHasta,
            int? idSucursal = null,
            int horasLaborablesDia = 8,
            int minutosAlmuerzoDia = 60)
        {
            if (idClinica <= 0)
                throw new ArgumentException("El ID de la clínica debe ser mayor a cero.", nameof(idClinica));

            if (fechaHasta.Date < fechaDesde.Date)
                throw new ArgumentException("La fecha final no puede ser menor que la fecha inicial.", nameof(fechaHasta));

            var minutosLaborablesDia = Math.Max(60, (horasLaborablesDia * 60) - Math.Max(0, minutosAlmuerzoDia));
            var inicio = fechaDesde.Date;
            var fin = fechaHasta.Date.AddDays(1).AddTicks(-1);

            var citas = await _citaRepository.ObtenerPorClinicaAsync(idClinica, inicio, fin, idSucursal, null);
            var citasVigentes = citas.Where(c => c.Estado != EstadoCita.Cancelada);

            var result = citasVigentes
                .GroupBy(c => new { Fecha = c.FechaHoraInicioPlan.Date, c.IdEspecialidad, Nombre = c.Especialidad?.Nombre ?? $"Especialidad #{c.IdEspecialidad}" })
                .Select(group =>
                {
                    var totalCitas = group.Count();
                    var totalMinutos = group.Sum(c => c.CitaServicios.Sum(cs => cs.DuracionMin > 0 ? cs.DuracionMin : (cs.Servicio?.DuracionMin ?? 0)));
                    var promedioDuracion = totalCitas > 0
                        ? Math.Round((decimal)totalMinutos / totalCitas, 2)
                        : 0m;

                    var citasPosibles = promedioDuracion > 0
                        ? (int)Math.Floor(minutosLaborablesDia / promedioDuracion)
                        : 0;

                    var citasDisponibles = Math.Max(0, citasPosibles - totalCitas);
                    var minutosDisponibles = Math.Max(0, minutosLaborablesDia - totalMinutos);
                    var saturacion = citasPosibles > 0
                        ? Math.Round((decimal)totalCitas * 100 / citasPosibles, 2)
                        : 0m;

                    return new CapacidadEspecialidadDiaDto
                    {
                        Fecha = group.Key.Fecha,
                        IdEspecialidad = group.Key.IdEspecialidad,
                        NombreEspecialidad = group.Key.Nombre,
                        TotalCitasAgendadas = totalCitas,
                        TotalMinutosAgendados = totalMinutos,
                        DuracionPromedioCitaMin = promedioDuracion,
                        MinutosLaborablesDia = minutosLaborablesDia,
                        MinutosDisponiblesDia = minutosDisponibles,
                        CitasPosiblesDia = citasPosibles,
                        CitasDisponiblesDia = citasDisponibles,
                        SaturacionPct = saturacion,
                    };
                })
                .OrderBy(x => x.Fecha)
                .ThenBy(x => x.NombreEspecialidad)
                .ToArray();

            return result;
        }

        private static ServicioResponseDto MapToResponseDto(Servicio servicio)
        {
            return new ServicioResponseDto
            {
                Id = servicio.Id,
                IdClinica = servicio.IdClinica,
                IdEspecialidad = servicio.IdEspecialidad,
                NombreEspecialidad = servicio.Especialidad?.Nombre ?? string.Empty,
                NombreServicio = servicio.NombreServicio,
                DuracionMin = servicio.DuracionMin,
                PrecioBase = servicio.PrecioBase,
                Activo = servicio.Activo
            };
        }
    }
}
