namespace ConcesionarioN.DTOs
{
    // HU-13: venta de un vehículo o una moto
    public class VentaUnidadDto
    {
        public string TipoUnidad { get; set; } = "Vehiculo"; // "Vehiculo" o "Moto"
        public int? ClienteId { get; set; }
        public int? UnidadId { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public decimal? PrecioFinal { get; set; }
        public string? Notas { get; set; }
    }

    public class LineaAccesorioDto
    {
        public int? AccesorioId { get; set; }
        public int? Cantidad { get; set; }
    }

    // HU-14: venta de uno o varios accesorios
    public class VentaAccesoriosDto
    {
        public int? ClienteId { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public string? Notas { get; set; }
        public List<LineaAccesorioDto> Lineas { get; set; } = new();
    }

    // Resultado de registrar una venta: errores por campo y, si todo salió bien, el id creado
    public class ResultadoVenta
    {
        public bool Exito => Errores.Count == 0 && VentaId.HasValue;
        public int? VentaId { get; set; }
        public List<ErrorCampo> Errores { get; set; } = new();

        public static ResultadoVenta Fallo(string campo, string mensaje)
        {
            var r = new ResultadoVenta();
            r.Errores.Add(new ErrorCampo(campo, mensaje));
            return r;
        }
    }

    // HU-15: filtros del historial
    public class FiltroVentasDto
    {
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public string? Tipo { get; set; }
        public string? Cliente { get; set; }
    }

    // HU-18: filtros del dashboard (periodo: hoy, semana, mes, anio, rango)
    public class FiltroDashboardDto
    {
        public string Periodo { get; set; } = "mes";
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public string? Tipo { get; set; }
        public int? VendedorId { get; set; }
    }

    // Unidad ofrecida en el formulario de venta
    public record UnidadVentaItem(int Id, string Tipo, string Texto, decimal Precio, string Estado);

    public class DashboardDto
    {
        public FiltroDashboardDto Filtro { get; set; } = new();
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public string? ErrorFiltro { get; set; }

        public decimal TotalVendido { get; set; }
        public int CantidadVentas { get; set; }
        public decimal TicketPromedio { get; set; }
        public string ProductoMasVendido { get; set; } = "—";
        public int UnidadesProductoMasVendido { get; set; }

        public List<string> EtiquetasGrafico { get; set; } = new();
        public List<decimal> ValoresGrafico { get; set; } = new();

        public List<ConcesionarioN.Models.Venta> Ventas { get; set; } = new();
        public List<ConcesionarioN.Models.Usuario> Vendedores { get; set; } = new();
    }
}
