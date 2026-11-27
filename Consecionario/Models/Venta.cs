using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConcesionarioN.Models
{
    public class Venta
    {
        [Key]
        public int Id { get; set; }

        public DateTime FechaVenta { get; set; } = DateTime.Now;

        // "Vehiculo", "Moto" o "Accesorio"
        [StringLength(20)]
        public string Tipo { get; set; } = string.Empty;

        // Cliente registrado (rol Cliente) y copia de su nombre al momento de la venta
        public int? ClienteId { get; set; }
        public Usuario? Cliente { get; set; }

        [Required]
        public string ClienteNombre { get; set; } = string.Empty;

        // Quién registró la venta (Admin o Dependiente)
        public int? VendedorId { get; set; }
        public Usuario? Vendedor { get; set; }

        // Venta de una unidad (vehículo o moto)
        public int? VehiculoId { get; set; }
        public Vehiculo? Vehiculo { get; set; }

        public int? MotoId { get; set; }
        public Moto? Moto { get; set; }

        // Texto del producto vendido, tal como se mostró al vender
        [StringLength(300)]
        public string Descripcion { get; set; } = string.Empty;

        [StringLength(30)]
        public string MetodoPago { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notas { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalVenta { get; set; }

        // HU-16: una venta anulada se conserva, pero no cuenta en indicadores
        public bool Anulada { get; set; }
        public DateTime? FechaAnulacion { get; set; }

        // Líneas de accesorios (HU-14)
        public List<VentaLinea> Lineas { get; set; } = new();
    }
}
