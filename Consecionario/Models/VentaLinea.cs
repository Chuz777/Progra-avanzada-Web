using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConcesionarioN.Models
{
    // Línea de una venta de accesorios: accesorio, cantidad y precio unitario al momento de vender
    public class VentaLinea
    {
        [Key]
        public int Id { get; set; }

        public int VentaId { get; set; }
        public Venta? Venta { get; set; }

        public int AccesorioId { get; set; }
        public Accesorio? Accesorio { get; set; }

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }
    }
}
