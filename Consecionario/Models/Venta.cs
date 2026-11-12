using System.ComponentModel.DataAnnotations;

namespace ConcesionarioN.Models
{
    public class Venta
    {
        [Key]
        public int Id { get; set; }

        public int? VehiculoId { get; set; }
        public Vehiculo? Vehiculo { get; set; }

        public int? AccesorioId { get; set; }
        public Accesorio? Accesorio { get; set; }

        [Required]
        public string ClienteNombre { get; set; } = string.Empty;

        [Required]
        public decimal TotalVenta { get; set; }

        public DateTime FechaVenta { get; set; } = DateTime.Now;
    }
}