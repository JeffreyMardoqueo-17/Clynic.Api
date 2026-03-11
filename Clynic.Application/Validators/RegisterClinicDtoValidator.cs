using Clynic.Application.DTOs.Usuarios;
using Clynic.Application.Rules;
using FluentValidation;

namespace Clynic.Application.Validators
{
    public class RegisterClinicDtoValidator : AbstractValidator<RegisterClinicDto>
    {
        public RegisterClinicDtoValidator(UsuarioRules rules)
        {
            RuleFor(x => x.NombreClinica)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(150);

            RuleFor(x => x.TelefonoClinica)
                .NotEmpty()
                .MinimumLength(7)
                .MaximumLength(50);

            RuleFor(x => x.DireccionClinica)
                .NotEmpty()
                .MinimumLength(5)
                .MaximumLength(250);

            RuleFor(x => x.NombreCompleto)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(150);

            RuleFor(x => x.Correo)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(150);

            RuleFor(x => x.Clave)
                .NotEmpty()
                .MinimumLength(6)
                .MaximumLength(100)
                .Must(rules.ClaveEsValida)
                .WithMessage("La clave debe tener al menos 6 caracteres válidos");
        }
    }
}
