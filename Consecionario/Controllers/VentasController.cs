using ConcesionarioN.Data.Context;
using ConcesionarioN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConcesionarioN.Controllers
{
    public class VentasController : Controller
    {
        private readonly ConcesionarioNDbContext _context;

        public VentasController(ConcesionarioNDbContext context)
        {
            _context = context;
        }

        // Listado de Ventas (CRUD - Index)
        public async Task<IActionResult> Index()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Vehiculo)
                .Include(v => v.Accesorio)
                .ToListAsync();
            return View(ventas);
        }

        // GET: Ventas/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Vehiculos = await _context.Vehiculos.Where(v => v.Estado == "Disponible").ToListAsync();
            ViewBag.Accesorios = await _context.Accesorios.Where(a => a.Stock > 0).ToListAsync();
            return View();
        }

        // POST: Ventas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venta venta)
        {
            if (ModelState.IsValid)
            {
                venta.FechaVenta = DateTime.Now;
                _context.Add(venta);

                // Si vendió un vehículo, actualizar su estado a Vendido
                if (venta.VehiculoId.HasValue)
                {
                    var vehiculo = await _context.Vehiculos.FindAsync(venta.VehiculoId.Value);
                    if (vehiculo != null) vehiculo.Estado = "Vendido";
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(venta);
        }

        // REQUERIMIENTO ESPECIAL: Dashboard con Estadísticas y Filtros por Fechas
        public async Task<IActionResult> Dashboard(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var ventasQuery = _context.Ventas
                .Include(v => v.Vehiculo)
                .Include(v => v.Accesorio)
                .AsQueryable();

            // Aplicar filtros por período si el usuario los selecciona
            if (fechaInicio.HasValue)
            {
                ventasQuery = ventasQuery.Where(v => v.FechaVenta >= fechaInicio.Value);
            }
            if (fechaFin.HasValue)
            {
                ventasQuery = ventasQuery.Where(v => v.FechaVenta <= fechaFin.Value.AddDays(1));
            }

            var ventasFiltradas = await ventasQuery.ToListAsync();

            // Cálculo de Indicadores Clave (KPIs) para el Dashboard
            ViewBag.TotalIngresos = ventasFiltradas.Sum(v => v.TotalVenta);
            ViewBag.CantidadVentas = ventasFiltradas.Count;
            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");

            return View(ventasFiltradas);
        }
    }
}