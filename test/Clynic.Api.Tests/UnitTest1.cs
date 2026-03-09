using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Clynic.Api.Tests;

public class UnitTest1
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static string BaseUrl =>
        Environment.GetEnvironmentVariable("API_E2E_BASEURL")?.TrimEnd('/')
        ?? "http://localhost:8080";

    [Fact]
    public async Task HealthEndpoint_ShouldBeHealthy()
    {
        using var http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        using var response = await http.GetAsync("/Health/status");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("OK", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ReservaPublica_MismaSucursalMismoHorario_EnParalelo_DebePermitirSoloUna()
    {
        using var http = new HttpClient { BaseAddress = new Uri(BaseUrl) };

        var catalog = await ObtenerCatalogoPublicoAsync(http);
        var clinicaId = catalog.IdClinica;
        var sucursalId = catalog.Sucursales[0].Id;
        var servicioId = catalog.Servicios[0].Id;

        var fechaHoraInicio = DateTime.UtcNow
            .AddDays(15)
            .Date
            .AddHours(10)
            .AddMinutes(Random.Shared.Next(0, 50));

        var payload1 = new CrearCitaPublicaRequest
        {
            IdClinica = clinicaId,
            IdSucursal = sucursalId,
            Nombres = "Paciente",
            Apellidos = "ConcurrenciaA",
            Telefono = "8290001001",
            Correo = $"e2e-a-{Guid.NewGuid():N}@mail.com",
            FechaHoraInicioPlan = fechaHoraInicio,
            Notas = "E2E concurrencia A",
            IdsServicios = [servicioId]
        };

        var payload2 = new CrearCitaPublicaRequest
        {
            IdClinica = clinicaId,
            IdSucursal = sucursalId,
            Nombres = "Paciente",
            Apellidos = "ConcurrenciaB",
            Telefono = "8290001002",
            Correo = $"e2e-b-{Guid.NewGuid():N}@mail.com",
            FechaHoraInicioPlan = fechaHoraInicio,
            Notas = "E2E concurrencia B",
            IdsServicios = [servicioId]
        };

        var t1 = http.PostAsJsonAsync("/api/Citas/publica", payload1);
        var t2 = http.PostAsJsonAsync("/api/Citas/publica", payload2);

        var responses = await Task.WhenAll(t1, t2);

        var created = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var conflict = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

        if (created == 0 && conflict == 0)
        {
            var details = await Task.WhenAll(responses.Select(async r =>
            {
                var content = await r.Content.ReadAsStringAsync();
                return $"{(int)r.StatusCode} {r.StatusCode}: {content}";
            }));

            Assert.Fail("Respuestas inesperadas al reservar en paralelo: " + string.Join(" | ", details));
        }

        Assert.Equal(1, created);
        Assert.Equal(1, conflict);
    }

    private static async Task<CatalogoPublicoResponse> ObtenerCatalogoPublicoAsync(HttpClient http)
    {
        Exception? lastError = null;

        for (int clinicaId = 1; clinicaId <= 30; clinicaId++)
        {
            try
            {
                using var response = await http.GetAsync($"/api/Citas/publica/catalogo/{clinicaId}");
                if (!response.IsSuccessStatusCode)
                {
                    continue;
                }

                var body = await response.Content.ReadAsStringAsync();
                var catalogo = JsonSerializer.Deserialize<CatalogoPublicoResponse>(body, JsonOptions);

                if (catalogo?.Sucursales?.Count > 0 && catalogo?.Servicios?.Count > 0)
                {
                    return catalogo;
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
            }
        }

        var baseMessage = "No se encontró un catálogo público válido (sucursales + servicios).";
        if (lastError is not null)
        {
            throw new InvalidOperationException(baseMessage, lastError);
        }

        throw new InvalidOperationException(baseMessage);
    }

    private sealed class CatalogoPublicoResponse
    {
        public int IdClinica { get; set; }
        public List<CatalogoItem> Sucursales { get; set; } = [];
        public List<CatalogoItem> Servicios { get; set; } = [];
    }

    private sealed class CatalogoItem
    {
        public int Id { get; set; }
    }

    private sealed class CrearCitaPublicaRequest
    {
        public int IdClinica { get; set; }
        public int IdSucursal { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public DateTime FechaHoraInicioPlan { get; set; }
        public string Notas { get; set; } = string.Empty;
        public List<int> IdsServicios { get; set; } = [];
    }
}
