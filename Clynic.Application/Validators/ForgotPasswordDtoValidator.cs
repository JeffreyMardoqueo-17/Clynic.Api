using FluentValidation;
using Clynic.Application.DTOs.Usuarios;

namespace Clynic.Application.Validators
{
    public class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
    {
        public ForgotPasswordDtoValidator()
        {
            RuleFor(x => x.Correo)
                .NotEmpty()
                .WithMessage("El correo es obligatorio")
                .EmailAddress()
                .WithMessage("El formato del correo no es válido");
        }
    }

    public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordDtoValidator()
        {
            RuleFor(x => x.Correo)
                .NotEmpty()
                .WithMessage("El correo es obligatorio")
                .EmailAddress()
                .WithMessage("El formato del correo no es válido");

            RuleFor(x => x.Codigo)
                .NotEmpty()
                .WithMessage("El código de verificación es obligatorio")
                .Length(8, 12)
                .WithMessage("El código debe tener entre 8 y 12 caracteres");

            RuleFor(x => x.NuevaClave)
                .NotEmpty()
                .WithMessage("La nueva clave es obligatoria")
                .MinimumLength(6)
                .WithMessage("La nueva clave debe tener al menos 6 caracteres")
                .MaximumLength(100)
                .WithMessage("La nueva clave no puede exceder 100 caracteres");

            RuleFor(x => x.ConfirmarClave)
                .NotEmpty()
                .WithMessage("La confirmación de clave es obligatoria")
                .Equal(x => x.NuevaClave)
                .WithMessage("Las claves no coinciden");
        }
    }
}
