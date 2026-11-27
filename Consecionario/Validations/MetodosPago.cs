namespace ConcesionarioN.Validations
{
    public static class MetodosPago
    {
        public const string Efectivo = "Efectivo";
        public const string Transferencia = "Transferencia";
        public const string Tarjeta = "Tarjeta";
        public const string Financiamiento = "Financiamiento";

        public static readonly string[] Todos = { Efectivo, Transferencia, Tarjeta, Financiamiento };
    }
}
