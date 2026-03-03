using Clynic.Application.DTOs.HistorialesClinicos;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Application.Interfaces.Services;
using Clynic.Domain.Models;
using FluentValidation;

namespace Clynic.Application.Services
{
    public class HistorialClinicoService : IHistorialClinicoService
    {
        private readonly IHistorialClinicoRepository _historialClinicoRepository;
        private readonly IPacienteRepository _pacienteRepository;
        private readonly IValidator<UpsertHistorialClinicoDto> _upsertHistorialValidator;

        public HistorialClinicoService(
            IHistorialClinicoRepository historialClinicoRepository,
            IPacienteRepository pacienteRepository,
            IValidator<UpsertHistorialClinicoDto> upsertHistorialValidator)
        {
            _historialClinicoRepository = historialClinicoRepository ?? throw new ArgumentNullException(nameof(historialClinicoRepository));
            _pacienteRepository = pacienteRepository ?? throw new ArgumentNullException(nameof(pacienteRepository));
            _upsertHistorialValidator = upsertHistorialValidator ?? throw new ArgumentNullException(nameof(upsertHistorialValidator));
        }

        public async Task<HistorialClinicoResponseDto?> ObtenerPorPacienteAsync(int idPaciente)
        {
            if (idPaciente <= 0)
            {
                throw new ArgumentException("El ID del paciente debe ser mayor a cero.", nameof(idPaciente));
            }

            var historial = await _historialClinicoRepository.ObtenerPorPacienteAsync(idPaciente);
            return historial != null ? MapToResponseDto(historial) : null;
        }

        public async Task<HistorialClinicoResponseDto> GuardarAsync(int idPaciente, UpsertHistorialClinicoDto dto)
        {
            if (idPaciente <= 0)
            {
                throw new ArgumentException("El ID del paciente debe ser mayor a cero.", nameof(idPaciente));
            }

            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var validationResult = await _upsertHistorialValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errores = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Errores de validación: {errores}");
            }

            var paciente = await _pacienteRepository.ObtenerPorIdAsync(idPaciente)
                ?? throw new KeyNotFoundException($"No se encontró el paciente con ID {idPaciente}.");

            var historial = new HistorialClinico
            {
                IdPaciente = paciente.Id,
                EnfermedadesPrevias = dto.EnfermedadesPrevias.Trim(),
                MedicamentosActuales = dto.MedicamentosActuales.Trim(),
                Alergias = dto.Alergias.Trim(),
                AntecedentesFamiliares = dto.AntecedentesFamiliares.Trim(),
                Observaciones = dto.Observaciones.Trim()
            };

            var guardado = await _historialClinicoRepository.GuardarAsync(historial);
            return MapToResponseDto(guardado);
        }

        private static HistorialClinicoResponseDto MapToResponseDto(HistorialClinico historial)
        {
            return new HistorialClinicoResponseDto
            {
                Id = historial.Id,
                IdPaciente = historial.IdPaciente,
                EnfermedadesPrevias = historial.EnfermedadesPrevias,
                MedicamentosActuales = historial.MedicamentosActuales,
                Alergias = historial.Alergias,
                AntecedentesFamiliares = historial.AntecedentesFamiliares,
                Observaciones = historial.Observaciones,
                FechaCreacion = historial.FechaCreacion,
                FechaActualizacion = historial.FechaActualizacion
            };
        }
    }
}
