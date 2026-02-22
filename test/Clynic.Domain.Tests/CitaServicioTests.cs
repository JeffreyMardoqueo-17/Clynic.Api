using Clynic.Domain.Models;

namespace Clynic.Domain.Tests;

public class CitaServicioTests
{
    [Fact]
    public void NuevoCitaServicio_DebePermitirAsignaciones()
    {
        var citaServicio = new CitaServicio
        {
            IdCita = 8,
            IdServicio = 9,
            DuracionMin = 45,
            Precio = 1200m
        };

        Assert.Equal(8, citaServicio.IdCita);
        Assert.Equal(9, citaServicio.IdServicio);
        Assert.Equal(45, citaServicio.DuracionMin);
        Assert.Equal(1200m, citaServicio.Precio);
        Assert.Null(citaServicio.Cita);
        Assert.Null(citaServicio.Servicio);
    }
}
