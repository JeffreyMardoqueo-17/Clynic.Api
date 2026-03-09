using Clynic.Application.DTOs.CatalogoPersonal;

namespace Clynic.Application.Interfaces.Services
{
    public interface ICatalogoPersonalService
    {
        Task<IReadOnlyCollection<RolSucursalResponseDto>> CrearRolesBaseSucursalAsync(int idClinica, int idSucursal);
        Task<RolSucursalResponseDto> CrearRolAsync(CreateRolSucursalDto createDto);
        Task<IReadOnlyCollection<RolSucursalResponseDto>> ObtenerRolesPorSucursalAsync(int idClinica, int idSucursal);

        Task<EspecialidadResponseDto> CrearEspecialidadAsync(CreateEspecialidadDto createDto);
        Task<IReadOnlyCollection<EspecialidadResponseDto>> ObtenerEspecialidadesPorClinicaAsync(int idClinica);

        Task<EspecialidadSucursalResponseDto> AsignarEspecialidadASucursalAsync(AsignarEspecialidadSucursalDto createDto);
        Task<IReadOnlyCollection<EspecialidadSucursalResponseDto>> ObtenerEspecialidadesPorSucursalAsync(int idClinica, int idSucursal);
    }
}
