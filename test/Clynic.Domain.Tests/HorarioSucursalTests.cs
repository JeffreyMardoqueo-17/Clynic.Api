using Clynic.Domain.Models;

namespace Clynic.Domain.Tests;

public class HorarioSucursalTests
{
    [Fact]
    public void NuevoHorarioSucursal_DebePermitirAsignarPropiedades()
    {
        var inicio = new TimeSpan(8, 0, 0);
        var fin = new TimeSpan(16, 0, 0);

        var horario = new HorarioSucursal
        {
            IdSucursal = 4,
            DiaSemana = 1,
            HoraInicio = inicio,
            HoraFin = fin
        };

        Assert.Equal(4, horario.IdSucursal);
        Assert.Equal(1, horario.DiaSemana);
        Assert.Equal(inicio, horario.HoraInicio);
        Assert.Equal(fin, horario.HoraFin);
        Assert.Null(horario.Sucursal);
    }
}
