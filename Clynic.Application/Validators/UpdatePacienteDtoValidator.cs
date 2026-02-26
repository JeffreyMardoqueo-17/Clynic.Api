using Clynic.Application.DTOs.Pacientes;
using Clynic.Application.Rules;
using FluentValidation;

namespace Clynic.Application.Validators
{
    public class UpdatePacienteDtoValidator : AbstractValidator<UpdatePacienteDto>
    {
        public UpdatePacienteDtoValidator(PacienteRules pacienteRules)
        {
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

            RuleFor(x => x.Telefono)
                .MaximumLength(50)
                .WithMessage("El teléfono no puede exceder 50 caracteres.");

            RuleFor(x => x.FechaNacimiento)
                .Must(pacienteRules.FechaNacimientoValida)
                .WithMessage("La fecha de nacimiento no puede ser mayor a la fecha actual.");
        }
    }
}
