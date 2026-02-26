using Clynic.Application.DTOs.HorariosSucursal;
using Clynic.Application.Rules;
using FluentValidation;

namespace Clynic.Application.Validators
{
    public class UpdateHorarioSucursalDtoValidator : AbstractValidator<UpdateHorarioSucursalDto>
    {
        private readonly HorarioSucursalRules _rules;

        public UpdateHorarioSucursalDtoValidator(HorarioSucursalRules rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));

            RuleFor(x => x.DiaSemana)
                .Must(diaSemana => _rules.DiaSemanaEsValido(diaSemana))
                .WithMessage("El dia de la semana debe estar entre 1 (Lunes) y 7 (Domingo).");

            RuleFor(x => x)
                .Must(dto => _rules.RangoHorarioEsValido(dto.HoraInicio, dto.HoraFin))
                .WithMessage("La hora de inicio debe ser menor a la hora de fin.");
        }
    }
}