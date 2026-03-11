using Clynic.Application.DTOs.Citas;
using FluentValidation;

namespace Clynic.Application.Validators
{
    public class RegistrarConsultaMedicaDtoValidator : AbstractValidator<RegistrarConsultaMedicaDto>
    {
        public RegistrarConsultaMedicaDtoValidator()
        {
            RuleFor(x => x.Diagnostico)
                .NotEmpty()
                .WithMessage("El diagnóstico es obligatorio.")
                .MaximumLength(4000)
                .WithMessage("El diagnóstico no puede exceder 4000 caracteres.");

            RuleFor(x => x.Tratamiento)
                .MaximumLength(4000)
                .WithMessage("El tratamiento no puede exceder 4000 caracteres.");

            RuleFor(x => x.Receta)
                .MaximumLength(4000)
                .WithMessage("La receta no puede exceder 4000 caracteres.");

            RuleFor(x => x.ExamenesSolicitados)
                .MaximumLength(4000)
                .WithMessage("Los exámenes solicitados no pueden exceder 4000 caracteres.");

            RuleFor(x => x.NotasMedicas)
                .MaximumLength(4000)
                .WithMessage("Las notas médicas no pueden exceder 4000 caracteres.");

            RuleFor(x => x.FechaConsulta)
                .Must(f => !f.HasValue || f.Value <= DateTime.UtcNow.AddMinutes(5))
                .WithMessage("La fecha de consulta no puede estar en el futuro.");
        }
    }
}
