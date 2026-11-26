namespace ConcesionarioN.Validations
{
    // Reglas de validación compartidas por los DTOs del módulo Usuarios.
    public static class UsuarioValidaciones
    {
        // HU-02/CA-02: mínimo 8 caracteres y al menos un número
        public const string PasswordRegex = @"^(?=.*\d).{8,}$";
        public const string PasswordMensaje = "La contraseña debe tener al menos 8 caracteres e incluir al menos un número.";

        // Formato cédula costarricense: 1-2345-0678 (guiones opcionales)
        public const string CedulaRegex = @"^\d-?\d{4}-?\d{4}$";
        public const string CedulaMensaje = "La cédula debe tener el formato 1-2345-0678.";

        // Formato teléfono: 8888-1234 (guion opcional)
        public const string TelefonoRegex = @"^\d{4}-?\d{4}$";
        public const string TelefonoMensaje = "El teléfono debe tener el formato 8888-1234.";

        public const string CorreoMensaje = "Ingrese un correo electrónico válido.";
    }
}
