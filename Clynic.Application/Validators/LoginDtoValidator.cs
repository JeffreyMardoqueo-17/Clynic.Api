using FluentValidation;
using Clynic.Application.DTOs.Usuarios;

namespace Clynic.Application.Validators
{
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Correo)
                .NotEmpty()
                .WithMessage("El correo es obligatorio")
                .EmailAddress()
                .WithMessage("El formato del correo no es válido");

            RuleFor(x => x.Clave)
                .NotEmpty()
                .WithMessage("La clave es obligatoria");
        }
    }
}
