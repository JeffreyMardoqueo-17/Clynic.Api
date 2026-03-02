using Clynic.Application.DTOs.Dashboard;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Application.Interfaces.Services;
using Clynic.Domain.Models.Enums;

namespace Clynic.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IClinicaRepository _clinicaRepository;
        private readonly IPacienteRepository _pacienteRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ISucursalRepository _sucursalRepository;
        private readonly ICitaRepository _citaRepository;

        public DashboardService(
            IClinicaRepository clinicaRepository,
            IPacienteRepository pacienteRepository,
            IUsuarioRepository usuarioRepository,
            ISucursalRepository sucursalRepository,
            ICitaRepository citaRepository)
        {
            _clinicaRepository = clinicaRepository ?? throw new ArgumentNullException(nameof(clinicaRepository));
            _pacienteRepository = pacienteRepository ?? throw new ArgumentNullException(nameof(pacienteRepository));
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _sucursalRepository = sucursalRepository ?? throw new ArgumentNullException(nameof(sucursalRepository));
            _citaRepository = citaRepository ?? throw new ArgumentNullException(nameof(citaRepository));
        }

        public async Task<DashboardResumenDto> ObtenerResumenAsync(int idClinica, int? idSucursal = null, DateTime? fechaReferencia = null)
        {
            if (idClinica <= 0)
            {
                throw new ArgumentException("El ID de la clínica debe ser mayor a cero.", nameof(idClinica));
            }

            var clinica = await _clinicaRepository.ObtenerPorIdAsync(idClinica)
                ?? throw new KeyNotFoundException($"No se encontró la clínica con ID {idClinica}.");

            var fechaCorte = (fechaReferencia ?? DateTime.UtcNow).Date;
            if (fechaCorte < clinica.FechaCreacion.Date)
            {
                fechaCorte = clinica.FechaCreacion.Date;
            }

            var inicioDia = fechaCorte;
            var finDia = fechaCorte.AddDays(1);

            var totalPacientes = await _pacienteRepository.ContarPorClinicaAsync(idClinica);
            var totalTrabajadores = idSucursal.HasValue
                ? await _usuarioRepository.ContarActivosPorClinicaYSucursalAsync(idClinica, idSucursal.Value)
                : await _usuarioRepository.ContarActivosPorClinicaAsync(idClinica);
            var totalSucursales = idSucursal.HasValue
                ? 1
                : await _sucursalRepository.ContarActivasPorClinicaAsync(idClinica);
            var totalCitasHoy = await _citaRepository.ContarPorClinicaYFechaAsync(idClinica, inicioDia, finDia, idSucursal);

            return new DashboardResumenDto
            {
                TotalPacientes = totalPacientes,
                TotalTrabajadores = totalTrabajadores,
                TotalSucursales = totalSucursales,
                TotalCitasHoy = totalCitasHoy,
                FechaCorte = fechaCorte,
            };
        }

        public async Task<DashboardCitasSerieDto> ObtenerCitasPorDiaAsync(int idClinica, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idSucursal = null)
        {
            if (idClinica <= 0)
            {
                throw new ArgumentException("El ID de la clínica debe ser mayor a cero.", nameof(idClinica));
            }

            var clinica = await _clinicaRepository.ObtenerPorIdAsync(idClinica)
                ?? throw new KeyNotFoundException($"No se encontró la clínica con ID {idClinica}.");

            var fechaMinima = clinica.FechaCreacion.Date;
            var fechaFinal = (fechaHasta ?? DateTime.UtcNow).Date;

            if (fechaFinal < fechaMinima)
            {
                fechaFinal = fechaMinima;
            }

            var fechaInicial = (fechaDesde ?? fechaFinal.AddDays(-29)).Date;
            if (fechaInicial < fechaMinima)
            {
                fechaInicial = fechaMinima;
            }

            if (fechaInicial > fechaFinal)
            {
                throw new ArgumentException("La fecha inicial no puede ser mayor que la fecha final.");
            }

            var agregados = await _citaRepository.ObtenerTotalesPorDiaAsync(
                idClinica,
                fechaInicial,
                fechaFinal.AddDays(1),
                idSucursal);

            var mapaTotales = agregados
                .GroupBy(x => x.Fecha.Date)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Total));

            var serie = new List<DashboardCitasPorDiaDto>();
            for (var fecha = fechaInicial; fecha <= fechaFinal; fecha = fecha.AddDays(1))
            {
                serie.Add(new DashboardCitasPorDiaDto
                {
                    Fecha = fecha,
                    TotalCitas = mapaTotales.TryGetValue(fecha.Date, out var total) ? total : 0
                });
            }

            return new DashboardCitasSerieDto
            {
                FechaMinima = fechaMinima,
                FechaDesde = fechaInicial,
                FechaHasta = fechaFinal,
                TotalPeriodo = serie.Sum(x => x.TotalCitas),
                Serie = serie
            };
        }

        public async Task<DashboardOperativoDto> ObtenerOperativoAsync(int idClinica, int? idSucursal = null, DateTime? fechaReferencia = null)
        {
            if (idClinica <= 0)
            {
                throw new ArgumentException("El ID de la clínica debe ser mayor a cero.", nameof(idClinica));
            }

            var clinica = await _clinicaRepository.ObtenerPorIdAsync(idClinica)
                ?? throw new KeyNotFoundException($"No se encontró la clínica con ID {idClinica}.");

            var fechaCorte = (fechaReferencia ?? DateTime.UtcNow).Date;
            if (fechaCorte < clinica.FechaCreacion.Date)
            {
                fechaCorte = clinica.FechaCreacion.Date;
            }

            var inicioDia = fechaCorte;
            var finDia = fechaCorte.AddDays(1);

            var totalPacientes = await _pacienteRepository.ContarPorClinicaAsync(idClinica);
            var totalTrabajadores = idSucursal.HasValue
                ? await _usuarioRepository.ContarActivosPorClinicaYSucursalAsync(idClinica, idSucursal.Value)
                : await _usuarioRepository.ContarActivosPorClinicaAsync(idClinica);

            var totalCitasHoy = await _citaRepository.ContarPorClinicaYFechaAsync(idClinica, inicioDia, finDia, idSucursal);

            var totalCitasPendientes = await _citaRepository.ContarPorClinicaYFechaAsync(
                idClinica,
                inicioDia,
                DateTime.MaxValue,
                idSucursal,
                EstadoCita.Pendiente);

            var pendientes = await _citaRepository.ObtenerPorClinicaAsync(
                idClinica,
                inicioDia,
                null,
                idSucursal,
                EstadoCita.Pendiente);

            var listaPendientes = pendientes
                .OrderBy(c => c.FechaHoraInicioPlan)
                .Take(50)
                .Select(c => new DashboardCitaPendienteDto
                {
                    IdCita = c.Id,
                    IdSucursal = c.IdSucursal,
                    NombrePaciente = c.Paciente != null
                        ? $"{c.Paciente.Nombres} {c.Paciente.Apellidos}".Trim()
                        : $"Paciente {c.IdPaciente}",
                    FechaHoraInicioPlan = c.FechaHoraInicioPlan,
                    FechaHoraFinPlan = c.FechaHoraFinPlan,
                    Estado = c.Estado.ToString(),
                })
                .ToList();

            return new DashboardOperativoDto
            {
                TotalPacientes = totalPacientes,
                TotalCitasHoy = totalCitasHoy,
                TotalCitasPendientes = totalCitasPendientes,
                TotalTrabajadores = totalTrabajadores,
                FiltradoPorSucursal = idSucursal.HasValue,
                IdSucursal = idSucursal,
                Pendientes = listaPendientes,
            };
        }
    }
}
