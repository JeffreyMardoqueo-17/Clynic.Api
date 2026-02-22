using Clynic.Domain.Models;
using Clynic.Domain.Models.Enums;

namespace Clynic.Domain.Tests;

public class CitaTests
{
    [Fact]
    public void NuevaCita_DebeInicializarValoresPorDefecto()
    {
        var before = DateTime.UtcNow;
        var cita = new Cita();
        var after = DateTime.UtcNow;

        Assert.Equal(EstadoCita.Pendiente, cita.Estado);
        Assert.Equal(string.Empty, cita.Notas);
        Assert.Equal(0m, cita.SubTotal);
        Assert.Equal(0m, cita.TotalFinal);
        Assert.InRange(cita.FechaCreacion, before, after);
        Assert.NotNull(cita.CitaServicios);
        Assert.Empty(cita.CitaServicios);
        Assert.Null(cita.Clinica);
        Assert.Null(cita.Sucursal);
        Assert.Null(cita.Paciente);
        Assert.Null(cita.Doctor);
    }

    [Fact]
    public void Cita_DebePermitirAsignarRelacionesYDatosPlanificados()
    {
        var inicio = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        var fin = inicio.AddMinutes(30);

        var cita = new Cita
        {
            IdClinica = 1,
            IdSucursal = 2,
            IdPaciente = 3,
            IdDoctor = 4,
            FechaHoraInicioPlan = inicio,
            FechaHoraFinPlan = fin,
            Estado = EstadoCita.Confirmada,
            Notas = "Primera consulta"
        };

        Assert.Equal(1, cita.IdClinica);
        Assert.Equal(2, cita.IdSucursal);
        Assert.Equal(3, cita.IdPaciente);
        Assert.Equal(4, cita.IdDoctor);
        Assert.Equal(inicio, cita.FechaHoraInicioPlan);
        Assert.Equal(fin, cita.FechaHoraFinPlan);
        Assert.Equal(EstadoCita.Confirmada, cita.Estado);
        Assert.Equal("Primera consulta", cita.Notas);
    }
}
