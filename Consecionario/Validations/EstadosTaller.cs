namespace ConcesionarioN.Validations
{
    // HU-21/CA-01: estados de una recepción de taller
    public static class EstadosTaller
    {
        public const string Recibido = "Recibido";
        public const string EnDiagnostico = "En diagnóstico";
        public const string EnReparacion = "En reparación";
        public const string Listo = "Listo";
        public const string Entregado = "Entregado";

        public static readonly string[] Todos = { Recibido, EnDiagnostico, EnReparacion, Listo, Entregado };
    }

    // HU-23 y HU-24: estados de una cita de ingreso al taller
    public static class EstadosCita
    {
        public const string Programada = "Programada";
        public const string Cancelada = "Cancelada";

        public static readonly string[] Todos = { Programada, Cancelada };
    }

    // Horario de atención del taller para las citas (una cita por hora)
    public static class HorarioTaller
    {
        public const int HoraInicio = 8;  // primera cita 08:00
        public const int HoraFin = 17;    // última cita 16:00

        public static bool EsDiaHabil(DateTime dia) =>
            dia.DayOfWeek != DayOfWeek.Saturday && dia.DayOfWeek != DayOfWeek.Sunday;

        public static bool EsHoraValida(int hora) => hora >= HoraInicio && hora < HoraFin;

        // Todos los horarios de un día hábil (vacío si es fin de semana)
        public static List<DateTime> HorariosDelDia(DateTime dia)
        {
            var lista = new List<DateTime>();
            if (!EsDiaHabil(dia)) return lista;
            for (var h = HoraInicio; h < HoraFin; h++) lista.Add(dia.Date.AddHours(h));
            return lista;
        }
    }
}
