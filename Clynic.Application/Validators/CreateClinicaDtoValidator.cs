using FluentValidation;
using Clynic.Application.DTOs.Clinicas;
using Clynic.Application.Rules;

namespace Clynic.Application.Validators
{
    /// <summary>
    /// Validador para CreateClinicaDto usando FluentValidation
    /// </summary>
    public class CreateClinicaDtoValidator : AbstractValidator<CreateClinicaDto>
    {
        private readonly ClinicaRules _rules;

        public CreateClinicaDtoValidator(ClinicaRules rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));

            // Validación del Nombre
            RuleFor(x => x.Nombre)
                .NotEmpty()
                .WithMessage("El nombre de la clínica es obligatorio.")
                .MinimumLength(3)
                .WithMessage("El nombre debe tener al menos 3 caracteres.")
                .MaximumLength(150)
                .WithMessage("El nombre no puede exceder 150 caracteres.")
                .Must(nombre => _rules.NombreTieneLongitudMinima(nombre))
                .WithMessage("El nombre debe tener al menos 3 caracteres válidos.")
                .MustAsync(async (nombre, cancellation) => await _rules.NombreEsUnicoAsync(nombre))
                .WithMessage("Ya existe una clínica con este nombre.");

            // Validación del Teléfono
            RuleFor(x => x.Telefono)
                .NotEmpty()
                .WithMessage("El teléfono es obligatorio.")
                .MinimumLength(7)
                .WithMessage("El teléfono debe tener al menos 7 caracteres.")
                .MaximumLength(50)
                .WithMessage("El teléfono no puede exceder 50 caracteres.")
                .Must(telefono => _rules.TelefonoEsValido(telefono))
                .WithMessage("El teléfono contiene caracteres no permitidos o es inválido.");

            // Validación de la Dirección
            RuleFor(x => x.Direccion)
                .NotEmpty()
                .WithMessage("La dirección es obligatoria.")
                .MinimumLength(5)
                .WithMessage("La dirección debe tener al menos 5 caracteres.")
                .MaximumLength(250)
                .WithMessage("La dirección no puede exceder 250 caracteres.")
                .Must(direccion => _rules.DireccionEsValida(direccion))
                .WithMessage("La dirección debe tener al menos 5 caracteres válidos.");
        }
    }
}
