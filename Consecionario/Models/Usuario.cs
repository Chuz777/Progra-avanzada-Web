using System.ComponentModel.DataAnnotations;

namespace ConcesionarioN.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(50)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de teléfono es obligatorio")]
        [Phone]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Rol { get; set; } = "Cliente"; // "Admin", "Dependiente", "Cliente"
    }
}