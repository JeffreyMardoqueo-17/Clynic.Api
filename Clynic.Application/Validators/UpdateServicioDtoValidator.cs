using Clynic.Application.DTOs.Servicios;
using Clynic.Application.Rules;
using FluentValidation;

namespace Clynic.Application.Validators
{
    public class UpdateServicioDtoValidator : AbstractValidator<UpdateServicioDto>
    {
        private readonly ServicioRules _rules;

        public UpdateServicioDtoValidator(ServicioRules rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));

            RuleFor(x => x)
                .Must(x =>
                    x.NombreServicio != null ||
                    x.DuracionMin.HasValue ||
                    x.PrecioBase.HasValue ||
                    x.Activo.HasValue)
                .WithMessage("Debes enviar al menos un campo para actualizar.");

            When(x => x.NombreServicio != null, () =>
            {
                RuleFor(x => x.NombreServicio!)
                    .MaximumLength(150)
                    .WithMessage("El nombre del servicio no puede exceder 150 caracteres.")
                    .Must(nombre => _rules.NombreEsValido(nombre))
                    .WithMessage("El nombre del servicio debe tener al menos 3 caracteres válidos.");
            });

            When(x => x.DuracionMin.HasValue, () =>
            {
                RuleFor(x => x.DuracionMin!.Value)
                    .Must(_rules.DuracionEsValida)
                    .WithMessage("La duración debe estar entre 1 y 600 minutos.");
            });

            When(x => x.PrecioBase.HasValue, () =>
            {
                RuleFor(x => x.PrecioBase!.Value)
                    .Must(_rules.PrecioEsValido)
                    .WithMessage("El precio base debe ser mayor o igual a 0.");
            });
        }
    }
}
