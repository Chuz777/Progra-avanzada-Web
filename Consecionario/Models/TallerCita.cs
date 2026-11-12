using System.ComponentModel.DataAnnotations;

namespace ConcesionarioN.Models
{
    public class TallerCita
    {
        [Key]
        public int Id { get; set; }

        // Relación opcional con el usuario registrado
        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [Required(ErrorMessage = "La placa o identificador del vehículo es obligatorio")]
        public string PlacaVehiculo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe detallar el problema")]
        public string DescripcionProblema { get; set; } = string.Empty;

        public string EstadoTaller { get; set; } = "Recibido"; // Recibido, En Proceso, Reparado

        [Required]
        public DateTime FechaCita { get; set; } = DateTime.Now;

  
        [Required]
        public string ClienteNombre { get; set; } = string.Empty;

        [Required]
        public string ClienteTelefono { get; set; } = string.Empty;

        [Required]
        public string ClienteEmail { get; set; } = string.Empty;
    }
}