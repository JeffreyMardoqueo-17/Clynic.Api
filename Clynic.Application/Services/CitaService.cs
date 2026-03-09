using Clynic.Application.DTOs.Citas;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Application.Interfaces.Services;
using Clynic.Domain.Models;
using Clynic.Domain.Models.Enums;
using FluentValidation;
using System.Data;

namespace Clynic.Application.Services
{
    public class CitaService : ICitaService
    {
        private readonly ICitaRepository _citaRepository;
        private readonly IPacienteRepository _pacienteRepository;
        private readonly IServicioRepository _servicioRepository;
        private readonly IClinicaRepository _clinicaRepository;
        private readonly ISucursalRepository _sucursalRepository;
        private readonly IHorarioSucursalRepository _horarioSucursalRepository;
        private readonly ISucursalEspecialidadRepository _sucursalEspecialidadRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IValidator<CreateCitaPublicaDto> _createCitaValidator;
        private readonly IValidator<CreateCitaInternaDto> _createCitaInternaValidator;
        private readonly IValidator<RegistrarConsultaMedicaDto> _registrarConsultaValidator;
        private readonly IDoctorNotificationService _doctorNotificationService;

        public CitaService(
            ICitaRepository citaRepository,
            IPacienteRepository pacienteRepository,
            IServicioRepository servicioRepository,
            IClinicaRepository clinicaRepository,
            ISucursalRepository sucursalRepository,
            IHorarioSucursalRepository horarioSucursalRepository,
            ISucursalEspecialidadRepository sucursalEspecialidadRepository,
            IUsuarioRepository usuarioRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IValidator<CreateCitaPublicaDto> createCitaValidator,
            IValidator<CreateCitaInternaDto> createCitaInternaValidator,
            IValidator<RegistrarConsultaMedicaDto> registrarConsultaValidator,
            IDoctorNotificationService doctorNotificationService)
        {
            _citaRepository = citaRepository ?? throw new ArgumentNullException(nameof(citaRepository));
            _pacienteRepository = pacienteRepository ?? throw new ArgumentNullException(nameof(pacienteRepository));
            _servicioRepository = servicioRepository ?? throw new ArgumentNullException(nameof(servicioRepository));
            _clinicaRepository = clinicaRepository ?? throw new ArgumentNullException(nameof(clinicaRepository));
            _sucursalRepository = sucursalRepository ?? throw new ArgumentNullException(nameof(sucursalRepository));
            _horarioSucursalRepository = horarioSucursalRepository ?? throw new ArgumentNullException(nameof(horarioSucursalRepository));
            _sucursalEspecialidadRepository = sucursalEspecialidadRepository ?? throw new ArgumentNullException(nameof(sucursalEspecialidadRepository));
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _createCitaValidator = createCitaValidator ?? throw new ArgumentNullException(nameof(createCitaValidator));
            _createCitaInternaValidator = createCitaInternaValidator ?? throw new ArgumentNullException(nameof(createCitaInternaValidator));
            _registrarConsultaValidator = registrarConsultaValidator ?? throw new ArgumentNullException(nameof(registrarConsultaValidator));
            _doctorNotificationService = doctorNotificationService ?? throw new ArgumentNullException(nameof(doctorNotificationService));
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

            var creada = await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var paciente = await _pacienteRepository.ObtenerPorCorreoAsync(dto.IdClinica, dto.Correo);
                if (paciente == null)
                {
                    paciente = await _pacienteRepository.CrearAsync(new Paciente
                    {
                        IdClinica = dto.IdClinica,
                        IdEspecialidad = dto.IdEspecialidad,
                        Nombres = dto.Nombres.Trim(),
                        Apellidos = dto.Apellidos.Trim(),
                        Telefono = dto.Telefono.Trim(),
                        Correo = dto.Correo.Trim().ToLower(),
                        FechaRegistro = DateTime.UtcNow
                    });
                }
                else
                {
                    paciente.IdEspecialidad = dto.IdEspecialidad;
                    paciente.Nombres = dto.Nombres.Trim();
                    paciente.Apellidos = dto.Apellidos.Trim();
                    paciente.Telefono = dto.Telefono.Trim();
                    await _pacienteRepository.ActualizarAsync(paciente);
                }

                var duracionTotal = CalcularDuracionTotalMin(servicios.Select(s => s.DuracionMin));
                var subtotal = servicios.Sum(s => s.PrecioBase);
                var fechaFin = dto.FechaHoraInicioPlan.AddMinutes(duracionTotal);

                await ValidarCapacidadSucursalAsync(
                    dto.IdClinica,
                    dto.IdSucursal,
                    dto.IdEspecialidad,
                    dto.FechaHoraInicioPlan,
                    duracionTotal);

                var cita = new Cita
                {
                    IdClinica = dto.IdClinica,
                    IdSucursal = dto.IdSucursal,
                    IdPaciente = paciente.Id,
                    IdEspecialidad = dto.IdEspecialidad,
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

                return await _citaRepository.CrearAsync(cita);
            }, IsolationLevel.Serializable);

            try
            {
                var correoPaciente = creada.Paciente?.Correo ?? dto.Correo.Trim().ToLower();
                var nombrePaciente = creada.Paciente != null
                    ? $"{creada.Paciente.Nombres} {creada.Paciente.Apellidos}".Trim()
                    : $"{dto.Nombres} {dto.Apellidos}".Trim();

                await _emailService.EnviarConfirmacionCitaAgendadaAsync(
                    correoPaciente,
                    nombrePaciente,
                    "Clínica",
                    sucursal.Nombre,
                    creada.FechaHoraInicioPlan,
                    creada.FechaHoraFinPlan,
                    servicios.Select(s => s.NombreServicio),
                    creada.Notas);
            }
            catch
            {
            }

            return MapToResponseDto(creada);
        }

        public async Task<HorariosDisponiblesCitaDto> ObtenerHorariosDisponiblesPublicoAsync(
            int idClinica,
            int idSucursal,
            DateTime fecha,
            int idEspecialidad,
            IEnumerable<int> idsServicios,
            int intervaloMin = 30)
        {
            if (idClinica <= 0)
            {
                throw new ArgumentException("El ID de clínica debe ser mayor a cero.", nameof(idClinica));
            }

            if (idSucursal <= 0)
            {
                throw new ArgumentException("El ID de sucursal debe ser mayor a cero.", nameof(idSucursal));
            }

            if (idEspecialidad <= 0)
            {
                throw new ArgumentException("El ID de especialidad debe ser mayor a cero.", nameof(idEspecialidad));
            }

            var intervalo = intervaloMin <= 0 ? 30 : intervaloMin;
            var ids = (idsServicios ?? Enumerable.Empty<int>()).Distinct().ToList();
            if (ids.Count == 0)
            {
                throw new ValidationException("Debes seleccionar al menos un servicio para calcular horarios disponibles.");
            }

            var clinicaExiste = await _clinicaRepository.ExisteAsync(idClinica);
            if (!clinicaExiste)
            {
                throw new KeyNotFoundException($"No se encontró la clínica con ID {idClinica}.");
            }

            var sucursal = await _sucursalRepository.ObtenerPorIdAsync(idSucursal);
            if (sucursal == null || !sucursal.Activa || sucursal.IdClinica != idClinica)
            {
                throw new ValidationException("La sucursal no pertenece a la clínica o está inactiva.");
            }

            var servicios = await _servicioRepository.ObtenerPorIdsClinicaAsync(idClinica, ids);
            if (servicios.Count != ids.Count)
            {
                throw new ValidationException("Uno o más servicios no existen o no pertenecen a la clínica.");
            }

            var duracionEstimadaMin = CalcularDuracionTotalMin(servicios.Select(s => s.DuracionMin));
            var especialidadSucursal = await _sucursalEspecialidadRepository.ObtenerConfiguracionActivaAsync(idSucursal, idEspecialidad);
            if (especialidadSucursal == null)
            {
                return new HorariosDisponiblesCitaDto
                {
                    IdClinica = idClinica,
                    IdSucursal = idSucursal,
                    IdEspecialidad = idEspecialidad,
                    Fecha = fecha.Date,
                    DuracionEstimadaMin = duracionEstimadaMin,
                    IntervaloMin = intervalo,
                    CitasMaximasPorDiaEspecialidad = 0,
                    CitasOcupadasDiaEspecialidad = 0,
                    Horarios = Array.Empty<HorarioDisponibleItemDto>()
                };
            }

            var totalDoctoresEspecialidad = await _usuarioRepository.ContarProfesionalesActivosPorEspecialidadAsync(
                idClinica,
                idSucursal,
                idEspecialidad);

            if (totalDoctoresEspecialidad <= 0)
            {
                return new HorariosDisponiblesCitaDto
                {
                    IdClinica = idClinica,
                    IdSucursal = idSucursal,
                    IdEspecialidad = idEspecialidad,
                    Fecha = fecha.Date,
                    DuracionEstimadaMin = duracionEstimadaMin,
                    IntervaloMin = intervalo,
                    CitasMaximasPorDiaEspecialidad = 0,
                    CitasOcupadasDiaEspecialidad = 0,
                    Horarios = Array.Empty<HorarioDisponibleItemDto>()
                };
            }

            var capacidadMaximaDia = await CalcularCapacidadDiariaEspecialidadAsync(
                idClinica,
                idSucursal,
                idEspecialidad,
                fecha.Date,
                duracionEstimadaMin);

            var citasOcupadasEspecialidadDia = await _citaRepository.ContarCitasActivasEspecialidadDiaAsync(
                idClinica,
                idSucursal,
                idEspecialidad,
                fecha.Date);

            if (capacidadMaximaDia <= 0 || citasOcupadasEspecialidadDia >= capacidadMaximaDia)
            {
                return new HorariosDisponiblesCitaDto
                {
                    IdClinica = idClinica,
                    IdSucursal = idSucursal,
                    IdEspecialidad = idEspecialidad,
                    Fecha = fecha.Date,
                    DuracionEstimadaMin = duracionEstimadaMin,
                    IntervaloMin = intervalo,
                    CitasMaximasPorDiaEspecialidad = capacidadMaximaDia,
                    CitasOcupadasDiaEspecialidad = citasOcupadasEspecialidadDia,
                    Horarios = Array.Empty<HorarioDisponibleItemDto>()
                };
            }

            var horarios = (await _horarioSucursalRepository.ObtenerPorSucursalAsync(idSucursal))
                .Where(h => h.DiaSemana == MapearDiaSemana(fecha.Date) && h.HoraInicio.HasValue && h.HoraFin.HasValue)
                .OrderBy(h => h.HoraInicio)
                .ToList();

            if (horarios.Count == 0)
            {
                return new HorariosDisponiblesCitaDto
                {
                    IdClinica = idClinica,
                    IdSucursal = idSucursal,
                    IdEspecialidad = idEspecialidad,
                    Fecha = fecha.Date,
                    DuracionEstimadaMin = duracionEstimadaMin,
                    IntervaloMin = intervalo,
                    CitasMaximasPorDiaEspecialidad = capacidadMaximaDia,
                    CitasOcupadasDiaEspecialidad = citasOcupadasEspecialidadDia,
                    Horarios = Array.Empty<HorarioDisponibleItemDto>()
                };
            }

            var disponibles = new List<HorarioDisponibleItemDto>();
            var inicioDia = fecha.Date;

            foreach (var tramo in horarios)
            {
                var inicioTramo = inicioDia.Add(tramo.HoraInicio!.Value);
                var finTramo = inicioDia.Add(tramo.HoraFin!.Value);

                if (finTramo <= inicioTramo)
                {
                    continue;
                }

                for (var inicioCita = inicioTramo; inicioCita.AddMinutes(duracionEstimadaMin) <= finTramo; inicioCita = inicioCita.AddMinutes(intervalo))
                {
                    if (inicioCita < DateTime.UtcNow)
                    {
                        continue;
                    }

                    var finCita = inicioCita.AddMinutes(duracionEstimadaMin);

                    var traslapes = await _citaRepository.ContarTraslapesEspecialidadHorarioAsync(
                        idClinica,
                        idSucursal,
                        idEspecialidad,
                        inicioCita,
                        finCita);

                    if (traslapes >= totalDoctoresEspecialidad)
                    {
                        continue;
                    }

                    disponibles.Add(new HorarioDisponibleItemDto
                    {
                        FechaHoraInicioPlan = inicioCita,
                        FechaHoraFinPlan = finCita,
                        HoraLabel = inicioCita.ToString("HH:mm")
                    });
                }
            }

            return new HorariosDisponiblesCitaDto
            {
                IdClinica = idClinica,
                IdSucursal = idSucursal,
                IdEspecialidad = idEspecialidad,
                Fecha = fecha.Date,
                DuracionEstimadaMin = duracionEstimadaMin,
                IntervaloMin = intervalo,
                CitasMaximasPorDiaEspecialidad = capacidadMaximaDia,
                CitasOcupadasDiaEspecialidad = citasOcupadasEspecialidadDia,
                Horarios = disponibles
                    .GroupBy(h => h.FechaHoraInicioPlan)
                    .Select(g => g.First())
                    .OrderBy(h => h.FechaHoraInicioPlan)
                    .ToArray()
            };
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

            var especialidadesPorSucursal = new List<CatalogoEspecialidadSucursalDto>();
            foreach (var sucursal in sucursales)
            {
                var configuradas = await _sucursalEspecialidadRepository.ObtenerActivasPorSucursalAsync(sucursal.Id);
                foreach (var cfg in configuradas)
                {
                    var capacidadEstimada = await CalcularCapacidadDiariaEspecialidadAsync(
                        idClinica,
                        sucursal.Id,
                        cfg.IdEspecialidad,
                        DateTime.UtcNow.Date,
                        30);

                    especialidadesPorSucursal.Add(new CatalogoEspecialidadSucursalDto
                    {
                        IdSucursal = cfg.IdSucursal,
                        IdEspecialidad = cfg.IdEspecialidad,
                        NombreEspecialidad = cfg.Especialidad?.Nombre ?? string.Empty,
                        DescripcionEspecialidad = cfg.Especialidad?.Descripcion ?? string.Empty,
                        CitasMaximasPorDia = capacidadEstimada
                    });
                }
            }

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
                EspecialidadesPorSucursal = especialidadesPorSucursal,
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
                if (doctor == null || !doctor.Activo || doctor.IdClinica != dto.IdClinica || !EsProfesionalAtencion(doctor))
                {
                    throw new ValidationException("El profesional seleccionado no es válido para esta clínica.");
                }

                if (doctor.IdSucursal != dto.IdSucursal)
                {
                    throw new ValidationException("El profesional seleccionado no pertenece a la sucursal indicada.");
                }

                if (!doctor.IdEspecialidad.HasValue || doctor.IdEspecialidad.Value != dto.IdEspecialidad)
                {
                    throw new ValidationException("El profesional no corresponde a la especialidad seleccionada.");
                }
            }

            var idsServicios = dto.IdsServicios.Distinct().ToList();
            var servicios = await _servicioRepository.ObtenerPorIdsClinicaAsync(dto.IdClinica, idsServicios);
            if (servicios.Count != idsServicios.Count)
            {
                throw new ValidationException("Uno o más servicios no existen o no pertenecen a la clínica.");
            }

            var creada = await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var duracionTotal = CalcularDuracionTotalMin(servicios.Select(s => s.DuracionMin));
                var subtotal = servicios.Sum(s => s.PrecioBase);
                var fechaFin = dto.FechaHoraInicioPlan.AddMinutes(duracionTotal);

                await ValidarCapacidadSucursalAsync(
                    dto.IdClinica,
                    dto.IdSucursal,
                    dto.IdEspecialidad,
                    dto.FechaHoraInicioPlan,
                    duracionTotal);

                if (dto.IdDoctor.HasValue)
                {
                    var doctorTieneTraslape = await _citaRepository.ExisteTraslapeHorarioDoctorAsync(
                        dto.IdClinica,
                        dto.IdSucursal,
                        dto.IdDoctor.Value,
                        dto.FechaHoraInicioPlan,
                        fechaFin);

                    if (doctorTieneTraslape)
                    {
                        throw new InvalidOperationException("El doctor seleccionado ya tiene una cita en ese rango horario. Selecciona otro horario u otro doctor.");
                    }
                }

                var cita = new Cita
                {
                    IdClinica = dto.IdClinica,
                    IdSucursal = dto.IdSucursal,
                    IdPaciente = dto.IdPaciente,
                    IdEspecialidad = dto.IdEspecialidad,
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

                return await _citaRepository.CrearAsync(cita);
            }, IsolationLevel.Serializable);

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

            var idDoctorAnterior = cita.IdDoctor;

            if (!dto.IdDoctor.HasValue)
            {
                cita.IdDoctor = null;
            }
            else
            {
                var doctor = await _usuarioRepository.ObtenerPorIdAsync(dto.IdDoctor.Value);
                if (doctor == null || !doctor.Activo || doctor.IdClinica != cita.IdClinica || !EsProfesionalAtencion(doctor))
                {
                    throw new ValidationException("El profesional seleccionado no es válido para esta clínica.");
                }

                if (!doctor.IdEspecialidad.HasValue || doctor.IdEspecialidad.Value != cita.IdEspecialidad)
                {
                    throw new ValidationException("El profesional no corresponde a la especialidad de la cita.");
                }

                cita.IdDoctor = doctor.Id;
                if (cita.Estado == EstadoCita.Pendiente)
                {
                    cita.Estado = EstadoCita.Confirmada;
                }
            }

            var actualizada = await _citaRepository.ActualizarAsync(cita);

            if (idDoctorAnterior.HasValue && (!cita.IdDoctor.HasValue || cita.IdDoctor.Value != idDoctorAnterior.Value))
            {
                await _doctorNotificationService.NotifyQueueUpdatedAsync(
                    idDoctorAnterior.Value,
                    actualizada.Id,
                    "doctor-desasignado",
                    "Un paciente fue removido de tu cola.");
            }

            if (cita.IdDoctor.HasValue)
            {
                await _doctorNotificationService.NotifyQueueUpdatedAsync(
                    cita.IdDoctor.Value,
                    actualizada.Id,
                    "doctor-asignado",
                    "Recepcion te asigno un paciente.");
            }

            return MapToResponseDto(actualizada);
        }

        public async Task<CitaResponseDto?> CambiarEstadoAsync(int idCita, CambiarEstadoCitaDto dto, string rolEjecutor, int idUsuarioEjecutor)
        {
            if (idCita <= 0)
            {
                throw new ArgumentException("El ID de la cita debe ser mayor a cero.", nameof(idCita));
            }

            if (idUsuarioEjecutor <= 0)
            {
                throw new ArgumentException("El ID del usuario ejecutor debe ser mayor a cero.", nameof(idUsuarioEjecutor));
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

            if (EsProfesionalRol(rolEjecutor))
            {
                if (!cita.IdDoctor.HasValue || cita.IdDoctor.Value != idUsuarioEjecutor)
                {
                    throw new UnauthorizedAccessException("Solo el doctor asignado puede cambiar el estado de esta cita.");
                }
            }

            if (cita.Estado == EstadoCita.Cancelada || cita.Estado == EstadoCita.Completada)
            {
                throw new InvalidOperationException("No se puede cambiar el estado de una cita cancelada o completada.");
            }

            if (!EsTransicionPermitida(cita.Estado, dto.NuevoEstado, rolEjecutor))
            {
                throw new UnauthorizedAccessException("No tienes permisos para realizar esta transición de estado.");
            }

            if (dto.NuevoEstado == EstadoCita.EnConsulta)
            {
                if (!cita.IdDoctor.HasValue)
                {
                    throw new ValidationException("La cita debe tener doctor asignado para pasar a consulta.");
                }

                cita.FechaHoraInicioReal ??= DateTime.UtcNow;
            }

            if (dto.NuevoEstado == EstadoCita.Presente)
            {
                cita.FechaHoraInicioReal ??= DateTime.UtcNow;
            }

            if (dto.NuevoEstado == EstadoCita.Completada)
            {
                if (cita.Estado == EstadoCita.EnConsulta)
                {
                    var consultaRegistrada = await _citaRepository.ObtenerConsultaPorCitaAsync(idCita);
                    if (consultaRegistrada == null)
                    {
                        throw new InvalidOperationException("Debe registrarse la consulta médica antes de cerrar la cita.");
                    }
                }

                cita.FechaHoraFinReal ??= DateTime.UtcNow;
            }

            cita.Estado = dto.NuevoEstado;

            if (!string.IsNullOrWhiteSpace(dto.NotasOperacion))
            {
                var nota = dto.NotasOperacion.Trim();
                cita.Notas = string.IsNullOrWhiteSpace(cita.Notas)
                    ? nota
                    : $"{cita.Notas} | {nota}";
            }

            var actualizada = await _citaRepository.ActualizarAsync(cita);

            if (actualizada.IdDoctor.HasValue)
            {
                var evento = dto.NuevoEstado switch
                {
                    EstadoCita.Presente => "paciente-en-recepcion",
                    EstadoCita.EnConsulta => "paciente-en-consulta",
                    EstadoCita.Completada => "paciente-finalizado",
                    EstadoCita.Cancelada => "paciente-cancelado",
                    _ => "estado-actualizado"
                };

                await _doctorNotificationService.NotifyQueueUpdatedAsync(
                    actualizada.IdDoctor.Value,
                    actualizada.Id,
                    evento,
                    $"La cita #{actualizada.Id} cambio a estado {actualizada.Estado}.");
            }

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

            if (cita.Estado == EstadoCita.Cancelada || cita.Estado == EstadoCita.Completada)
            {
                throw new InvalidOperationException("La cita no está disponible para registrar consulta.");
            }

            if (!cita.IdDoctor.HasValue)
            {
                throw new ValidationException("La cita debe tener doctor asignado para registrar la consulta.");
            }

            if (cita.Estado != EstadoCita.EnConsulta)
            {
                throw new InvalidOperationException("La cita debe estar en consulta para registrar la consulta médica.");
            }

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

            var rolEjecutorNombre = usuarioEjecutor.Rol?.Nombre ?? string.Empty;
            if (!EsProfesionalRol(rolEjecutorNombre) && !EsRol(rolEjecutorNombre, "Admin"))
            {
                throw new UnauthorizedAccessException("Solo un doctor o admin puede registrar una consulta.");
            }

            if (EsProfesionalRol(rolEjecutorNombre) && cita.IdDoctor.HasValue && cita.IdDoctor.Value != usuarioEjecutor.Id)
            {
                throw new UnauthorizedAccessException("Solo el doctor asignado puede registrar la consulta de esta cita.");
            }

            var idDoctorAtencion = cita.IdDoctor;

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

            cita.FechaHoraInicioReal ??= cita.FechaHoraInicioPlan;
            if (!cita.IdDoctor.HasValue && idDoctorAtencion.HasValue)
            {
                cita.IdDoctor = idDoctorAtencion;
            }

            var notaConsulta = $"Consulta médica registrada por doctor #{idDoctorEjecutor}.";
            cita.Notas = string.IsNullOrWhiteSpace(cita.Notas)
                ? notaConsulta
                : $"{cita.Notas} | {notaConsulta}";

            await _citaRepository.ActualizarAsync(cita);

            return MapConsultaToResponseDto(consultaCreada);
        }

        private static bool EsTransicionPermitida(EstadoCita estadoActual, EstadoCita estadoNuevo, string rolEjecutor)
        {
            if (estadoActual == estadoNuevo)
            {
                return true;
            }

            if (EsRol(rolEjecutor, "Admin"))
            {
                return true;
            }

            return NormalizarRol(rolEjecutor) switch
            {
                "recepcionista" =>
                    (estadoActual == EstadoCita.Pendiente || estadoActual == EstadoCita.Confirmada) && estadoNuevo == EstadoCita.Presente
                    || estadoActual == EstadoCita.Presente && estadoNuevo == EstadoCita.EnConsulta
                    || estadoActual == EstadoCita.EnConsulta && estadoNuevo == EstadoCita.Completada
                    || (estadoActual == EstadoCita.Pendiente || estadoActual == EstadoCita.Confirmada || estadoActual == EstadoCita.Presente) && estadoNuevo == EstadoCita.Cancelada,
                "doctor" or "nutricionista" or "fisioterapeuta" =>
                    estadoActual == EstadoCita.Presente && estadoNuevo == EstadoCita.EnConsulta,
                _ => false
            };
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
                IdEspecialidad = cita.IdEspecialidad,
                NombreEspecialidad = cita.Especialidad?.Nombre ?? string.Empty,
                IdDoctor = cita.IdDoctor,
                NombrePaciente = cita.Paciente != null ? $"{cita.Paciente.Nombres} {cita.Paciente.Apellidos}".Trim() : string.Empty,
                CorreoPaciente = cita.Paciente?.Correo ?? string.Empty,
                TelefonoPaciente = cita.Paciente?.Telefono ?? string.Empty,
                FechaHoraInicioPlan = cita.FechaHoraInicioPlan,
                FechaHoraFinPlan = cita.FechaHoraFinPlan,
                DuracionEstimadaMin = (int)Math.Max(1, (cita.FechaHoraFinPlan - cita.FechaHoraInicioPlan).TotalMinutes),
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

        private async Task ValidarCapacidadSucursalAsync(
            int idClinica,
            int idSucursal,
            int idEspecialidad,
            DateTime fechaHoraInicio,
            int duracionEstimadaMin)
        {
            var config = await _sucursalEspecialidadRepository.ObtenerConfiguracionActivaAsync(idSucursal, idEspecialidad);
            if (config == null)
            {
                throw new InvalidOperationException("La especialidad seleccionada no está habilitada en esta sucursal.");
            }

            var totalDoctoresEspecialidad = await _usuarioRepository.ContarProfesionalesActivosPorEspecialidadAsync(
                idClinica,
                idSucursal,
                idEspecialidad);

            if (totalDoctoresEspecialidad <= 0)
            {
                throw new InvalidOperationException("No hay especialistas activos para esta especialidad en la sucursal seleccionada.");
            }

            var capacidadMaximaDia = await CalcularCapacidadDiariaEspecialidadAsync(
                idClinica,
                idSucursal,
                idEspecialidad,
                fechaHoraInicio.Date,
                duracionEstimadaMin);

            var citasDia = await _citaRepository.ContarCitasActivasEspecialidadDiaAsync(
                idClinica,
                idSucursal,
                idEspecialidad,
                fechaHoraInicio.Date);

            if (capacidadMaximaDia <= 0 || citasDia >= capacidadMaximaDia)
            {
                throw new InvalidOperationException("La especialidad seleccionada alcanzó su capacidad diaria en esta sucursal.");
            }

            var inicio = fechaHoraInicio;
            var fin = fechaHoraInicio.AddMinutes(Math.Max(1, duracionEstimadaMin));

            var traslapes = await _citaRepository.ContarTraslapesEspecialidadHorarioAsync(
                idClinica,
                idSucursal,
                idEspecialidad,
                inicio,
                fin);

            if (traslapes >= totalDoctoresEspecialidad)
            {
                throw new InvalidOperationException("No hay disponibilidad para esa hora en la especialidad seleccionada.");
            }
        }

        private async Task<int> CalcularCapacidadDiariaEspecialidadAsync(
            int idClinica,
            int idSucursal,
            int idEspecialidad,
            DateTime fecha,
            int duracionEstimadaMin)
        {
            var totalDoctoresEspecialidad = await _usuarioRepository.ContarProfesionalesActivosPorEspecialidadAsync(
                idClinica,
                idSucursal,
                idEspecialidad);

            if (totalDoctoresEspecialidad <= 0)
            {
                return 0;
            }

            var minutosJornada = await ObtenerMinutosJornadaSucursalAsync(idSucursal, fecha);
            if (minutosJornada <= 0)
            {
                return 0;
            }

            var duracion = Math.Max(1, duracionEstimadaMin);
            return (minutosJornada * totalDoctoresEspecialidad) / duracion;
        }

        private async Task<int> ObtenerMinutosJornadaSucursalAsync(int idSucursal, DateTime fecha)
        {
            var horarios = (await _horarioSucursalRepository.ObtenerPorSucursalAsync(idSucursal))
                .Where(h => h.DiaSemana == MapearDiaSemana(fecha.Date) && h.HoraInicio.HasValue && h.HoraFin.HasValue)
                .ToList();

            var minutos = 0;
            foreach (var horario in horarios)
            {
                var inicio = fecha.Date.Add(horario.HoraInicio!.Value);
                var fin = fecha.Date.Add(horario.HoraFin!.Value);
                if (fin > inicio)
                {
                    minutos += (int)(fin - inicio).TotalMinutes;
                }
            }

            return minutos;
        }

        private static bool EsProfesionalAtencion(Usuario usuario)
        {
            var rol = NormalizarRol(usuario.Rol?.Nombre);
            return rol is not null && rol != "admin" && rol != "recepcionista";
        }

        private static bool EsRol(string? rol, string esperado)
        {
            return string.Equals(rol?.Trim(), esperado, StringComparison.OrdinalIgnoreCase);
        }

        private static bool EsProfesionalRol(string? rol)
        {
            var normalizado = NormalizarRol(rol);
            return normalizado is "doctor" or "nutricionista" or "fisioterapeuta";
        }

        private static string NormalizarRol(string? rol)
        {
            return (rol ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static int CalcularDuracionTotalMin(IEnumerable<int> duraciones)
        {
            var total = duraciones.Sum();
            return total > 0 ? total : 30;
        }

        private static int MapearDiaSemana(DateTime fecha)
        {
            return fecha.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)fecha.DayOfWeek;
        }
    }
}
