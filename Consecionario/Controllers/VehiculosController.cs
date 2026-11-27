using ConcesionarioN.Data.Context;
using ConcesionarioN.DTOs;
using ConcesionarioN.Models;
using ConcesionarioN.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConcesionarioN.Controllers
{
    public class VehiculosController : Controller
    {
        private readonly ConcesionarioNDbContext _context;
        private readonly ICatalogoService _catalogo;

        public VehiculosController(ConcesionarioNDbContext context, ICatalogoService catalogo)
        {
            _context = context;
            _catalogo = catalogo;
        }

        // HU-08: catálogo público con filtros por marca, precio máximo y estado
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? marca, decimal? precioMax, string? estado)
        {
            var consulta = _context.Vehiculos.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(marca))
            {
                var m = marca.Trim();
                consulta = consulta.Where(v => v.Marca.Contains(m));
            }
            if (precioMax.HasValue) consulta = consulta.Where(v => v.Precio <= precioMax.Value);
            if (!string.IsNullOrWhiteSpace(estado)) consulta = consulta.Where(v => v.Estado == estado);

            ViewBag.Filtro = new FiltroCatalogoDto { Controlador = "Vehiculos", Marca = marca, PrecioMax = precioMax, Estado = estado };
            return View(await consulta.OrderByDescending(v => v.Id).ToListAsync());
        }

        // El registro se hace en un único formulario (Home/CrearItem)
        [Authorize(Roles = "Admin,Dependiente")]
        public IActionResult Create() => RedirectToAction("CrearItem", "Home", new { tipo = "Vehiculo" });

        [Authorize(Roles = "Admin,Dependiente")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var vehiculo = await _context.Vehiculos.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
            if (vehiculo == null) return NotFound();
            return View(vehiculo);
        }

        // HU-09
        [Authorize(Roles = "Admin,Dependiente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Vehiculo vehiculo)
        {
            if (id != vehiculo.Id) return NotFound();
            if (!await _context.Vehiculos.AnyAsync(v => v.Id == id)) return NotFound();

            ModelState.Remove(nameof(Vehiculo.ImagenUrl));
            vehiculo.ImagenUrl = vehiculo.ImagenUrl?.Trim() ?? string.Empty;
            vehiculo.Placa = _catalogo.Normalizar(vehiculo.Placa);
            vehiculo.Vin = _catalogo.Normalizar(vehiculo.Vin);

            foreach (var e in await _catalogo.ValidarIdentificadoresAsync(vehiculo.Placa, vehiculo.Vin, "Vehiculo", id))
                ModelState.AddModelError(e.Campo, e.Mensaje);

            // HU-09/CA-02: no se puede marcar Disponible una unidad vendida o con reserva activa
            if (vehiculo.Estado == "Disponible" && await _catalogo.VehiculoVendidoOReservadoAsync(id))
                ModelState.AddModelError(nameof(Vehiculo.Estado),
                    "No se puede marcar como Disponible una unidad vendida o con reserva activa.");

            if (!ModelState.IsValid) return View(vehiculo);

            _context.Update(vehiculo);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Vehículo actualizado correctamente.";
            return RedirectToAction(nameof(Detalles), new { id });
        }

        // HU-08/CA-02: si el código no existe, se muestra la página "No encontrado"
        [AllowAnonymous]
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null) return NotFound();
            var vehiculo = await _context.Vehiculos.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (vehiculo == null) return NotFound();
            return View(vehiculo);
        }

        // HU-10: solo Admin; se bloquea si la unidad tiene ventas o reservas
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null) return NotFound();

            if (await _catalogo.VehiculoTieneMovimientosAsync(id))
            {
                TempData["Error"] = "No se puede eliminar el vehículo porque tiene ventas o reservas asociadas.";
                return RedirectToAction(nameof(Index));
            }

            _context.Vehiculos.Remove(vehiculo);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Vehículo eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
