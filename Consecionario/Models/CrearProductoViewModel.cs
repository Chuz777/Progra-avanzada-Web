using System.ComponentModel.DataAnnotations;

namespace ConcesionarioN.Models
{
    public class CrearProductoViewModel
    {
        [Required(ErrorMessage = "Seleccione qué desea registrar")]
        public string TipoProducto { get; set; } = "Vehiculo"; // "Vehiculo", "Moto", "Accesorio"

        // Campos compartidos (Vehículo y Moto)
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public int? Anio { get; set; }
        public string? Placa { get; set; }
        public string? Vin { get; set; }
        public string? ImagenUrl { get; set; }

        // Campo compartido por los 3 (la validación de rango depende del tipo y se hace en el controlador)
        public decimal Precio { get; set; }

        // Exclusivo Vehículo
        public string Transmision { get; set; } = "Automática";

        // Exclusivo Moto
        public string? CilindrajeCC { get; set; }

        // Exclusivos Accesorio
        public string? Nombre { get; set; }
        public string? Categoria { get; set; }
        public int? Stock { get; set; }
    }
}
