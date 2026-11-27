using ConcesionarioN.Data.Context;
using ConcesionarioN.DTOs;
using ConcesionarioN.Models;
using ConcesionarioN.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConcesionarioN.Controllers
{
    public class MotosController : Controller
    {
        private readonly ConcesionarioNDbContext _context;
        private readonly ICatalogoService _catalogo;

        public MotosController(ConcesionarioNDbContext context, ICatalogoService catalogo)
        {
            _context = context;
            _catalogo = catalogo;
        }

        // HU-08: catálogo público con filtros por marca, precio máximo y estado
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? marca, decimal? precioMax, string? estado)
        {
            var consulta = _context.Motos.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(marca))
            {
                var m = marca.Trim();
                consulta = consulta.Where(x => x.Marca.Contains(m));
            }
            if (precioMax.HasValue) consulta = consulta.Where(x => x.Precio <= precioMax.Value);
            if (!string.IsNullOrWhiteSpace(estado)) consulta = consulta.Where(x => x.Estado == estado);

            ViewBag.Filtro = new FiltroCatalogoDto { Controlador = "Motos", Marca = marca, PrecioMax = precioMax, Estado = estado };
            return View(await consulta.OrderByDescending(x => x.Id).ToListAsync());
        }

        // El registro se hace en un único formulario (Home/CrearItem)
        [Authorize(Roles = "Admin,Dependiente")]
        public IActionResult Create() => RedirectToAction("CrearItem", "Home", new { tipo = "Moto" });

        [Authorize(Roles = "Admin,Dependiente")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var moto = await _context.Motos.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (moto == null) return NotFound();
            return View(moto);
        }

        // HU-09
        [Authorize(Roles = "Admin,Dependiente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Moto moto)
        {
            if (id != moto.Id) return NotFound();
            if (!await _context.Motos.AnyAsync(m => m.Id == id)) return NotFound();

            ModelState.Remove(nameof(Moto.ImagenUrl));
            moto.ImagenUrl = moto.ImagenUrl?.Trim() ?? string.Empty;
            moto.Placa = _catalogo.Normalizar(moto.Placa);
            moto.Vin = _catalogo.Normalizar(moto.Vin);

            foreach (var e in await _catalogo.ValidarIdentificadoresAsync(moto.Placa, moto.Vin, "Moto", id))
                ModelState.AddModelError(e.Campo, e.Mensaje);

            // HU-09/CA-02: no se puede marcar Disponible una unidad vendida
            if (moto.Estado == "Disponible" && await _catalogo.MotoVendidaAsync(id))
                ModelState.AddModelError(nameof(Moto.Estado),
                    "No se puede marcar como Disponible una unidad vendida o con reserva activa.");

            if (!ModelState.IsValid) return View(moto);

            _context.Update(moto);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Moto actualizada correctamente.";
            return RedirectToAction(nameof(Detalles), new { id });
        }

        // HU-08/CA-02: si el código no existe, se muestra la página "No encontrado"
        [AllowAnonymous]
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null) return NotFound();
            var moto = await _context.Motos.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (moto == null) return NotFound();
            return View(moto);
        }

        // HU-10: solo Admin; se bloquea si la unidad tiene ventas asociadas
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var moto = await _context.Motos.FindAsync(id);
            if (moto == null) return NotFound();

            if (await _catalogo.MotoTieneMovimientosAsync(id))
            {
                TempData["Error"] = "No se puede eliminar la moto porque tiene ventas o reservas asociadas.";
                return RedirectToAction(nameof(Index));
            }

            _context.Motos.Remove(moto);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Moto eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
