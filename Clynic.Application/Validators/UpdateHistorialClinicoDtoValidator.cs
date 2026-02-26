using Clynic.Application.DTOs.Pacientes;
using FluentValidation;

namespace Clynic.Application.Validators
{
    public class UpdateHistorialClinicoDtoValidator : AbstractValidator<UpdateHistorialClinicoDto>
    {
        public UpdateHistorialClinicoDtoValidator()
        {
            RuleFor(x => x.EnfermedadesPrevias)
                .MaximumLength(4000)
                .WithMessage("Las enfermedades previas no pueden exceder 4000 caracteres.");

            RuleFor(x => x.MedicamentosActuales)
                .MaximumLength(4000)
                .WithMessage("Los medicamentos actuales no pueden exceder 4000 caracteres.");

            RuleFor(x => x.Alergias)
                .MaximumLength(4000)
                .WithMessage("Las alergias no pueden exceder 4000 caracteres.");

            RuleFor(x => x.AntecedentesFamiliares)
                .MaximumLength(4000)
                .WithMessage("Los antecedentes familiares no pueden exceder 4000 caracteres.");

            RuleFor(x => x.Observaciones)
                .MaximumLength(4000)
                .WithMessage("Las observaciones no pueden exceder 4000 caracteres.");
        }
    }
}
