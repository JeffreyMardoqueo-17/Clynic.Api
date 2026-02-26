using Clynic.Application.DTOs.Citas;
using Clynic.Application.Rules;
using FluentValidation;

namespace Clynic.Application.Validators
{
    public class CreateCitaPublicaDtoValidator : AbstractValidator<CreateCitaPublicaDto>
    {
        public CreateCitaPublicaDtoValidator(CitaRules citaRules, PacienteRules pacienteRules)
        {
            RuleFor(x => x.IdClinica)
                .GreaterThan(0)
                .WithMessage("El ID de la clínica debe ser mayor a cero.");

            RuleFor(x => x.IdSucursal)
                .GreaterThan(0)
                .WithMessage("El ID de la sucursal debe ser mayor a cero.");

            RuleFor(x => x.Nombres)
                .NotEmpty()
                .WithMessage("Los nombres son requeridos.")
                .MaximumLength(150)
                .Must(pacienteRules.NombreValido)
                .WithMessage("Los nombres deben contener al menos 2 caracteres.");

            RuleFor(x => x.Apellidos)
                .NotEmpty()
                .WithMessage("Los apellidos son requeridos.")
                .MaximumLength(150)
                .Must(pacienteRules.NombreValido)
                .WithMessage("Los apellidos deben contener al menos 2 caracteres.");

            RuleFor(x => x.Correo)
                .NotEmpty()
                .WithMessage("El correo es requerido.")
                .Must(pacienteRules.CorreoValido)
                .WithMessage("El correo no tiene un formato válido.");

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
        }
    }
}
