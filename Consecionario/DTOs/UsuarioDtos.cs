using System.ComponentModel.DataAnnotations;
using ConcesionarioN.Validations;

namespace ConcesionarioN.DTOs
{
    // HU-01
    public class LoginDto
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = UsuarioValidaciones.CorreoMensaje)]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;
    }

    // HU-02
    public class RegistroDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres")]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cédula es obligatoria")]
        [RegularExpression(UsuarioValidaciones.CedulaRegex, ErrorMessage = UsuarioValidaciones.CedulaMensaje)]
        [Display(Name = "Cédula")]
        public string Cedula { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [RegularExpression(UsuarioValidaciones.TelefonoRegex, ErrorMessage = UsuarioValidaciones.TelefonoMensaje)]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = UsuarioValidaciones.CorreoMensaje)]
        [StringLength(150)]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [RegularExpression(UsuarioValidaciones.PasswordRegex, ErrorMessage = UsuarioValidaciones.PasswordMensaje)]
        [StringLength(100)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;
    }

    // HU-03
    public class UsuarioCrearDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres")]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = UsuarioValidaciones.CorreoMensaje)]
        [StringLength(150)]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        [RegularExpression(UsuarioValidaciones.TelefonoRegex, ErrorMessage = UsuarioValidaciones.TelefonoMensaje)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [RegularExpression(UsuarioValidaciones.PasswordRegex, ErrorMessage = UsuarioValidaciones.PasswordMensaje)]
        [StringLength(100)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seleccione un rol")]
        [Display(Name = "Rol")]
        public string Rol { get; set; } = RolesSistema.Dependiente;
    }

    // HU-05 (nombre, teléfono y rol; el correo se muestra pero no se modifica)
    public class UsuarioEditarDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres")]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        [RegularExpression(UsuarioValidaciones.TelefonoRegex, ErrorMessage = UsuarioValidaciones.TelefonoMensaje)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "Seleccione un rol")]
        [Display(Name = "Rol")]
        public string Rol { get; set; } = RolesSistema.Cliente;
    }

    // HU-04
    public class UsuarioListaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class PaginaResultadoDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        public bool TienePaginaAnterior => PaginaActual > 1;
        public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;
    }

    // Resultado de una operación del servicio; Campo indica a qué input asociar el mensaje.
    public class ResultadoOperacion
    {
        public bool Exito { get; set; }
        public string? Campo { get; set; }
        public string? Mensaje { get; set; }

        public static ResultadoOperacion Ok(string? mensaje = null) => new() { Exito = true, Mensaje = mensaje };
        public static ResultadoOperacion Error(string mensaje, string? campo = null) => new() { Exito = false, Mensaje = mensaje, Campo = campo };
    }

    // HU-06: indica si el usuario se eliminó o se desactivó.
    public class ResultadoEliminacion : ResultadoOperacion
    {
        public bool Desactivado { get; set; }
    }
}
