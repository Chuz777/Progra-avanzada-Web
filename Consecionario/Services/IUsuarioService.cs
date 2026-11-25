using ConcesionarioN.DTOs;
using ConcesionarioN.Models;

namespace ConcesionarioN.Services
{
    public interface IUsuarioService
    {
        Task<Usuario?> AutenticarAsync(string email, string password);
        Task<ResultadoOperacion> RegistrarClienteAsync(RegistroDto dto);
        Task<ResultadoOperacion> CrearAsync(UsuarioCrearDto dto);
        Task<PaginaResultadoDto<UsuarioListaDto>> ListarAsync(string? texto, string? rol, int pagina, int tamanoPagina);
        Task<Usuario?> ObtenerAsync(int id);
        Task<ResultadoOperacion> ActualizarAsync(UsuarioEditarDto dto);
        Task<bool> TieneMovimientosAsync(int id);
        Task<ResultadoEliminacion> EliminarAsync(int id, int adminActualId);
        Task SembrarDatosInicialesAsync();
    }
}
