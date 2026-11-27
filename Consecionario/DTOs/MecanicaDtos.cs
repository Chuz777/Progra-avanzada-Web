using ConcesionarioN.Models;

namespace ConcesionarioN.DTOs
{
    // HU-20: recepción de un vehículo en el taller
    public class RecepcionCrearDto
    {
        public int? ClienteId { get; set; }
        public string PlacaVehiculo { get; set; } = string.Empty;
        public string? EstadoVehiculo { get; set; }
        public string DescripcionProblema { get; set; } = string.Empty;
    }

    // HU-21/CA-01: filtros del listado de recepciones
    public class FiltroRecepcionesDto
    {
        public string? Estado { get; set; }
        public string? Placa { get; set; }
    }

    // HU-23: agendar cita de ingreso (HU-24: también se usa al reprogramar)
    public class CitaDto
    {
        public int? ClienteId { get; set; }
        public string PlacaVehiculo { get; set; } = string.Empty;
        public string? Motivo { get; set; }
        public DateTime? Fecha { get; set; }
        public int? Hora { get; set; }
    }

    // Horario del calendario: libre u ocupado
    public record HorarioItem(DateTime FechaHora, bool Libre);

    // HU-24/CA-01: agenda por día o por semana
    public class AgendaDto
    {
        public string Vista { get; set; } = "dia"; // "dia" o "semana"
        public DateTime Fecha { get; set; }
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public List<CitaIngreso> Citas { get; set; } = new();
    }

    // Resultado de registrar una recepción o una cita: errores por campo y el id creado
    public class ResultadoMecanica
    {
        public bool Exito => Errores.Count == 0 && Id.HasValue;
        public int? Id { get; set; }
        public List<ErrorCampo> Errores { get; set; } = new();

        public static ResultadoMecanica Fallo(string campo, string mensaje)
        {
            var r = new ResultadoMecanica();
            r.Errores.Add(new ErrorCampo(campo, mensaje));
            return r;
        }
    }
}
