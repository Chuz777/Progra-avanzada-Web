using ConcesionarioN.DTOs;
using ConcesionarioN.Models;

namespace ConcesionarioN.Services
{
    public interface IVentaService
    {
        Task<ResultadoVenta> RegistrarUnidadAsync(VentaUnidadDto dto, int vendedorId);
        Task<ResultadoVenta> RegistrarAccesoriosAsync(VentaAccesoriosDto dto, int vendedorId);

        // soloClienteId: si viene, solo devuelve las compras de ese cliente (HU-15/CA-02)
        Task<List<Venta>> ListarAsync(FiltroVentasDto filtro, int? soloClienteId);
        Task<Venta?> ObtenerAsync(int id);

        Task<ResultadoOperacion> EditarAsync(int id, string metodoPago, string? notas);
        Task<ResultadoOperacion> AnularAsync(int id);

        Task<DashboardDto> DashboardAsync(FiltroDashboardDto filtro);
        Task<byte[]> ExportarCsvAsync(FiltroDashboardDto filtro);

        Task<List<Usuario>> ClientesActivosAsync();
        Task<List<UnidadVentaItem>> UnidadesAsync();
        Task<List<Accesorio>> AccesoriosAsync();
    }
}
