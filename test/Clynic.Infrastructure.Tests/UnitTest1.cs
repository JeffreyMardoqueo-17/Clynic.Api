using Microsoft.EntityFrameworkCore;
using Clynic.Domain.Models;
using Clynic.Infrastructure.Data;
using Clynic.Infrastructure.Repositories;

namespace Clynic.Infrastructure.Tests;

public class ClinicaRepositoryTests
{
    [Fact]
    public async Task CrearAsync_DebePersistirClinicaActiva()
    {
        await using var context = BuildContext();
        var repository = new ClinicaRepository(context);

        var creada = await repository.CrearAsync(new Clinica
        {
            Nombre = "Clinica Repo",
            Telefono = "55555555",
            Direccion = "Calle Repo 10"
        });

        Assert.True(creada.Id > 0);
        Assert.True(creada.Activa);
        Assert.True(await context.Clinicas.AnyAsync(c => c.Id == creada.Id));
    }

    [Fact]
    public async Task ObtenerTodasAsync_DebeRetornarSoloActivasYOrdenadasPorNombre()
    {
        await using var context = BuildContext();

        context.Clinicas.AddRange(
            new Clinica { Nombre = "Zeta", Telefono = "1", Direccion = "A", Activa = true },
            new Clinica { Nombre = "Alfa", Telefono = "2", Direccion = "B", Activa = true },
            new Clinica { Nombre = "Beta", Telefono = "3", Direccion = "C", Activa = false }
        );

        await context.SaveChangesAsync();

        var repository = new ClinicaRepository(context);
        var resultado = (await repository.ObtenerTodasAsync()).ToList();

        Assert.Equal(2, resultado.Count);
        Assert.Equal("Alfa", resultado[0].Nombre);
        Assert.Equal("Zeta", resultado[1].Nombre);
    }

    private static ClynicDbContext BuildContext()
    {
        var options = new DbContextOptionsBuilder<ClynicDbContext>()
            .UseInMemoryDatabase($"infra-test-{Guid.NewGuid()}")
            .Options;

        return new ClynicDbContext(options);
    }
}
