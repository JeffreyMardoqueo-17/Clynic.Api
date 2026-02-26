using Clynic.Application.DTOs.Citas;
using Clynic.Application.Rules;
using Clynic.Domain.Models.Enums;
using FluentValidation;

namespace Clynic.Application.Validators
{
    public class CreateCitaInternaDtoValidator : AbstractValidator<CreateCitaInternaDto>
    {
        public CreateCitaInternaDtoValidator(CitaRules citaRules)
        {
            RuleFor(x => x.IdClinica)
                .GreaterThan(0)
                .WithMessage("El ID de la clínica debe ser mayor a cero.");

            RuleFor(x => x.IdSucursal)
                .GreaterThan(0)
                .WithMessage("El ID de la sucursal debe ser mayor a cero.");

            RuleFor(x => x.IdPaciente)
                .GreaterThan(0)
                .WithMessage("El ID del paciente debe ser mayor a cero.");

            RuleFor(x => x.IdDoctor)
                .Must(v => !v.HasValue || v.Value > 0)
                .WithMessage("El ID del doctor debe ser mayor a cero.");

            RuleFor(x => x.FechaHoraInicioPlan)
                .Must(citaRules.FechaAgendamientoValida)
                .WithMessage("La fecha y hora de la cita deben ser iguales o posteriores al momento actual.");

            RuleFor(x => x.IdsServicios)
                .Must(citaRules.TieneServicios)
                .WithMessage("Debe seleccionar al menos un servicio.");

            RuleForEach(x => x.IdsServicios)
                .GreaterThan(0)
                .WithMessage("Todos los IDs de servicios deben ser mayores a cero.");

            RuleFor(x => x.Notas)
                .MaximumLength(250)
                .WithMessage("Las notas no pueden exceder 250 caracteres.");

            RuleFor(x => x.EstadoInicial)
                .Must(estado => estado == EstadoCita.Pendiente || estado == EstadoCita.Confirmada || estado == EstadoCita.Cancelada)
                .WithMessage("El estado inicial solo puede ser Pendiente, Confirmada o Cancelada.");
        }
    }
}
