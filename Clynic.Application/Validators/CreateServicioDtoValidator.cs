using Clynic.Application.DTOs.Servicios;
using Clynic.Application.Rules;
using FluentValidation;

namespace Clynic.Application.Validators
{
    public class CreateServicioDtoValidator : AbstractValidator<CreateServicioDto>
    {
        private readonly ServicioRules _rules;

        public CreateServicioDtoValidator(ServicioRules rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));

            RuleFor(x => x.IdClinica)
                .GreaterThan(0)
                .WithMessage("El ID de la clínica debe ser mayor a cero.");

            RuleFor(x => x.NombreServicio)
                .NotEmpty()
                .WithMessage("El nombre del servicio es obligatorio.")
                .MaximumLength(150)
                .WithMessage("El nombre del servicio no puede exceder 150 caracteres.")
                .Must(nombre => _rules.NombreEsValido(nombre))
                .WithMessage("El nombre del servicio debe tener al menos 3 caracteres válidos.")
                .MustAsync(async (dto, nombre, cancellation) =>
                    await _rules.NombreEsUnicoAsync(nombre, dto.IdClinica))
                .WithMessage("Ya existe un servicio con ese nombre en la clínica.");

            RuleFor(x => x.DuracionMin)
                .Must(_rules.DuracionEsValida)
                .WithMessage("La duración debe estar entre 1 y 600 minutos.");

            RuleFor(x => x.PrecioBase)
                .Must(_rules.PrecioEsValido)
                .WithMessage("El precio base debe ser mayor o igual a 0.");
        }
    }
}
