using FluentValidation;
using Clynic.Application.DTOs.Usuarios;
using Clynic.Application.Rules;

namespace Clynic.Application.Validators
{
    public class UpdateUsuarioDtoValidator : AbstractValidator<UpdateUsuarioDto>
    {
        private readonly UsuarioRules _rules;

        public UpdateUsuarioDtoValidator(UsuarioRules rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));

            When(x => !string.IsNullOrWhiteSpace(x.NombreCompleto), () =>
            {
                RuleFor(x => x.NombreCompleto!)
                    .MinimumLength(3)
                    .WithMessage("El nombre debe tener al menos 3 caracteres")
                    .MaximumLength(150)
                    .WithMessage("El nombre no puede exceder 150 caracteres");
            });

            When(x => !string.IsNullOrWhiteSpace(x.Correo), () =>
            {
                RuleFor(x => x.Correo!)
                    .EmailAddress()
                    .WithMessage("El formato del correo no es válido")
                    .MaximumLength(150)
                    .WithMessage("El correo no puede exceder 150 caracteres");
            });

            When(x => x.IdRol.HasValue, () =>
            {
                RuleFor(x => x.IdRol!.Value)
                    .GreaterThan(0)
                    .WithMessage("El ID del rol debe ser mayor a 0");
            });

            When(x => x.IdEspecialidad.HasValue, () =>
            {
                RuleFor(x => x.IdEspecialidad!.Value)
                    .GreaterThan(0)
                    .WithMessage("El ID de la especialidad debe ser mayor a 0");
            });

            When(x => x.IdSucursal.HasValue, () =>
            {
                RuleFor(x => x.IdSucursal!.Value)
                    .GreaterThan(0)
                    .WithMessage("El ID de la sucursal debe ser mayor a 0");
            });
        }
    }
}
