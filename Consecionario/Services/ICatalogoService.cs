using ConcesionarioN.DTOs;

namespace ConcesionarioN.Services
{
    public interface ICatalogoService
    {
        // Normaliza placa y VIN (mayúsculas, sin espacios)
        string? Normalizar(string? valor);

        // HU-07/CA-02: formato y duplicados de placa/VIN entre vehículos y motos.
        // tipo = "Vehiculo" o "Moto"; idExcluir = unidad que se está editando.
        Task<List<ErrorCampo>> ValidarIdentificadoresAsync(string? placa, string? vin, string tipo, int? idExcluir);

        // HU-09/CA-02
        Task<bool> VehiculoVendidoOReservadoAsync(int id);
        Task<bool> MotoVendidaAsync(int id);

        // HU-10/CA-02
        Task<bool> VehiculoTieneMovimientosAsync(int id);
        Task<bool> MotoTieneMovimientosAsync(int id);

        // HU-12/CA-02
        Task<bool> AccesorioTieneVentasAsync(int id);
    }
}
