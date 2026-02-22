using Clynic.Domain.Models.Enums;

namespace Clynic.Domain.Tests;

public class EnumsTests
{
    [Fact]
    public void EstadoCita_DebeMantenerValoresEsperados()
    {
        Assert.Equal(1, (int)EstadoCita.Pendiente);
        Assert.Equal(2, (int)EstadoCita.Confirmada);
        Assert.Equal(3, (int)EstadoCita.Cancelada);
        Assert.Equal(4, (int)EstadoCita.Completada);
    }

    [Fact]
    public void UsuarioRol_DebeMantenerValoresEsperados()
    {
        Assert.Equal(1, (int)UsuarioRol.Admin);
        Assert.Equal(2, (int)UsuarioRol.Doctor);
        Assert.Equal(3, (int)UsuarioRol.Recepcionista);
    }
}
