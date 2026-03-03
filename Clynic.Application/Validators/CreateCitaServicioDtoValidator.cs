using Clynic.Application.DTOs.CitaServicios;
using FluentValidation;

namespace Clynic.Application.Validators
{
    public class CreateCitaServicioDtoValidator : AbstractValidator<CreateCitaServicioDto>
    {
        public CreateCitaServicioDtoValidator()
        {
            RuleFor(x => x.IdCita)
                .GreaterThan(0)
                .WithMessage("El ID de la cita debe ser mayor a cero.");

            RuleFor(x => x.IdServicio)
                .GreaterThan(0)
                .WithMessage("El ID del servicio debe ser mayor a cero.");

            RuleFor(x => x.DuracionMin)
                .GreaterThan(0)
                .When(x => x.DuracionMin.HasValue)
                .WithMessage("La duración debe ser mayor a cero.")
                .LessThanOrEqualTo(1440)
                .When(x => x.DuracionMin.HasValue)
                .WithMessage("La duración no puede exceder 1440 minutos.");

            RuleFor(x => x.Precio)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Precio.HasValue)
                .WithMessage("El precio no puede ser negativo.");
        }
    }
}
