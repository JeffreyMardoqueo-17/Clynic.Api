using Clynic.Domain.Models;

namespace Clynic.Domain.Tests;

public class SucursalTests
{
    [Fact]
    public void NuevaSucursal_DebeInicializarValoresPorDefecto()
    {
        var sucursal = new Sucursal();

        Assert.True(sucursal.Activa);
        Assert.NotNull(sucursal.Horarios);
        Assert.NotNull(sucursal.Citas);
        Assert.Empty(sucursal.Horarios);
        Assert.Empty(sucursal.Citas);
        Assert.Null(sucursal.Clinica);
    }

    [Fact]
    public void Sucursal_DebePermitirAsignarPropiedadesBasicas()
    {
        var sucursal = new Sucursal
        {
            IdClinica = 10,
            Nombre = "Sucursal Centro",
            Direccion = "Calle 5"
        };

        Assert.Equal(10, sucursal.IdClinica);
        Assert.Equal("Sucursal Centro", sucursal.Nombre);
        Assert.Equal("Calle 5", sucursal.Direccion);
    }
}
