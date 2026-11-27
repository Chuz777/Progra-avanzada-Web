using System.ComponentModel.DataAnnotations;

namespace ConcesionarioN.Models
{
    // Recepción de un vehículo en el taller (HU-20, HU-21, HU-22)
    public class TallerCita
    {
        [Key]
        public int Id { get; set; }

        // Cliente registrado dueño del vehículo
        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [Required(ErrorMessage = "La placa o identificador del vehículo es obligatorio")]
        public string PlacaVehiculo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe detallar el problema")]
        public string DescripcionProblema { get; set; } = string.Empty;

        // Condición en que se recibe el vehículo (golpes, kilometraje, etc.)
        [StringLength(300)]
        public string? EstadoVehiculo { get; set; }

        // Recibido, En diagnóstico, En reparación, Listo, Entregado (ver Validations/EstadosTaller.cs)
        public string EstadoTaller { get; set; } = "Recibido";

        // Fecha de ingreso al taller
        [Required]
        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        // HU-21/CA-01: fecha del último cambio de estado
        public DateTime? FechaCambioEstado { get; set; }

        // Copia de los datos del cliente al momento de la recepción
        [Required]
        public string ClienteNombre { get; set; } = string.Empty;

        public string? ClienteTelefono { get; set; }

        public string? ClienteEmail { get; set; }
    }
}
