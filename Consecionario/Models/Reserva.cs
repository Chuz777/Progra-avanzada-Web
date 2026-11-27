using System.ComponentModel.DataAnnotations;

namespace ConcesionarioN.Models
{
    public class Reserva
    {
        [Key]
        public int Id { get; set; } // <--- Esta es la clave primaria que falta

        [Required]
        public int VehiculoId { get; set; }
        public Vehiculo? Vehiculo { get; set; }

        [Required]
        public string ClienteNombre { get; set; } = string.Empty;

        [Required]
        public decimal MontoAdelanto { get; set; }

        public string EstadoReserva { get; set; } = "Activa";

        public DateTime FechaReserva { get; set; } = DateTime.Now;

        [Required]
        public DateTime FechaLimiteFinanciamiento { get; set; }
    }
}