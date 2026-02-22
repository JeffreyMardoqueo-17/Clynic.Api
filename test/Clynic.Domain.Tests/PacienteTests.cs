using Clynic.Domain.Models;

namespace Clynic.Domain.Tests;

public class PacienteTests
{
    [Fact]
    public void NuevoPaciente_DebeInicializarValoresPorDefecto()
    {
        var before = DateTime.UtcNow;
        var paciente = new Paciente();
        var after = DateTime.UtcNow;

        Assert.InRange(paciente.FechaRegistro, before, after);
        Assert.NotNull(paciente.Citas);
        Assert.Empty(paciente.Citas);
    }

    [Fact]
    public void Paciente_DebePermitirAsignarPropiedadesBasicas()
    {
        var paciente = new Paciente
        {
            NombreCompleto = "Juan López",
            Telefono = "5551231234"
        };

        Assert.Equal("Juan López", paciente.NombreCompleto);
        Assert.Equal("5551231234", paciente.Telefono);
    }
}
