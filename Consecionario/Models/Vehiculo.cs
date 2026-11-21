using System.ComponentModel.DataAnnotations;

namespace ConcesionarioN.Models
{
    public class Vehiculo
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La marca es obligatoria")]
        public string Marca { get; set; } = string.Empty;

        [Required(ErrorMessage = "El modelo es obligatorio")]
        public string Modelo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El año es obligatorio")]
        public int Anio { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        public decimal Precio { get; set; }

        public string Transmision { get; set; } = "Automática";
        public string Estado { get; set; } = "Disponible";
        public string ImagenUrl { get; set; } = string.Empty;
    }
}