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

        // Obligatoria al registrarse como cliente (HU-02); opcional para personal (HU-03).
        [StringLength(15)]
        public string? Cedula { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [StringLength(15)]
        public string? Telefono { get; set; }

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // "Admin", "Dependiente" o "Cliente" (ver Validations/RolesSistema.cs)
        [Required]
        [StringLength(20)]
        public string Rol { get; set; } = "Cliente";

        // HU-06: un usuario con movimientos se desactiva en lugar de eliminarse.
        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
