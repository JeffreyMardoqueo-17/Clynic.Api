using Clynic.Application.DTOs.HorariosSucursal;
using Clynic.Application.Rules;
using FluentValidation;

namespace Clynic.Application.Validators
{
    /// <summary>
    /// Validador para CreateHorarioSucursalDto usando FluentValidation
    /// </summary>
    public class CreateHorarioSucursalDtoValidator : AbstractValidator<CreateHorarioSucursalDto>
    {
        private readonly HorarioSucursalRules _rules;

        public CreateHorarioSucursalDtoValidator(HorarioSucursalRules rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));

            RuleFor(x => x.IdSucursal)
                .GreaterThan(0)
                .WithMessage("El ID de la sucursal debe ser mayor a cero.")
                .MustAsync(async (idSucursal, cancellation) =>
                    await _rules.SucursalExisteAsync(idSucursal))
                .WithMessage("La sucursal especificada no existe.");

            RuleFor(x => x.DiaSemana)
                .Must(diaSemana => _rules.DiaSemanaEsValido(diaSemana))
                .WithMessage("El dia de la semana debe estar entre 1 (Lunes) y 7 (Domingo).");

            RuleFor(x => x)
                .Must(dto => _rules.RangoHorarioEsValido(dto.HoraInicio, dto.HoraFin))
                .WithMessage("La hora de inicio debe ser menor a la hora de fin.");

            RuleFor(x => x)
                .MustAsync(async (dto, cancellation) =>
                    await _rules.NoExisteCruceHorarioAsync(dto.IdSucursal, dto.DiaSemana, dto.HoraInicio, dto.HoraFin))
                .WithMessage("Ya existe un horario que se cruza en la misma sucursal y dia.");
        }
    }
}
