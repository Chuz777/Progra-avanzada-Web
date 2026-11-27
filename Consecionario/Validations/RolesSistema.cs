namespace ConcesionarioN.Validations
{
    public static class RolesSistema
    {
        public const string Admin = "Admin";
        public const string Dependiente = "Dependiente";
        public const string Cliente = "Cliente";

        // Roles que un Admin puede asignar al crear personal (HU-03)
        public static readonly string[] Personal = { Admin, Dependiente };

        // Todos los roles (filtro de listado y edición)
        public static readonly string[] Todos = { Admin, Dependiente, Cliente };
    }
}
