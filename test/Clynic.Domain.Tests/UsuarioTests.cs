using Clynic.Domain.Models;
using Clynic.Domain.Models.Enums;

namespace Clynic.Domain.Tests;

public class UsuarioTests
{
    [Fact]
    public void NuevoUsuario_DebeInicializarValoresPorDefecto()
    {
        var before = DateTime.UtcNow;
        var usuario = new Usuario();
        var after = DateTime.UtcNow;

        Assert.True(usuario.Activo);
        Assert.Equal(UsuarioRol.Admin, usuario.Rol);
        Assert.InRange(usuario.FechaCreacion, before, after);
        Assert.NotNull(usuario.CitasComoDoctor);
        Assert.Empty(usuario.CitasComoDoctor);
        Assert.Null(usuario.Clinica);
    }

    [Fact]
    public void Usuario_DebePermitirAsignarDatosPrincipales()
    {
        var usuario = new Usuario
        {
            IdClinica = 2,
            NombreCompleto = "Dra. Ana Perez",
            Correo = "ana@clynic.com",
            ClaveHash = "HASH",
            Rol = UsuarioRol.Doctor
        };

        Assert.Equal(2, usuario.IdClinica);
        Assert.Equal("Dra. Ana Perez", usuario.NombreCompleto);
        Assert.Equal("ana@clynic.com", usuario.Correo);
        Assert.Equal("HASH", usuario.ClaveHash);
        Assert.Equal(UsuarioRol.Doctor, usuario.Rol);
    }
}
