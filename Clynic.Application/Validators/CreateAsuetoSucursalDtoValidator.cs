using Clynic.Application.DTOs.HorariosSucursal;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Application.Rules;
using FluentValidation;

namespace Clynic.Application.Validators
{
    public class CreateAsuetoSucursalDtoValidator : AbstractValidator<CreateAsuetoSucursalDto>
    {
        public CreateAsuetoSucursalDtoValidator(
            HorarioSucursalRules horarioRules,
            IAsuetoSucursalRepository asuetoRepository)
        {
            RuleFor(x => x.IdSucursal)
                .GreaterThan(0)
                .WithMessage("El ID de la sucursal debe ser mayor a cero.")
                .MustAsync(async (idSucursal, cancellation) =>
                    await horarioRules.SucursalExisteAsync(idSucursal))
                .WithMessage("La sucursal especificada no existe.");

            RuleFor(x => x.Fecha)
                .MustAsync(async (dto, fecha, cancellation) =>
                    !await asuetoRepository.ExisteEnFechaAsync(dto.IdSucursal, fecha))
                .WithMessage("Ya existe un asueto registrado para esa fecha en la sucursal.");

            RuleFor(x => x.Motivo)
                .MaximumLength(200)
                .WithMessage("El motivo no puede exceder 200 caracteres.");
        }
    }
}