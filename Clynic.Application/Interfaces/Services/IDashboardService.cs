using Clynic.Application.DTOs.Dashboard;

namespace Clynic.Application.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<DashboardResumenDto> ObtenerResumenAsync(int idClinica, int? idSucursal = null, DateTime? fechaReferencia = null);
        Task<DashboardCitasSerieDto> ObtenerCitasPorDiaAsync(int idClinica, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idSucursal = null);
        Task<DashboardOperativoDto> ObtenerOperativoAsync(int idClinica, int? idSucursal = null, DateTime? fechaReferencia = null);
    }
}
