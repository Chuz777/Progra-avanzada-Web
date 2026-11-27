using System.ComponentModel.DataAnnotations;

namespace ConcesionarioN.Models
{
    // Cita de ingreso al taller (HU-23, HU-24)
    public class CitaIngreso
    {
        [Key]
        public int Id { get; set; }

        // Cliente registrado y copia de su nombre
        public int ClienteId { get; set; }
        public Usuario? Cliente { get; set; }

        [Required]
        [StringLength(100)]
        public string ClienteNombre { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string PlacaVehiculo { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Motivo { get; set; }

        // Fecha y hora de la cita (una cita por horario)
        public DateTime FechaHora { get; set; }

        // "Programada" o "Cancelada" (ver Validations/EstadosTaller.cs)
        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Programada";

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaCancelacion { get; set; }
    }
}
