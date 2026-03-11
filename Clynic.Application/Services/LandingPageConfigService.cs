using System.Text;
using System.Text.Json;
using Clynic.Application.DTOs.LandingPages;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Application.Interfaces.Services;
using Clynic.Domain.Models;

namespace Clynic.Application.Services
{
    public class LandingPageConfigService : ILandingPageConfigService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        private readonly ILandingPageConfigRepository _landingRepository;
        private readonly IClinicaRepository _clinicaRepository;
        private readonly ISucursalRepository _sucursalRepository;
        private readonly IHorarioSucursalRepository _horarioRepository;
        private readonly IServicioRepository _servicioRepository;

        public LandingPageConfigService(
            ILandingPageConfigRepository landingRepository,
            IClinicaRepository clinicaRepository,
            ISucursalRepository sucursalRepository,
            IHorarioSucursalRepository horarioRepository,
            IServicioRepository servicioRepository)
        {
            _landingRepository = landingRepository ?? throw new ArgumentNullException(nameof(landingRepository));
            _clinicaRepository = clinicaRepository ?? throw new ArgumentNullException(nameof(clinicaRepository));
            _sucursalRepository = sucursalRepository ?? throw new ArgumentNullException(nameof(sucursalRepository));
            _horarioRepository = horarioRepository ?? throw new ArgumentNullException(nameof(horarioRepository));
            _servicioRepository = servicioRepository ?? throw new ArgumentNullException(nameof(servicioRepository));
        }

        public async Task<LandingPageConfigResponseDto> ObtenerPorClinicaAsync(int idClinica)
        {
            if (idClinica <= 0)
            {
                throw new ArgumentException("El id de clínica es requerido.", nameof(idClinica));
            }

            var existente = await _landingRepository.ObtenerPorClinicaAsync(idClinica);
            if (existente == null)
            {
                var clinica = await _clinicaRepository.ObtenerPorIdAsync(idClinica)
                    ?? throw new KeyNotFoundException("La clínica no existe.");

                return new LandingPageConfigResponseDto
                {
                    Id = 0,
                    IdClinica = idClinica,
                    NombreLanding = clinica.Nombre,
                    HeroTitulo = $"Bienvenido a {clinica.Nombre}",
                    HeroSubtitulo = "Tu salud en manos expertas",
                    DescripcionGeneral = "Atención médica con procesos ordenados y seguimiento continuo.",
                    TelefonoContacto = clinica.Telefono ?? string.Empty,
                    CorreoContacto = string.Empty,
                    DireccionContacto = clinica.Direccion ?? string.Empty,
                    WhatsappContacto = string.Empty,
                    CtaPrincipalTexto = "Agendar cita",
                    CtaPrincipalUrl = "/agendar-cita",
                    ServiciosDestacados = new List<LandingServiceItemDto>(),
                    MetaTitulo = clinica.Nombre,
                    MetaDescripcion = $"Landing de {clinica.Nombre}",
                    DominioBase = string.Empty,
                    MostrarHorariosSucursal = true,
                    Publicada = false,
                    FechaActualizacion = DateTime.UtcNow,
                };
            }

            return MapConfig(existente);
        }

        public async Task<LandingPageConfigResponseDto> GuardarAsync(UpsertLandingPageConfigDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var clinica = await _clinicaRepository.ObtenerPorIdAsync(dto.IdClinica)
                ?? throw new KeyNotFoundException("La clínica no existe.");

            var existente = await _landingRepository.ObtenerPorClinicaAsync(dto.IdClinica);
            if (existente == null)
            {
                var nuevo = new LandingPageConfig
                {
                    IdClinica = dto.IdClinica,
                    NombreLanding = string.IsNullOrWhiteSpace(dto.NombreLanding) ? clinica.Nombre : dto.NombreLanding.Trim(),
                    HeroTitulo = dto.HeroTitulo.Trim(),
                    HeroSubtitulo = dto.HeroSubtitulo.Trim(),
                    DescripcionGeneral = dto.DescripcionGeneral.Trim(),
                    TelefonoContacto = dto.TelefonoContacto.Trim(),
                    CorreoContacto = dto.CorreoContacto.Trim(),
                    DireccionContacto = dto.DireccionContacto.Trim(),
                    WhatsappContacto = dto.WhatsappContacto.Trim(),
                    CtaPrincipalTexto = dto.CtaPrincipalTexto.Trim(),
                    CtaPrincipalUrl = dto.CtaPrincipalUrl.Trim(),
                    ServiciosJson = ToServiciosJson(dto.ServiciosDestacados),
                    MetaTitulo = dto.MetaTitulo.Trim(),
                    MetaDescripcion = dto.MetaDescripcion.Trim(),
                    DominioBase = NormalizeHost(dto.DominioBase),
                    MostrarHorariosSucursal = dto.MostrarHorariosSucursal,
                    Publicada = dto.Publicada,
                    FechaCreacion = DateTime.UtcNow,
                    FechaActualizacion = DateTime.UtcNow,
                };

                var creado = await _landingRepository.CrearAsync(nuevo);
                return MapConfig(creado);
            }

            existente.NombreLanding = string.IsNullOrWhiteSpace(dto.NombreLanding) ? clinica.Nombre : dto.NombreLanding.Trim();
            existente.HeroTitulo = dto.HeroTitulo.Trim();
            existente.HeroSubtitulo = dto.HeroSubtitulo.Trim();
            existente.DescripcionGeneral = dto.DescripcionGeneral.Trim();
            existente.TelefonoContacto = dto.TelefonoContacto.Trim();
            existente.CorreoContacto = dto.CorreoContacto.Trim();
            existente.DireccionContacto = dto.DireccionContacto.Trim();
            existente.WhatsappContacto = dto.WhatsappContacto.Trim();
            existente.CtaPrincipalTexto = dto.CtaPrincipalTexto.Trim();
            existente.CtaPrincipalUrl = dto.CtaPrincipalUrl.Trim();
            existente.ServiciosJson = ToServiciosJson(dto.ServiciosDestacados);
            existente.MetaTitulo = dto.MetaTitulo.Trim();
            existente.MetaDescripcion = dto.MetaDescripcion.Trim();
            existente.DominioBase = NormalizeHost(dto.DominioBase);
            existente.MostrarHorariosSucursal = dto.MostrarHorariosSucursal;
            existente.Publicada = dto.Publicada;
            existente.FechaActualizacion = DateTime.UtcNow;

            var actualizado = await _landingRepository.ActualizarAsync(existente);
            return MapConfig(actualizado);
        }

        public async Task<LandingPublicResponseDto?> ObtenerPublicaAsync(string clinicaSlug)
        {
            if (string.IsNullOrWhiteSpace(clinicaSlug))
            {
                return null;
            }

            var slugBuscado = Slugify(clinicaSlug);
            if (string.IsNullOrWhiteSpace(slugBuscado))
            {
                return null;
            }

            var clinicas = await _clinicaRepository.ObtenerTodasAsync();
            var clinica = clinicas.FirstOrDefault(c => Slugify(c.Nombre) == slugBuscado);
            if (clinica == null)
            {
                return null;
            }

            var config = await _landingRepository.ObtenerPorClinicaAsync(clinica.Id);
            if (config == null || !config.Publicada)
            {
                return null;
            }

            var sucursales = (await _sucursalRepository.ObtenerPorClinicaAsync(config.IdClinica)).ToList();
            var sucursal = sucursales.FirstOrDefault();

            if (sucursal == null)
            {
                return null;
            }

            var horarios = config.MostrarHorariosSucursal
                ? (await _horarioRepository.ObtenerPorSucursalAsync(sucursal.Id))
                    .Select(h => new LandingHorarioDto
                    {
                        DiaSemana = h.DiaSemana,
                        HoraInicio = h.HoraInicio?.ToString(@"hh\:mm") ?? string.Empty,
                        HoraFin = h.HoraFin?.ToString(@"hh\:mm") ?? string.Empty,
                    })
                    .ToList()
                : new List<LandingHorarioDto>();

            var servicios = (await _servicioRepository.ObtenerPorClinicaAsync(config.IdClinica, incluirInactivos: false))
                .Select(s => new LandingServicioClinicaDto
                {
                    Id = s.Id,
                    NombreServicio = s.NombreServicio,
                    PrecioBase = s.PrecioBase,
                    DuracionMin = s.DuracionMin,
                })
                .ToList();

            var destacados = ParseServiciosJson(config.ServiciosJson);

            return new LandingPublicResponseDto
            {
                IdClinica = clinica.Id,
                IdSucursal = sucursal.Id,
                NombreClinica = clinica.Nombre,
                NombreSucursal = sucursal.Nombre,
                SlugClinica = slugBuscado,
                SubdominioSucursal = Slugify(sucursal.Nombre),
                NombreLanding = config.NombreLanding,
                HeroTitulo = config.HeroTitulo,
                HeroSubtitulo = config.HeroSubtitulo,
                DescripcionGeneral = config.DescripcionGeneral,
                TelefonoContacto = string.IsNullOrWhiteSpace(config.TelefonoContacto) ? (clinica.Telefono ?? string.Empty) : config.TelefonoContacto,
                CorreoContacto = config.CorreoContacto,
                DireccionContacto = string.IsNullOrWhiteSpace(config.DireccionContacto) ? (clinica.Direccion ?? string.Empty) : config.DireccionContacto,
                WhatsappContacto = config.WhatsappContacto,
                CtaPrincipalTexto = config.CtaPrincipalTexto,
                CtaPrincipalUrl = config.CtaPrincipalUrl,
                ServiciosDestacados = destacados,
                ServiciosClinica = servicios,
                HorariosSucursal = horarios,
                MetaTitulo = config.MetaTitulo,
                MetaDescripcion = config.MetaDescripcion,
            };
        }

        private static LandingPageConfigResponseDto MapConfig(LandingPageConfig model)
        {
            return new LandingPageConfigResponseDto
            {
                Id = model.Id,
                IdClinica = model.IdClinica,
                NombreLanding = model.NombreLanding,
                HeroTitulo = model.HeroTitulo,
                HeroSubtitulo = model.HeroSubtitulo,
                DescripcionGeneral = model.DescripcionGeneral,
                TelefonoContacto = model.TelefonoContacto,
                CorreoContacto = model.CorreoContacto,
                DireccionContacto = model.DireccionContacto,
                WhatsappContacto = model.WhatsappContacto,
                CtaPrincipalTexto = model.CtaPrincipalTexto,
                CtaPrincipalUrl = model.CtaPrincipalUrl,
                ServiciosDestacados = ParseServiciosJson(model.ServiciosJson),
                MetaTitulo = model.MetaTitulo,
                MetaDescripcion = model.MetaDescripcion,
                DominioBase = model.DominioBase,
                MostrarHorariosSucursal = model.MostrarHorariosSucursal,
                Publicada = model.Publicada,
                FechaActualizacion = model.FechaActualizacion,
            };
        }

        private static List<LandingServiceItemDto> ParseServiciosJson(string serviciosJson)
        {
            if (string.IsNullOrWhiteSpace(serviciosJson))
            {
                return new List<LandingServiceItemDto>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<LandingServiceItemDto>>(serviciosJson, JsonOptions)
                    ?? new List<LandingServiceItemDto>();
            }
            catch
            {
                return new List<LandingServiceItemDto>();
            }
        }

        private static string ToServiciosJson(List<LandingServiceItemDto> servicios)
        {
            var safe = servicios
                .Where(x => !string.IsNullOrWhiteSpace(x.Titulo))
                .Select(x => new LandingServiceItemDto
                {
                    Titulo = x.Titulo.Trim(),
                    Descripcion = x.Descripcion?.Trim() ?? string.Empty,
                })
                .ToList();

            return JsonSerializer.Serialize(safe, JsonOptions);
        }

        private static string NormalizeHost(string? host)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                return string.Empty;
            }

            var value = host.Trim().ToLowerInvariant();
            var colon = value.IndexOf(':');
            if (colon > -1)
            {
                value = value[..colon];
            }

            if (value.StartsWith("www."))
            {
                value = value[4..];
            }

            return value;
        }

        private static string Slugify(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var normalized = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();
            bool previousDash = false;

            foreach (var ch in normalized)
            {
                if (char.GetUnicodeCategory(ch) == System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsLetterOrDigit(ch))
                {
                    builder.Append(char.ToLowerInvariant(ch));
                    previousDash = false;
                    continue;
                }

                if (!previousDash)
                {
                    builder.Append('-');
                    previousDash = true;
                }
            }

            return builder.ToString().Trim('-');
        }
    }
}
