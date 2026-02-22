using Clynic.Domain.Models;

namespace Clynic.Domain.Tests;

public class ServicioTests
{
    [Fact]
    public void NuevoServicio_DebeInicializarValoresPorDefecto()
    {
        var servicio = new Servicio();

        Assert.True(servicio.Activo);
        Assert.NotNull(servicio.CitaServicios);
        Assert.Empty(servicio.CitaServicios);
        Assert.Null(servicio.Clinica);
    }

    [Fact]
    public void Servicio_DebePermitirAsignarPropiedades()
    {
        var servicio = new Servicio
        {
            IdClinica = 7,
            NombreServicio = "Consulta General",
            DuracionMin = 30,
            PrecioBase = 500m
        };

        Assert.Equal(7, servicio.IdClinica);
        Assert.Equal("Consulta General", servicio.NombreServicio);
        Assert.Equal(30, servicio.DuracionMin);
        Assert.Equal(500m, servicio.PrecioBase);
    }
}
