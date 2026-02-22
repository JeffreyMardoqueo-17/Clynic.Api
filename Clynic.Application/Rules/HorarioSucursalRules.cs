using Clynic.Application.Interfaces.Repositories;

namespace Clynic.Application.Rules
{
    /// <summary>
    /// Clase con las reglas de negocio para Horarios de Sucursal
    /// </summary>
    public class HorarioSucursalRules
    {
        private readonly IHorarioSucursalRepository _horarioRepository;
        private readonly ISucursalRepository _sucursalRepository;

        public HorarioSucursalRules(
            IHorarioSucursalRepository horarioRepository,
            ISucursalRepository sucursalRepository)
        {
            _horarioRepository = horarioRepository ?? throw new ArgumentNullException(nameof(horarioRepository));
            _sucursalRepository = sucursalRepository ?? throw new ArgumentNullException(nameof(sucursalRepository));
        }

        /// <summary>
        /// Valida si la sucursal existe
        /// </summary>
        public async Task<bool> SucursalExisteAsync(int idSucursal)
        {
            return await _sucursalRepository.ExisteAsync(idSucursal);
        }

        /// <summary>
        /// Valida que el dia de la semana sea valido (0 = Domingo, 6 = Sabado)
        /// </summary>
        public bool DiaSemanaEsValido(int diaSemana)
        {
            return diaSemana >= 0 && diaSemana <= 6;
        }

        /// <summary>
        /// Valida que el rango horario sea correcto
        /// </summary>
        public bool RangoHorarioEsValido(TimeSpan horaInicio, TimeSpan horaFin)
        {
            return horaInicio < horaFin;
        }

        /// <summary>
        /// Valida que no exista cruce de horarios para la misma sucursal y dia
        /// </summary>
        public async Task<bool> NoExisteCruceHorarioAsync(int idSucursal, int diaSemana, TimeSpan horaInicio, TimeSpan horaFin)
        {
            var existeCruce = await _horarioRepository.ExisteCruceHorarioAsync(idSucursal, diaSemana, horaInicio, horaFin);
            return !existeCruce;
        }
    }
}
