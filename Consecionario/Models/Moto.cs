using System.ComponentModel.DataAnnotations;
using ConcesionarioN.Validations;

namespace ConcesionarioN.Models
{
    public class Moto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La marca es obligatoria")]
        public string Marca { get; set; } = string.Empty;

        [Required(ErrorMessage = "El modelo es obligatorio")]
        public string Modelo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El año es obligatorio")]
        [AnioUnidad]
        public int Anio { get; set; }

        [Range(0.01, 1000000000, ErrorMessage = "El precio debe ser mayor que cero.")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El cilindraje (CC) es obligatorio")]
        public string CilindrajeCC { get; set; } = string.Empty; // Específico para motos

        // Identificadores únicos (HU-07/CA-02). Nulos solo en registros antiguos.
        [StringLength(10)]
        public string? Placa { get; set; }

        [StringLength(17)]
        public string? Vin { get; set; }

        public string Estado { get; set; } = "Disponible";
        public string ImagenUrl { get; set; } = string.Empty;
    }
}
