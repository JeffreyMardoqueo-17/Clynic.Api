using FluentValidation;
using Clynic.Application.DTOs.Usuarios;

namespace Clynic.Application.Validators
{
    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(x => x.ClaveActual)
                .NotEmpty()
                .WithMessage("La clave actual es obligatoria");

            RuleFor(x => x.NuevaClave)
                .NotEmpty()
                .WithMessage("La nueva clave es obligatoria")
                .MinimumLength(6)
                .WithMessage("La nueva clave debe tener al menos 6 caracteres")
                .MaximumLength(100)
                .WithMessage("La nueva clave no puede exceder 100 caracteres")
                .NotEqual(x => x.ClaveActual)
                .WithMessage("La nueva clave debe ser diferente a la actual");

            RuleFor(x => x.ConfirmarClave)
                .NotEmpty()
                .WithMessage("La confirmación de clave es obligatoria")
                .Equal(x => x.NuevaClave)
                .WithMessage("Las claves no coinciden");
        }
    }
}
