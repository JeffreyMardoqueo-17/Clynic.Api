using FluentValidation;
using Clynic.Application.DTOs.Sucursales;
using Clynic.Application.Rules;

namespace Clynic.Application.Validators
{
    /// <summary>
    /// Validador para CreateSucursalDto usando FluentValidation
    /// </summary>
    public class CreateSucursalDtoValidator : AbstractValidator<CreateSucursalDto>
    {
        private readonly SucursalRules _rules;

        public CreateSucursalDtoValidator(SucursalRules rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));

            RuleFor(x => x.IdClinica)
                .GreaterThan(0)
                .WithMessage("El ID de la clinica debe ser mayor a cero.");

            RuleFor(x => x.Nombre)
                .NotEmpty()
                .WithMessage("El nombre de la sucursal es obligatorio.")
                .MinimumLength(3)
                .WithMessage("El nombre debe tener al menos 3 caracteres.")
                .MaximumLength(150)
                .WithMessage("El nombre no puede exceder 150 caracteres.")
                .Must(nombre => _rules.NombreTieneLongitudMinima(nombre))
                .WithMessage("El nombre debe tener al menos 3 caracteres validos.")
                .MustAsync(async (dto, nombre, cancellation) =>
                    await _rules.NombreEsUnicoAsync(nombre, dto.IdClinica))
                .WithMessage("Ya existe una sucursal con este nombre en la clinica.");

            RuleFor(x => x.Direccion)
                .NotEmpty()
                .WithMessage("La direccion es obligatoria.")
                .MinimumLength(5)
                .WithMessage("La direccion debe tener al menos 5 caracteres.")
                .MaximumLength(250)
                .WithMessage("La direccion no puede exceder 250 caracteres.")
                .Must(direccion => _rules.DireccionEsValida(direccion))
                .WithMessage("La direccion debe tener al menos 5 caracteres validos.");
        }
    }
}
