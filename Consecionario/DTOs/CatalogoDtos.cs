namespace ConcesionarioN.DTOs
{
    // Error asociado a un campo de formulario
    public record ErrorCampo(string Campo, string Mensaje);

    // Filtros del catálogo de vehículos y motos (HU-08/CA-01)
    public class FiltroCatalogoDto
    {
        public string Controlador { get; set; } = "Vehiculos";
        public string? Marca { get; set; }
        public decimal? PrecioMax { get; set; }
        public string? Estado { get; set; }
    }
}
