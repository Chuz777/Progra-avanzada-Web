using ConcesionarioN.DTOs;
using ConcesionarioN.Models;

namespace ConcesionarioN.Services
{
    public interface IMecanicaService
    {
        // ---------- Datos para los formularios ----------
        Task<List<Usuario>> ClientesActivosAsync();

        // Placas conocidas de cada cliente (unidades compradas y recepciones anteriores)
        Task<Dictionary<int, List<string>>> PlacasPorClienteAsync(int? soloClienteId = null);

        // ---------- Recepciones (HU-20, HU-21, HU-22) ----------
        Task<ResultadoMecanica> RegistrarRecepcionAsync(RecepcionCrearDto dto);

        // soloClienteId: si viene, solo devuelve los vehículos de ese cliente (HU-21/CA-02)
        Task<List<TallerCita>> ListarRecepcionesAsync(FiltroRecepcionesDto filtro, int? soloClienteId);
        Task<TallerCita?> ObtenerRecepcionAsync(int id);
        Task<ResultadoOperacion> CambiarEstadoAsync(int id, string nuevoEstado);
        Task<ResultadoOperacion> EliminarRecepcionAsync(int id);

        // ---------- Citas de ingreso (HU-23, HU-24) ----------
        // excluirCitaId: al reprogramar, el horario actual de la cita cuenta como libre
        Task<List<HorarioItem>> HorariosAsync(DateTime dia, int? excluirCitaId = null);
        Task<ResultadoMecanica> AgendarCitaAsync(CitaDto dto);
        Task<CitaIngreso?> ObtenerCitaAsync(int id);
        Task<ResultadoMecanica> ReprogramarCitaAsync(int id, DateTime? fecha, int? hora);
        Task<ResultadoOperacion> CancelarCitaAsync(int id);
        Task<AgendaDto> AgendaAsync(string? vista, DateTime? fecha, int? soloClienteId);
    }
}
