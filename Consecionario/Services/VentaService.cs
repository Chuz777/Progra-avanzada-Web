using System.Globalization;
using System.Text;
using ConcesionarioN.Data.Context;
using ConcesionarioN.DTOs;
using ConcesionarioN.Models;
using ConcesionarioN.Validations;
using Microsoft.EntityFrameworkCore;

namespace ConcesionarioN.Services
{
    public class VentaService : IVentaService
    {
        private readonly ConcesionarioNDbContext _context;

        public VentaService(ConcesionarioNDbContext context)
        {
            _context = context;
        }

        // ---------- Datos para los formularios ----------

        public Task<List<Usuario>> ClientesActivosAsync() =>
            _context.Usuarios.AsNoTracking()
                .Where(u => u.Rol == RolesSistema.Cliente && u.Activo)
                .OrderBy(u => u.Nombre)
                .ToListAsync();

        public Task<List<Accesorio>> AccesoriosAsync() =>
            _context.Accesorios.AsNoTracking().OrderBy(a => a.Nombre).ToListAsync();

        public async Task<List<UnidadVentaItem>> UnidadesAsync()
        {
            var vehiculos = await _context.Vehiculos.AsNoTracking().OrderBy(v => v.Marca).ThenBy(v => v.Modelo).ToListAsync();
            var motos = await _context.Motos.AsNoTracking().OrderBy(m => m.Marca).ThenBy(m => m.Modelo).ToListAsync();

            var items = new List<UnidadVentaItem>();
            items.AddRange(vehiculos.Select(v => new UnidadVentaItem(v.Id, "Vehiculo", $"{v.Marca} {v.Modelo} {v.Anio} · {v.Placa ?? "sin placa"} — {v.Estado}", v.Precio, v.Estado)));
            items.AddRange(motos.Select(m => new UnidadVentaItem(m.Id, "Moto", $"{m.Marca} {m.Modelo} {m.Anio} · {m.Placa ?? "sin placa"} — {m.Estado}", m.Precio, m.Estado)));
            return items;
        }

        // ---------- HU-13: vender unidad ----------

        public async Task<ResultadoVenta> RegistrarUnidadAsync(VentaUnidadDto dto, int vendedorId)
        {
            var errores = new List<ErrorCampo>();

            if (dto.TipoUnidad != "Vehiculo" && dto.TipoUnidad != "Moto")
                return ResultadoVenta.Fallo(nameof(dto.TipoUnidad), "Seleccione si vende un vehículo o una moto.");

            var cliente = dto.ClienteId.HasValue
                ? await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == dto.ClienteId && u.Rol == RolesSistema.Cliente && u.Activo)
                : null;
            if (cliente == null) errores.Add(new ErrorCampo(nameof(dto.ClienteId), "Seleccione un cliente registrado."));

            if (!dto.UnidadId.HasValue) errores.Add(new ErrorCampo(nameof(dto.UnidadId), "Seleccione una unidad."));

            if (!MetodosPago.Todos.Contains(dto.MetodoPago))
                errores.Add(new ErrorCampo(nameof(dto.MetodoPago), "Seleccione el método de pago."));

            if (!dto.PrecioFinal.HasValue || dto.PrecioFinal.Value <= 0)
                errores.Add(new ErrorCampo(nameof(dto.PrecioFinal), "El precio final debe ser mayor que cero."));

            if (errores.Count > 0) return new ResultadoVenta { Errores = errores };

            await using var tx = await _context.Database.BeginTransactionAsync();

            var venta = new Venta
            {
                FechaVenta = DateTime.Now,
                Tipo = dto.TipoUnidad,
                ClienteId = cliente!.Id,
                ClienteNombre = cliente.Nombre,
                VendedorId = vendedorId,
                MetodoPago = dto.MetodoPago,
                Notas = string.IsNullOrWhiteSpace(dto.Notas) ? null : dto.Notas.Trim(),
                TotalVenta = dto.PrecioFinal!.Value
            };

            if (dto.TipoUnidad == "Vehiculo")
            {
                var vehiculo = await _context.Vehiculos.FirstOrDefaultAsync(v => v.Id == dto.UnidadId);
                if (vehiculo == null) return ResultadoVenta.Fallo(nameof(dto.UnidadId), "La unidad seleccionada no existe.");

                var yaVendido = vehiculo.Estado == "Vendido"
                    || await _context.Ventas.AnyAsync(v => v.VehiculoId == vehiculo.Id && !v.Anulada);
                if (yaVendido)
                    return ResultadoVenta.Fallo(nameof(dto.UnidadId), "La unidad ya fue vendida.");

                var reservas = await _context.Reservas
                    .Where(r => r.VehiculoId == vehiculo.Id && (r.EstadoReserva == "Activa" || r.EstadoReserva == "En proceso"))
                    .ToListAsync();

                // HU-13/CA-02: no se vende una unidad reservada por otro cliente
                if (reservas.Any(r => r.ClienteNombre != cliente.Nombre))
                    return ResultadoVenta.Fallo(nameof(dto.UnidadId), "La unidad está reservada por otro cliente.");

                if (vehiculo.Estado == "Reservado" && reservas.Count == 0)
                    return ResultadoVenta.Fallo(nameof(dto.UnidadId), "La unidad está reservada y no se puede vender.");

                if (vehiculo.Estado != "Disponible" && vehiculo.Estado != "Reservado")
                    return ResultadoVenta.Fallo(nameof(dto.UnidadId), "La unidad no está disponible para la venta.");

                venta.VehiculoId = vehiculo.Id;
                venta.Descripcion = $"{vehiculo.Marca} {vehiculo.Modelo} {vehiculo.Anio}" + (string.IsNullOrEmpty(vehiculo.Placa) ? "" : $" ({vehiculo.Placa})");
                vehiculo.Estado = "Vendido";

                // Si el comprador es quien reservó, la reserva se concreta
                foreach (var r in reservas) r.EstadoReserva = "Concretada";
            }
            else
            {
                var moto = await _context.Motos.FirstOrDefaultAsync(m => m.Id == dto.UnidadId);
                if (moto == null) return ResultadoVenta.Fallo(nameof(dto.UnidadId), "La unidad seleccionada no existe.");

                var yaVendida = moto.Estado == "Vendido"
                    || await _context.Ventas.AnyAsync(v => v.MotoId == moto.Id && !v.Anulada);
                if (yaVendida)
                    return ResultadoVenta.Fallo(nameof(dto.UnidadId), "La unidad ya fue vendida.");

                if (moto.Estado != "Disponible")
                    return ResultadoVenta.Fallo(nameof(dto.UnidadId), "La unidad está reservada o no disponible para la venta.");

                venta.MotoId = moto.Id;
                venta.Descripcion = $"{moto.Marca} {moto.Modelo} {moto.Anio}" + (string.IsNullOrEmpty(moto.Placa) ? "" : $" ({moto.Placa})");
                moto.Estado = "Vendido";
            }

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            return new ResultadoVenta { VentaId = venta.Id };
        }

        // ---------- HU-14: vender accesorios ----------

        public async Task<ResultadoVenta> RegistrarAccesoriosAsync(VentaAccesoriosDto dto, int vendedorId)
        {
            var errores = new List<ErrorCampo>();

            var cliente = dto.ClienteId.HasValue
                ? await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == dto.ClienteId && u.Rol == RolesSistema.Cliente && u.Activo)
                : null;
            if (cliente == null) errores.Add(new ErrorCampo(nameof(dto.ClienteId), "Seleccione un cliente registrado."));

            if (!MetodosPago.Todos.Contains(dto.MetodoPago))
                errores.Add(new ErrorCampo(nameof(dto.MetodoPago), "Seleccione el método de pago."));

            if (dto.Lineas.Count == 0)
                errores.Add(new ErrorCampo("Lineas", "Agregue al menos un accesorio."));

            // HU-14/CA-02: cada línea necesita accesorio y cantidad mayor que cero
            for (var i = 0; i < dto.Lineas.Count; i++)
            {
                var l = dto.Lineas[i];
                if (!l.AccesorioId.HasValue)
                    errores.Add(new ErrorCampo($"Lineas[{i}].AccesorioId", "Seleccione un accesorio."));
                if (!l.Cantidad.HasValue || l.Cantidad.Value <= 0)
                    errores.Add(new ErrorCampo($"Lineas[{i}].Cantidad", "La cantidad debe ser mayor que cero."));
            }
            if (errores.Count > 0) return new ResultadoVenta { Errores = errores };

            await using var tx = await _context.Database.BeginTransactionAsync();

            var ids = dto.Lineas.Select(l => l.AccesorioId!.Value).Distinct().ToList();
            var accesorios = await _context.Accesorios.Where(a => ids.Contains(a.Id)).ToDictionaryAsync(a => a.Id);

            // Cantidad total pedida por accesorio (si se repite en varias líneas) contra el stock disponible
            var pedido = dto.Lineas.GroupBy(l => l.AccesorioId!.Value).ToDictionary(g => g.Key, g => g.Sum(l => l.Cantidad!.Value));
            for (var i = 0; i < dto.Lineas.Count; i++)
            {
                var id = dto.Lineas[i].AccesorioId!.Value;
                if (!accesorios.TryGetValue(id, out var acc))
                {
                    errores.Add(new ErrorCampo($"Lineas[{i}].AccesorioId", "El accesorio no existe."));
                    continue;
                }
                if (pedido[id] > acc.Stock)
                    errores.Add(new ErrorCampo($"Lineas[{i}].Cantidad", $"Solo hay {acc.Stock} unidad(es) disponibles de {acc.Nombre}."));
            }
            if (errores.Count > 0) return new ResultadoVenta { Errores = errores };

            var venta = new Venta
            {
                FechaVenta = DateTime.Now,
                Tipo = "Accesorio",
                ClienteId = cliente!.Id,
                ClienteNombre = cliente.Nombre,
                VendedorId = vendedorId,
                MetodoPago = dto.MetodoPago,
                Notas = string.IsNullOrWhiteSpace(dto.Notas) ? null : dto.Notas.Trim()
            };

            foreach (var l in dto.Lineas)
            {
                var acc = accesorios[l.AccesorioId!.Value];
                var cantidad = l.Cantidad!.Value;
                venta.Lineas.Add(new VentaLinea
                {
                    AccesorioId = acc.Id,
                    Cantidad = cantidad,
                    PrecioUnitario = acc.Precio,
                    Subtotal = acc.Precio * cantidad
                });
                acc.Stock -= cantidad; // se descuenta el stock
            }

            venta.TotalVenta = venta.Lineas.Sum(l => l.Subtotal);
            venta.Descripcion = string.Join(", ", dto.Lineas
                .GroupBy(l => l.AccesorioId!.Value)
                .Select(g => $"{accesorios[g.Key].Nombre} x{g.Sum(l => l.Cantidad!.Value)}"));

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            return new ResultadoVenta { VentaId = venta.Id };
        }

        // ---------- HU-15: historial ----------

        private IQueryable<Venta> ConsultaBase() =>
            _context.Ventas.AsNoTracking()
                .Include(v => v.Vendedor)
                .Include(v => v.Lineas).ThenInclude(l => l.Accesorio);

        public async Task<List<Venta>> ListarAsync(FiltroVentasDto filtro, int? soloClienteId)
        {
            var q = ConsultaBase();

            if (soloClienteId.HasValue) q = q.Where(v => v.ClienteId == soloClienteId.Value);
            if (filtro.Desde.HasValue) q = q.Where(v => v.FechaVenta >= filtro.Desde.Value.Date);
            if (filtro.Hasta.HasValue) q = q.Where(v => v.FechaVenta < filtro.Hasta.Value.Date.AddDays(1));
            if (!string.IsNullOrWhiteSpace(filtro.Tipo)) q = q.Where(v => v.Tipo == filtro.Tipo);
            if (!string.IsNullOrWhiteSpace(filtro.Cliente))
            {
                var c = filtro.Cliente.Trim();
                q = q.Where(v => v.ClienteNombre.Contains(c));
            }

            return await q.OrderByDescending(v => v.FechaVenta).ThenByDescending(v => v.Id).ToListAsync();
        }

        public Task<Venta?> ObtenerAsync(int id) =>
            ConsultaBase().Include(v => v.Cliente).FirstOrDefaultAsync(v => v.Id == id);

        // ---------- HU-16 ----------

        public async Task<ResultadoOperacion> EditarAsync(int id, string metodoPago, string? notas)
        {
            if (!MetodosPago.Todos.Contains(metodoPago))
                return ResultadoOperacion.Error("Seleccione el método de pago.", "MetodoPago");

            var venta = await _context.Ventas.FirstOrDefaultAsync(v => v.Id == id);
            if (venta == null) return ResultadoOperacion.Error("La venta no existe.");
            if (venta.Anulada) return ResultadoOperacion.Error("No se puede editar una venta anulada.");

            venta.MetodoPago = metodoPago;
            venta.Notas = string.IsNullOrWhiteSpace(notas) ? null : notas.Trim();
            await _context.SaveChangesAsync();
            return ResultadoOperacion.Ok("Venta actualizada correctamente.");
        }

        public async Task<ResultadoOperacion> AnularAsync(int id)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();

            var venta = await _context.Ventas
                .Include(v => v.Lineas).ThenInclude(l => l.Accesorio)
                .Include(v => v.Vehiculo)
                .Include(v => v.Moto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null) return ResultadoOperacion.Error("La venta no existe.");
            if (venta.Anulada) return ResultadoOperacion.Error("La venta ya está anulada.");

            // La unidad vuelve a Disponible y el stock de accesorios se restituye
            if (venta.Vehiculo != null && venta.Vehiculo.Estado == "Vendido") venta.Vehiculo.Estado = "Disponible";
            if (venta.Moto != null && venta.Moto.Estado == "Vendido") venta.Moto.Estado = "Disponible";
            foreach (var l in venta.Lineas)
                if (l.Accesorio != null) l.Accesorio.Stock += l.Cantidad;

            venta.Anulada = true;
            venta.FechaAnulacion = DateTime.Now;

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            return ResultadoOperacion.Ok("Venta anulada. La unidad o el stock se restituyeron.");
        }

        // ---------- HU-17, HU-18: dashboard ----------

        // Devuelve el rango de fechas del periodo; error si el rango personalizado no es válido
        private static (DateTime Desde, DateTime Hasta, string? Error) ResolverRango(FiltroDashboardDto f)
        {
            var hoy = DateTime.Today;
            switch (f.Periodo)
            {
                case "hoy":
                    return (hoy, hoy, null);
                case "semana":
                    var lunes = hoy.AddDays(-(((int)hoy.DayOfWeek + 6) % 7));
                    return (lunes, lunes.AddDays(6), null);
                case "anio":
                    return (new DateTime(hoy.Year, 1, 1), new DateTime(hoy.Year, 12, 31), null);
                case "rango":
                    if (!f.Desde.HasValue || !f.Hasta.HasValue)
                        return (hoy, hoy, "Indique la fecha inicial y la final del rango.");
                    if (f.Desde.Value.Date > f.Hasta.Value.Date)
                        return (f.Desde.Value.Date, f.Hasta.Value.Date, "La fecha inicial no puede ser posterior a la final.");
                    return (f.Desde.Value.Date, f.Hasta.Value.Date, null);
                default: // mes
                    var inicio = new DateTime(hoy.Year, hoy.Month, 1);
                    return (inicio, inicio.AddMonths(1).AddDays(-1), null);
            }
        }

        private async Task<(List<Venta> Ventas, DateTime Desde, DateTime Hasta, string? Error)> VentasFiltradasAsync(FiltroDashboardDto f)
        {
            var (desde, hasta, error) = ResolverRango(f);
            if (error != null) return (new List<Venta>(), desde, hasta, error);

            var q = ConsultaBase()
                .Include(v => v.Vehiculo)
                .Include(v => v.Moto)
                .Where(v => !v.Anulada && v.FechaVenta >= desde && v.FechaVenta < hasta.AddDays(1));

            if (!string.IsNullOrWhiteSpace(f.Tipo)) q = q.Where(v => v.Tipo == f.Tipo);
            if (f.VendedorId.HasValue) q = q.Where(v => v.VendedorId == f.VendedorId.Value);

            var ventas = await q.OrderByDescending(v => v.FechaVenta).ThenByDescending(v => v.Id).ToListAsync();
            return (ventas, desde, hasta, null);
        }

        public async Task<DashboardDto> DashboardAsync(FiltroDashboardDto filtro)
        {
            var (ventas, desde, hasta, error) = await VentasFiltradasAsync(filtro);

            var dto = new DashboardDto
            {
                Filtro = filtro,
                Desde = desde,
                Hasta = hasta,
                ErrorFiltro = error,
                Ventas = ventas,
                Vendedores = await _context.Usuarios.AsNoTracking()
                    .Where(u => u.Rol == RolesSistema.Admin || u.Rol == RolesSistema.Dependiente)
                    .OrderBy(u => u.Nombre).ToListAsync()
            };

            dto.CantidadVentas = ventas.Count;
            dto.TotalVendido = ventas.Sum(v => v.TotalVenta);
            dto.TicketPromedio = ventas.Count == 0 ? 0 : Math.Round(dto.TotalVendido / ventas.Count, 2);

            // Producto más vendido, en unidades: cada unidad vendida cuenta 1; los accesorios cuentan su cantidad
            var conteo = new Dictionary<string, int>();
            void Sumar(string nombre, int cantidad) => conteo[nombre] = conteo.GetValueOrDefault(nombre) + cantidad;
            foreach (var v in ventas)
            {
                if (v.Vehiculo != null) Sumar($"{v.Vehiculo.Marca} {v.Vehiculo.Modelo}", 1);
                else if (v.Moto != null) Sumar($"{v.Moto.Marca} {v.Moto.Modelo}", 1);
                foreach (var l in v.Lineas)
                    if (l.Accesorio != null) Sumar(l.Accesorio.Nombre, l.Cantidad);
            }
            if (conteo.Count > 0)
            {
                var top = conteo.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key).First();
                dto.ProductoMasVendido = top.Key;
                dto.UnidadesProductoMasVendido = top.Value;
            }

            // Gráfico: total vendido por día (o por mes si el rango supera ~2 meses)
            var porMes = (hasta - desde).TotalDays > 62;
            var grupos = ventas
                .GroupBy(v => porMes ? new DateTime(v.FechaVenta.Year, v.FechaVenta.Month, 1) : v.FechaVenta.Date)
                .OrderBy(g => g.Key);
            foreach (var g in grupos)
            {
                dto.EtiquetasGrafico.Add(g.Key.ToString(porMes ? "MM/yyyy" : "dd/MM", CultureInfo.InvariantCulture));
                dto.ValoresGrafico.Add(g.Sum(v => v.TotalVenta));
            }

            return dto;
        }

        // ---------- HU-19: CSV ----------

        private static string Csv(string? valor)
        {
            var v = valor ?? string.Empty;
            // Evita que Excel interprete el texto como fórmula
            if (v.Length > 0 && "=+-@".Contains(v[0])) v = "'" + v;
            return "\"" + v.Replace("\"", "\"\"") + "\"";
        }

        public async Task<byte[]> ExportarCsvAsync(FiltroDashboardDto filtro)
        {
            var (ventas, _, _, _) = await VentasFiltradasAsync(filtro);
            var nl = "\r\n";
            var sb = new StringBuilder();
            sb.Append("Fecha;Cliente;Tipo;Producto;Método de pago;Vendedor;Total").Append(nl);

            foreach (var v in ventas)
            {
                sb.Append(string.Join(';',
                    Csv(v.FechaVenta.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)),
                    Csv(v.ClienteNombre),
                    Csv(v.Tipo),
                    Csv(v.Descripcion),
                    Csv(v.MetodoPago),
                    Csv(v.Vendedor?.Nombre),
                    v.TotalVenta.ToString("0.00", CultureInfo.InvariantCulture))).Append(nl);
            }

            var total = ventas.Sum(v => v.TotalVenta);
            sb.Append(string.Join(';', "\"TOTAL\"", "\"\"", "\"\"", Csv($"{ventas.Count} venta(s)"), "\"\"", "\"\"",
                total.ToString("0.00", CultureInfo.InvariantCulture))).Append(nl);

            var preamble = new UTF8Encoding(true).GetPreamble();
            var cuerpo = Encoding.UTF8.GetBytes(sb.ToString());
            return preamble.Concat(cuerpo).ToArray();
        }
    }
}
