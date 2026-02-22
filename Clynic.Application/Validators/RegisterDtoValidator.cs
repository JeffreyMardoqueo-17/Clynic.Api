using FluentValidation;
using Clynic.Application.DTOs.Usuarios;
using Clynic.Application.Rules;

namespace Clynic.Application.Validators
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        private readonly UsuarioRules _rules;

        public RegisterDtoValidator(UsuarioRules rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));

            RuleFor(x => x.NombreCompleto)
                .NotEmpty()
                .WithMessage("El nombre completo es obligatorio")
                .MinimumLength(3)
                .WithMessage("El nombre debe tener al menos 3 caracteres")
                .MaximumLength(150)
                .WithMessage("El nombre no puede exceder 150 caracteres");

            RuleFor(x => x.Correo)
                .NotEmpty()
                .WithMessage("El correo es obligatorio")
                .EmailAddress()
                .WithMessage("El formato del correo no es válido")
                .MaximumLength(150)
                .WithMessage("El correo no puede exceder 150 caracteres")
                .MustAsync(async (correo, cancellation) => await _rules.CorreoEsUnicoAsync(correo))
                .WithMessage("Ya existe un usuario con este correo electrónico");

            RuleFor(x => x.Clave)
                .NotEmpty()
                .WithMessage("La clave es obligatoria")
                .MinimumLength(6)
                .WithMessage("La clave debe tener al menos 6 caracteres")
                .MaximumLength(100)
                .WithMessage("La clave no puede exceder 100 caracteres")
                .Must(clave => _rules.ClaveEsValida(clave))
                .WithMessage("La clave debe tener al menos 6 caracteres válidos");

            RuleFor(x => x.IdClinica)
                .NotEmpty()
                .WithMessage("El ID de la clínica es obligatorio")
                .GreaterThan(0)
                .WithMessage("El ID de la clínica debe ser mayor a 0")
                .MustAsync(async (idClinica, cancellation) => await _rules.ClinicaExisteAsync(idClinica))
                .WithMessage("La clínica especificada no existe");

            RuleFor(x => x.Rol)
                .IsInEnum()
                .WithMessage("El rol especificado no es válido");
        }
    }
}
