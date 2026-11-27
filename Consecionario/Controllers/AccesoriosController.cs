using ConcesionarioN.Data.Context;
using ConcesionarioN.Models;
using ConcesionarioN.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConcesionarioN.Controllers
{
    public class AccesoriosController : Controller
    {
        private readonly ConcesionarioNDbContext _context;
        private readonly ICatalogoService _catalogo;

        public AccesoriosController(ConcesionarioNDbContext context, ICatalogoService catalogo)
        {
            _context = context;
            _catalogo = catalogo;
        }

        // HU-11: listado con nombre, categoría, precio y stock
        public async Task<IActionResult> Index()
        {
            return View(await _context.Accesorios.AsNoTracking().OrderBy(a => a.Nombre).ToListAsync());
        }

        // El registro se hace en un único formulario (Home/CrearItem)
        [Authorize(Roles = "Admin,Dependiente")]
        public IActionResult Create() => RedirectToAction("CrearItem", "Home", new { tipo = "Accesorio" });

        [Authorize(Roles = "Admin,Dependiente")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var accesorio = await _context.Accesorios.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            if (accesorio == null) return NotFound();
            return View(accesorio);
        }

        // HU-12/CA-01
        [Authorize(Roles = "Admin,Dependiente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Accesorio accesorio)
        {
            if (id != accesorio.Id) return NotFound();
            if (!await _context.Accesorios.AnyAsync(a => a.Id == id)) return NotFound();

            ModelState.Remove(nameof(Accesorio.ImagenUrl));
            accesorio.ImagenUrl = accesorio.ImagenUrl?.Trim() ?? string.Empty;

            if (!ModelState.IsValid) return View(accesorio);

            _context.Update(accesorio);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Accesorio actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null) return NotFound();

            var accesorio = await _context.Accesorios.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
            if (accesorio == null) return NotFound();

            return View(accesorio);
        }

        // La eliminación se confirma desde el listado (POST); un GET vuelve al listado
        [Authorize(Roles = "Admin,Dependiente")]
        public IActionResult Delete(int? id) => RedirectToAction(nameof(Index));

        // HU-12/CA-02: se bloquea si el accesorio tiene ventas
        [Authorize(Roles = "Admin,Dependiente")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var accesorio = await _context.Accesorios.FindAsync(id);
            if (accesorio == null) return NotFound();

            if (await _catalogo.AccesorioTieneVentasAsync(id))
            {
                TempData["Error"] = "No se puede eliminar el accesorio porque tiene ventas asociadas.";
                return RedirectToAction(nameof(Index));
            }

            _context.Accesorios.Remove(accesorio);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Accesorio eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
