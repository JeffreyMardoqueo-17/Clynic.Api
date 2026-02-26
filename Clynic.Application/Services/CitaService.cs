using Clynic.Application.DTOs.Citas;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Application.Interfaces.Services;
using Clynic.Domain.Models;
using Clynic.Domain.Models.Enums;
using FluentValidation;

namespace Clynic.Application.Services
{
    public class CitaService : ICitaService
    {
        private readonly ICitaRepository _citaRepository;
        private readonly IPacienteRepository _pacienteRepository;
        private readonly IServicioRepository _servicioRepository;
        private readonly IClinicaRepository _clinicaRepository;
        private readonly ISucursalRepository _sucursalRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IEmailService _emailService;
        private readonly IValidator<CreateCitaPublicaDto> _createCitaValidator;
        private readonly IValidator<CreateCitaInternaDto> _createCitaInternaValidator;
        private readonly IValidator<RegistrarConsultaMedicaDto> _registrarConsultaValidator;

        public CitaService(
            ICitaRepository citaRepository,
            IPacienteRepository pacienteRepository,
            IServicioRepository servicioRepository,
            IClinicaRepository clinicaRepository,
            ISucursalRepository sucursalRepository,
            IUsuarioRepository usuarioRepository,
            IEmailService emailService,
            IValidator<CreateCitaPublicaDto> createCitaValidator,
            IValidator<CreateCitaInternaDto> createCitaInternaValidator,
            IValidator<RegistrarConsultaMedicaDto> registrarConsultaValidator)
        {
            _citaRepository = citaRepository ?? throw new ArgumentNullException(nameof(citaRepository));
            _pacienteRepository = pacienteRepository ?? throw new ArgumentNullException(nameof(pacienteRepository));
            _servicioRepository = servicioRepository ?? throw new ArgumentNullException(nameof(servicioRepository));
            _clinicaRepository = clinicaRepository ?? throw new ArgumentNullException(nameof(clinicaRepository));
            _sucursalRepository = sucursalRepository ?? throw new ArgumentNullException(nameof(sucursalRepository));
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _createCitaValidator = createCitaValidator ?? throw new ArgumentNullException(nameof(createCitaValidator));
            _createCitaInternaValidator = createCitaInternaValidator ?? throw new ArgumentNullException(nameof(createCitaInternaValidator));
            _registrarConsultaValidator = registrarConsultaValidator ?? throw new ArgumentNullException(nameof(registrarConsultaValidator));
        }

        public async Task<CitaResponseDto> CrearPublicaAsync(CreateCitaPublicaDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var validationResult = await _createCitaValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errores = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Errores de validación: {errores}");
            }

            var clinicaExiste = await _clinicaRepository.ExisteAsync(dto.IdClinica);
            if (!clinicaExiste)
            {
                throw new KeyNotFoundException($"No se encontró la clínica con ID {dto.IdClinica}.");
            }

            var sucursal = await _sucursalRepository.ObtenerPorIdAsync(dto.IdSucursal);
            if (sucursal == null || !sucursal.Activa || sucursal.IdClinica != dto.IdClinica)
            {
                throw new ValidationException("La sucursal no pertenece a la clínica o está inactiva.");
            }

            var idsServicios = dto.IdsServicios.Distinct().ToList();
            var servicios = await _servicioRepository.ObtenerPorIdsClinicaAsync(dto.IdClinica, idsServicios);
            if (servicios.Count != idsServicios.Count)
            {
                throw new ValidationException("Uno o más servicios no existen o no pertenecen a la clínica.");
            }

            var paciente = await _pacienteRepository.ObtenerPorCorreoAsync(dto.IdClinica, dto.Correo);
            if (paciente == null)
            {
                paciente = await _pacienteRepository.CrearAsync(new Paciente
                {
                    IdClinica = dto.IdClinica,
                    Nombres = dto.Nombres.Trim(),
                    Apellidos = dto.Apellidos.Trim(),
                    Telefono = dto.Telefono.Trim(),
                    Correo = dto.Correo.Trim().ToLower(),
                    FechaRegistro = DateTime.UtcNow
                });
            }
            else
            {
                paciente.Nombres = dto.Nombres.Trim();
                paciente.Apellidos = dto.Apellidos.Trim();
                paciente.Telefono = dto.Telefono.Trim();
                await _pacienteRepository.ActualizarAsync(paciente);
            }

            var duracionTotal = servicios.Sum(s => s.DuracionMin);
            var subtotal = servicios.Sum(s => s.PrecioBase);
            var fechaFin = dto.FechaHoraInicioPlan.AddMinutes(duracionTotal <= 0 ? 30 : duracionTotal);

            var cita = new Cita
            {
                IdClinica = dto.IdClinica,
                IdSucursal = dto.IdSucursal,
                IdPaciente = paciente.Id,
                FechaHoraInicioPlan = dto.FechaHoraInicioPlan,
                FechaHoraFinPlan = fechaFin,
                Estado = EstadoCita.Pendiente,
                Notas = dto.Notas.Trim(),
                SubTotal = subtotal,
                TotalFinal = subtotal,
                CitaServicios = servicios.Select(s => new CitaServicio
                {
                    IdServicio = s.Id,
                    DuracionMin = s.DuracionMin,
                    Precio = s.PrecioBase
                }).ToList()
            };

            var creada = await _citaRepository.CrearAsync(cita);

            try
            {
                await _emailService.EnviarConfirmacionCitaAgendadaAsync(
                    paciente.Correo,
                    $"{paciente.Nombres} {paciente.Apellidos}".Trim(),
                    cita.Clinica?.Nombre ?? "Clínica",
                    sucursal.Nombre,
                    cita.FechaHoraInicioPlan,
                    cita.FechaHoraFinPlan,
                    servicios.Select(s => s.NombreServicio),
                    cita.Notas);
            }
            catch
            {
            }

            return MapToResponseDto(creada);
        }

        public async Task<CatalogoCitaPublicaDto> ObtenerCatalogoPublicoAsync(int idClinica)
        {
            if (idClinica <= 0)
            {
                throw new ArgumentException("El ID de la clínica debe ser mayor a cero.", nameof(idClinica));
            }

            var clinicaExiste = await _clinicaRepository.ExisteAsync(idClinica);
            if (!clinicaExiste)
            {
                throw new KeyNotFoundException($"No se encontró la clínica con ID {idClinica}.");
            }

            var sucursales = (await _sucursalRepository.ObtenerTodasAsync())
                .Where(s => s.Activa && s.IdClinica == idClinica)
                .OrderBy(s => s.Nombre)
                .Select(s => new CatalogoSucursalDto
                {
                    Id = s.Id,
                    Nombre = s.Nombre,
                    Direccion = s.Direccion
                })
                .ToArray();

            var servicios = (await _servicioRepository.ObtenerPorClinicaAsync(idClinica))
                .Where(s => s.Activo)
                .OrderBy(s => s.NombreServicio)
                .Select(s => new CatalogoServicioDto
                {
                    Id = s.Id,
                    NombreServicio = s.NombreServicio,
                    DuracionMin = s.DuracionMin,
                    PrecioBase = s.PrecioBase
                })
                .ToArray();

            return new CatalogoCitaPublicaDto
            {
                IdClinica = idClinica,
                Sucursales = sucursales,
                Servicios = servicios
            };
        }

        public async Task<CitaResponseDto> CrearInternaAsync(CreateCitaInternaDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var validationResult = await _createCitaInternaValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errores = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Errores de validación: {errores}");
            }

            var clinicaExiste = await _clinicaRepository.ExisteAsync(dto.IdClinica);
            if (!clinicaExiste)
            {
                throw new KeyNotFoundException($"No se encontró la clínica con ID {dto.IdClinica}.");
            }

            var sucursal = await _sucursalRepository.ObtenerPorIdAsync(dto.IdSucursal);
            if (sucursal == null || !sucursal.Activa || sucursal.IdClinica != dto.IdClinica)
            {
                throw new ValidationException("La sucursal no pertenece a la clínica o está inactiva.");
            }

            var paciente = await _pacienteRepository.ObtenerPorIdAsync(dto.IdPaciente)
                ?? throw new KeyNotFoundException($"No se encontró el paciente con ID {dto.IdPaciente}.");

            if (paciente.IdClinica != dto.IdClinica)
            {
                throw new ValidationException("El paciente no pertenece a la clínica especificada.");
            }

            if (dto.IdDoctor.HasValue)
            {
                var doctor = await _usuarioRepository.ObtenerPorIdAsync(dto.IdDoctor.Value);
                if (doctor == null || !doctor.Activo || doctor.IdClinica != dto.IdClinica || doctor.Rol != UsuarioRol.Doctor)
                {
                    throw new ValidationException("El doctor seleccionado no es válido para esta clínica.");
                }
            }

            var idsServicios = dto.IdsServicios.Distinct().ToList();
            var servicios = await _servicioRepository.ObtenerPorIdsClinicaAsync(dto.IdClinica, idsServicios);
            if (servicios.Count != idsServicios.Count)
            {
                throw new ValidationException("Uno o más servicios no existen o no pertenecen a la clínica.");
            }

            var duracionTotal = servicios.Sum(s => s.DuracionMin);
            var subtotal = servicios.Sum(s => s.PrecioBase);
            var fechaFin = dto.FechaHoraInicioPlan.AddMinutes(duracionTotal <= 0 ? 30 : duracionTotal);

            var cita = new Cita
            {
                IdClinica = dto.IdClinica,
                IdSucursal = dto.IdSucursal,
                IdPaciente = dto.IdPaciente,
                IdDoctor = dto.IdDoctor,
                FechaHoraInicioPlan = dto.FechaHoraInicioPlan,
                FechaHoraFinPlan = fechaFin,
                Estado = dto.EstadoInicial,
                Notas = dto.Notas.Trim(),
                SubTotal = subtotal,
                TotalFinal = subtotal,
                CitaServicios = servicios.Select(s => new CitaServicio
                {
                    IdServicio = s.Id,
                    DuracionMin = s.DuracionMin,
                    Precio = s.PrecioBase
                }).ToList()
            };

            var creada = await _citaRepository.CrearAsync(cita);
            return MapToResponseDto(creada);
        }

        public async Task<CitaResponseDto?> ObtenerPorIdAsync(int idCita)
        {
            if (idCita <= 0)
            {
                throw new ArgumentException("El ID de la cita debe ser mayor a cero.", nameof(idCita));
            }

            var cita = await _citaRepository.ObtenerPorIdAsync(idCita);
            return cita != null ? MapToResponseDto(cita) : null;
        }

        public async Task<IEnumerable<CitaResponseDto>> ObtenerPorClinicaAsync(
            int idClinica,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int? idSucursal = null,
            EstadoCita? estado = null)
        {
            if (idClinica <= 0)
            {
                throw new ArgumentException("El ID de la clínica debe ser mayor a cero.", nameof(idClinica));
            }

            var citas = await _citaRepository.ObtenerPorClinicaAsync(idClinica, fechaDesde, fechaHasta, idSucursal, estado);
            return citas.Select(MapToResponseDto);
        }

        public async Task<CitaResponseDto?> AsignarDoctorAsync(int idCita, AsignarDoctorCitaDto dto)
        {
            if (idCita <= 0)
            {
                throw new ArgumentException("El ID de la cita debe ser mayor a cero.", nameof(idCita));
            }

            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var cita = await _citaRepository.ObtenerPorIdAsync(idCita);
            if (cita == null)
            {
                return null;
            }

            if (!dto.IdDoctor.HasValue)
            {
                cita.IdDoctor = null;
            }
            else
            {
                var doctor = await _usuarioRepository.ObtenerPorIdAsync(dto.IdDoctor.Value);
                if (doctor == null || !doctor.Activo || doctor.IdClinica != cita.IdClinica || doctor.Rol != UsuarioRol.Doctor)
                {
                    throw new ValidationException("El doctor seleccionado no es válido para esta clínica.");
                }

                cita.IdDoctor = doctor.Id;
                if (cita.Estado == EstadoCita.Pendiente)
                {
                    cita.Estado = EstadoCita.Confirmada;
                }
            }

            var actualizada = await _citaRepository.ActualizarAsync(cita);
            return MapToResponseDto(actualizada);
        }

        public async Task<ConsultaMedicaResponseDto> RegistrarConsultaAsync(int idCita, int idDoctorEjecutor, RegistrarConsultaMedicaDto dto)
        {
            if (idCita <= 0)
            {
                throw new ArgumentException("El ID de la cita debe ser mayor a cero.", nameof(idCita));
            }

            if (idDoctorEjecutor <= 0)
            {
                throw new ArgumentException("El ID del doctor debe ser mayor a cero.", nameof(idDoctorEjecutor));
            }

            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var validationResult = await _registrarConsultaValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errores = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Errores de validación: {errores}");
            }

            var cita = await _citaRepository.ObtenerPorIdAsync(idCita)
                ?? throw new KeyNotFoundException($"No se encontró la cita con ID {idCita}.");

            var consultaExistente = await _citaRepository.ObtenerConsultaPorCitaAsync(idCita);
            if (consultaExistente != null)
            {
                throw new InvalidOperationException("La cita ya tiene una consulta médica registrada.");
            }

            var usuarioEjecutor = await _usuarioRepository.ObtenerPorIdAsync(idDoctorEjecutor)
                ?? throw new ValidationException("El usuario que registra la consulta no existe.");

            if (!usuarioEjecutor.Activo || usuarioEjecutor.IdClinica != cita.IdClinica)
            {
                throw new UnauthorizedAccessException("No tienes permisos para registrar esta consulta.");
            }

            if (usuarioEjecutor.Rol != UsuarioRol.Doctor && usuarioEjecutor.Rol != UsuarioRol.Admin)
            {
                throw new UnauthorizedAccessException("Solo un doctor o admin puede registrar una consulta.");
            }

            var idDoctorAtencion = cita.IdDoctor ?? (usuarioEjecutor.Rol == UsuarioRol.Doctor ? usuarioEjecutor.Id : null);

            var consulta = new ConsultaMedica
            {
                IdCita = cita.Id,
                IdClinica = cita.IdClinica,
                IdSucursal = cita.IdSucursal,
                IdPaciente = cita.IdPaciente,
                IdDoctor = idDoctorAtencion,
                Diagnostico = dto.Diagnostico.Trim(),
                Tratamiento = dto.Tratamiento.Trim(),
                Receta = dto.Receta.Trim(),
                ExamenesSolicitados = dto.ExamenesSolicitados.Trim(),
                NotasMedicas = dto.NotasMedicas.Trim(),
                FechaConsulta = dto.FechaConsulta ?? DateTime.UtcNow
            };

            var consultaCreada = await _citaRepository.CrearConsultaAsync(consulta);

            cita.Estado = EstadoCita.Completada;
            cita.FechaHoraInicioReal ??= cita.FechaHoraInicioPlan;
            cita.FechaHoraFinReal ??= DateTime.UtcNow;
            if (!cita.IdDoctor.HasValue && idDoctorAtencion.HasValue)
            {
                cita.IdDoctor = idDoctorAtencion;
            }

            await _citaRepository.ActualizarAsync(cita);

            return MapConsultaToResponseDto(consultaCreada);
        }

        private static CitaResponseDto MapToResponseDto(Cita cita)
        {
            var servicios = cita.CitaServicios
                .Select(cs => new CitaServicioDetalleDto
                {
                    IdServicio = cs.IdServicio,
                    NombreServicio = cs.Servicio?.NombreServicio ?? string.Empty,
                    DuracionMin = cs.DuracionMin,
                    Precio = cs.Precio
                })
                .ToArray();

            return new CitaResponseDto
            {
                Id = cita.Id,
                IdClinica = cita.IdClinica,
                IdSucursal = cita.IdSucursal,
                IdPaciente = cita.IdPaciente,
                IdDoctor = cita.IdDoctor,
                NombrePaciente = cita.Paciente != null ? $"{cita.Paciente.Nombres} {cita.Paciente.Apellidos}".Trim() : string.Empty,
                CorreoPaciente = cita.Paciente?.Correo ?? string.Empty,
                TelefonoPaciente = cita.Paciente?.Telefono ?? string.Empty,
                FechaHoraInicioPlan = cita.FechaHoraInicioPlan,
                FechaHoraFinPlan = cita.FechaHoraFinPlan,
                FechaHoraInicioReal = cita.FechaHoraInicioReal,
                FechaHoraFinReal = cita.FechaHoraFinReal,
                Estado = cita.Estado,
                Notas = cita.Notas,
                SubTotal = cita.SubTotal,
                TotalFinal = cita.TotalFinal,
                FechaCreacion = cita.FechaCreacion,
                Servicios = servicios,
                ConsultaMedica = cita.ConsultaMedica != null
                    ? MapConsultaToResponseDto(cita.ConsultaMedica)
                    : null
            };
        }

        private static ConsultaMedicaResponseDto MapConsultaToResponseDto(ConsultaMedica consulta)
        {
            return new ConsultaMedicaResponseDto
            {
                Id = consulta.Id,
                IdCita = consulta.IdCita,
                IdPaciente = consulta.IdPaciente,
                IdDoctor = consulta.IdDoctor,
                Diagnostico = consulta.Diagnostico,
                Tratamiento = consulta.Tratamiento,
                Receta = consulta.Receta,
                ExamenesSolicitados = consulta.ExamenesSolicitados,
                NotasMedicas = consulta.NotasMedicas,
                FechaConsulta = consulta.FechaConsulta
            };
        }
    }
}
