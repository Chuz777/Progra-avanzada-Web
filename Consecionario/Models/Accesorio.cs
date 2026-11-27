using System.ComponentModel.DataAnnotations;

namespace ConcesionarioN.Models
{
    public class Accesorio
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoría es obligatoria")]
        [StringLength(50, ErrorMessage = "La categoría no puede superar 50 caracteres")]
        public string Categoria { get; set; } = string.Empty;

        [Range(0, 1000000000, ErrorMessage = "El precio no puede ser negativo.")]
        public decimal Precio { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; }

        public string ImagenUrl { get; set; } = string.Empty;
    }
}
