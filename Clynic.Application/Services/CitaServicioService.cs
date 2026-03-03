using Clynic.Application.DTOs.CitaServicios;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Application.Interfaces.Services;
using Clynic.Domain.Models;
using Clynic.Domain.Models.Enums;
using FluentValidation;

namespace Clynic.Application.Services
{
    public class CitaServicioService : ICitaServicioService
    {
        private readonly ICitaServicioRepository _citaServicioRepository;
        private readonly ICitaRepository _citaRepository;
        private readonly IServicioRepository _servicioRepository;
        private readonly IValidator<CreateCitaServicioDto> _createCitaServicioValidator;

        public CitaServicioService(
            ICitaServicioRepository citaServicioRepository,
            ICitaRepository citaRepository,
            IServicioRepository servicioRepository,
            IValidator<CreateCitaServicioDto> createCitaServicioValidator)
        {
            _citaServicioRepository = citaServicioRepository ?? throw new ArgumentNullException(nameof(citaServicioRepository));
            _citaRepository = citaRepository ?? throw new ArgumentNullException(nameof(citaRepository));
            _servicioRepository = servicioRepository ?? throw new ArgumentNullException(nameof(servicioRepository));
            _createCitaServicioValidator = createCitaServicioValidator ?? throw new ArgumentNullException(nameof(createCitaServicioValidator));
        }

        public async Task<IReadOnlyCollection<CitaServicioResponseDto>> ObtenerPorCitaAsync(int idCita)
        {
            if (idCita <= 0)
            {
                throw new ArgumentException("El ID de la cita debe ser mayor a cero.", nameof(idCita));
            }

            var citaServicios = await _citaServicioRepository.ObtenerPorCitaAsync(idCita);
            return citaServicios
                .Select(MapToResponseDto)
                .ToArray();
        }

        public async Task<CitaServicioResponseDto?> ObtenerPorIdAsync(int idCitaServicio)
        {
            if (idCitaServicio <= 0)
            {
                throw new ArgumentException("El ID del detalle de cita debe ser mayor a cero.", nameof(idCitaServicio));
            }

            var citaServicio = await _citaServicioRepository.ObtenerPorIdAsync(idCitaServicio);
            return citaServicio != null ? MapToResponseDto(citaServicio) : null;
        }

        public async Task<CitaServicioResponseDto> CrearAsync(CreateCitaServicioDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var validationResult = await _createCitaServicioValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errores = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Errores de validación: {errores}");
            }

            var cita = await _citaRepository.ObtenerPorIdAsync(dto.IdCita)
                ?? throw new KeyNotFoundException($"No se encontró la cita con ID {dto.IdCita}.");

            if (cita.Estado is EstadoCita.Cancelada or EstadoCita.Completada)
            {
                throw new InvalidOperationException("No se pueden agregar servicios a una cita cancelada o completada.");
            }

            var servicio = await _servicioRepository.ObtenerPorIdAsync(dto.IdServicio, incluirInactivos: true)
                ?? throw new KeyNotFoundException($"No se encontró el servicio con ID {dto.IdServicio}.");

            if (!servicio.Activo)
            {
                throw new ValidationException("No se puede agregar un servicio inactivo a la cita.");
            }

            if (servicio.IdClinica != cita.IdClinica)
            {
                throw new ValidationException("El servicio no pertenece a la clínica de la cita.");
            }

            var citaServiciosActuales = await _citaServicioRepository.ObtenerPorCitaAsync(cita.Id);
            if (citaServiciosActuales.Any(cs => cs.IdServicio == dto.IdServicio))
            {
                throw new ValidationException("El servicio ya está agregado en esta cita.");
            }

            var citaServicio = new CitaServicio
            {
                IdCita = cita.Id,
                IdServicio = servicio.Id,
                DuracionMin = dto.DuracionMin ?? servicio.DuracionMin,
                Precio = dto.Precio ?? servicio.PrecioBase
            };

            var creado = await _citaServicioRepository.CrearAsync(citaServicio);
            await RecalcularTotalesYCronogramaAsync(cita.Id);

            var actualizado = await _citaServicioRepository.ObtenerPorIdAsync(creado.Id)
                ?? throw new InvalidOperationException("No se pudo recuperar el detalle de cita creado.");

            return MapToResponseDto(actualizado);
        }

        public async Task<bool> EliminarAsync(int idCitaServicio)
        {
            if (idCitaServicio <= 0)
            {
                throw new ArgumentException("El ID del detalle de cita debe ser mayor a cero.", nameof(idCitaServicio));
            }

            var existente = await _citaServicioRepository.ObtenerPorIdAsync(idCitaServicio);
            if (existente == null)
            {
                return false;
            }

            if (existente.Cita == null)
            {
                throw new InvalidOperationException("No se pudo resolver la cita asociada al detalle.");
            }

            if (existente.Cita.Estado is EstadoCita.Cancelada or EstadoCita.Completada)
            {
                throw new InvalidOperationException("No se pueden remover servicios de una cita cancelada o completada.");
            }

            var eliminado = await _citaServicioRepository.EliminarAsync(idCitaServicio);
            if (!eliminado)
            {
                return false;
            }

            await RecalcularTotalesYCronogramaAsync(existente.IdCita);
            return true;
        }

        private async Task RecalcularTotalesYCronogramaAsync(int idCita)
        {
            var cita = await _citaRepository.ObtenerPorIdAsync(idCita)
                ?? throw new KeyNotFoundException($"No se encontró la cita con ID {idCita}.");

            var detalles = await _citaServicioRepository.ObtenerPorCitaAsync(idCita);
            var subtotal = detalles.Sum(cs => cs.Precio);
            var duracionTotal = detalles.Sum(cs => cs.DuracionMin);
            var minutos = duracionTotal > 0 ? duracionTotal : 30;

            cita.SubTotal = subtotal;
            cita.TotalFinal = subtotal;
            cita.FechaHoraFinPlan = cita.FechaHoraInicioPlan.AddMinutes(minutos);

            await _citaRepository.ActualizarAsync(cita);
        }

        private static CitaServicioResponseDto MapToResponseDto(CitaServicio citaServicio)
        {
            return new CitaServicioResponseDto
            {
                Id = citaServicio.Id,
                IdCita = citaServicio.IdCita,
                IdServicio = citaServicio.IdServicio,
                NombreServicio = citaServicio.Servicio?.NombreServicio ?? string.Empty,
                DuracionMin = citaServicio.DuracionMin,
                Precio = citaServicio.Precio,
                FechaHoraInicioPlanCita = citaServicio.Cita?.FechaHoraInicioPlan ?? DateTime.MinValue,
                FechaHoraFinPlanCita = citaServicio.Cita?.FechaHoraFinPlan ?? DateTime.MinValue,
                SubTotalCita = citaServicio.Cita?.SubTotal ?? 0,
                TotalFinalCita = citaServicio.Cita?.TotalFinal ?? 0
            };
        }
    }
}
