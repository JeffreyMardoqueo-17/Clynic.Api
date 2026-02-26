using Clynic.Application.DTOs.Citas;
using Clynic.Application.DTOs.Pacientes;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Application.Interfaces.Services;
using Clynic.Domain.Models;
using FluentValidation;

namespace Clynic.Application.Services
{
    public class PacienteService : IPacienteService
    {
        private readonly IPacienteRepository _pacienteRepository;
        private readonly IValidator<UpdatePacienteDto> _updatePacienteValidator;
        private readonly IValidator<UpdateHistorialClinicoDto> _updateHistorialValidator;

        public PacienteService(
            IPacienteRepository pacienteRepository,
            IValidator<UpdatePacienteDto> updatePacienteValidator,
            IValidator<UpdateHistorialClinicoDto> updateHistorialValidator)
        {
            _pacienteRepository = pacienteRepository ?? throw new ArgumentNullException(nameof(pacienteRepository));
            _updatePacienteValidator = updatePacienteValidator ?? throw new ArgumentNullException(nameof(updatePacienteValidator));
            _updateHistorialValidator = updateHistorialValidator ?? throw new ArgumentNullException(nameof(updateHistorialValidator));
        }

        public async Task<IEnumerable<PacienteResponseDto>> ObtenerPorClinicaAsync(int idClinica, string? busqueda = null)
        {
            if (idClinica <= 0)
            {
                throw new ArgumentException("El ID de la clínica debe ser mayor a cero.", nameof(idClinica));
            }

            var pacientes = await _pacienteRepository.ObtenerPorClinicaAsync(idClinica, busqueda);
            return pacientes.Select(MapToResponseDto);
        }

        public async Task<PacienteResponseDto?> ObtenerPorIdAsync(int idPaciente)
        {
            if (idPaciente <= 0)
            {
                throw new ArgumentException("El ID del paciente debe ser mayor a cero.", nameof(idPaciente));
            }

            var paciente = await _pacienteRepository.ObtenerPorIdAsync(idPaciente);
            return paciente != null ? MapToResponseDto(paciente) : null;
        }

        public async Task<PacienteResponseDto?> ActualizarAsync(int idPaciente, UpdatePacienteDto dto)
        {
            if (idPaciente <= 0)
            {
                throw new ArgumentException("El ID del paciente debe ser mayor a cero.", nameof(idPaciente));
            }

            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var validationResult = await _updatePacienteValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errores = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Errores de validación: {errores}");
            }

            var paciente = await _pacienteRepository.ObtenerPorIdAsync(idPaciente);
            if (paciente == null)
            {
                return null;
            }

            paciente.Nombres = dto.Nombres.Trim();
            paciente.Apellidos = dto.Apellidos.Trim();
            paciente.Telefono = dto.Telefono.Trim();
            paciente.Correo = dto.Correo.Trim().ToLower();
            paciente.FechaNacimiento = dto.FechaNacimiento?.Date;

            var actualizado = await _pacienteRepository.ActualizarAsync(paciente);
            return MapToResponseDto(actualizado);
        }

        public async Task<HistorialClinicoResponseDto> GuardarHistorialAsync(int idPaciente, UpdateHistorialClinicoDto dto)
        {
            if (idPaciente <= 0)
            {
                throw new ArgumentException("El ID del paciente debe ser mayor a cero.", nameof(idPaciente));
            }

            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var validationResult = await _updateHistorialValidator.ValidateAsync(dto);
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

            var guardado = await _pacienteRepository.GuardarHistorialAsync(historial);
            return MapHistorialToResponseDto(guardado);
        }

        private static PacienteResponseDto MapToResponseDto(Paciente paciente)
        {
            var consultasRecientes = paciente.ConsultasMedicas
                .OrderByDescending(c => c.FechaConsulta)
                .Take(10)
                .Select(c => new ConsultaMedicaResponseDto
                {
                    Id = c.Id,
                    IdCita = c.IdCita,
                    IdPaciente = c.IdPaciente,
                    IdDoctor = c.IdDoctor,
                    Diagnostico = c.Diagnostico,
                    Tratamiento = c.Tratamiento,
                    Receta = c.Receta,
                    ExamenesSolicitados = c.ExamenesSolicitados,
                    NotasMedicas = c.NotasMedicas,
                    FechaConsulta = c.FechaConsulta
                })
                .ToArray();

            return new PacienteResponseDto
            {
                Id = paciente.Id,
                IdClinica = paciente.IdClinica,
                Nombres = paciente.Nombres,
                Apellidos = paciente.Apellidos,
                Telefono = paciente.Telefono,
                Correo = paciente.Correo,
                FechaNacimiento = paciente.FechaNacimiento,
                FechaRegistro = paciente.FechaRegistro,
                HistorialClinico = paciente.HistorialClinico != null
                    ? MapHistorialToResponseDto(paciente.HistorialClinico)
                    : null,
                ConsultasRecientes = consultasRecientes
            };
        }

        private static HistorialClinicoResponseDto MapHistorialToResponseDto(HistorialClinico historial)
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
