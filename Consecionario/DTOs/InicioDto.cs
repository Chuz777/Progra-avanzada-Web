using ConcesionarioN.Models;

namespace ConcesionarioN.DTOs
{
    // Datos de la página de inicio pública
    public class InicioDto
    {
        public List<Vehiculo> Vehiculos { get; set; } = new();
        public List<Moto> Motos { get; set; } = new();
    }
}
