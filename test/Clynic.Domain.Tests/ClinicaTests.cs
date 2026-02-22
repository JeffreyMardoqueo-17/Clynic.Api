using Clynic.Domain.Models;

namespace Clynic.Domain.Tests;

public class ClinicaTests
{
    [Fact]
    public void NuevaClinica_DebeInicializarValoresPorDefecto()
    {
        var before = DateTime.UtcNow;
        var clinica = new Clinica();
        var after = DateTime.UtcNow;

        Assert.True(clinica.Activa);
        Assert.InRange(clinica.FechaCreacion, before, after);
        Assert.NotNull(clinica.Sucursales);
        Assert.NotNull(clinica.Usuarios);
        Assert.NotNull(clinica.Servicios);
        Assert.NotNull(clinica.Citas);
        Assert.Empty(clinica.Sucursales);
        Assert.Empty(clinica.Usuarios);
        Assert.Empty(clinica.Servicios);
        Assert.Empty(clinica.Citas);
    }

    [Fact]
    public void Clinica_DebePermitirAsignarPropiedadesBasicas()
    {
        var clinica = new Clinica
        {
            Nombre = "Clynic Norte",
            Telefono = "+52 555 111 2222",
            Direccion = "Av. Reforma 100",
            Activa = false
        };

        Assert.Equal("Clynic Norte", clinica.Nombre);
        Assert.Equal("+52 555 111 2222", clinica.Telefono);
        Assert.Equal("Av. Reforma 100", clinica.Direccion);
        Assert.False(clinica.Activa);
    }
}
