using ConcesionarioN.Data.Context;
using ConcesionarioN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConcesionarioN.Controllers
{
    public class AccesoriosController : Controller
    {
        private readonly ConcesionarioNDbContext _context;

        public AccesoriosController(ConcesionarioNDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Accesorios.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Accesorio accesorio)
        {
            if (ModelState.IsValid)
            {
                _context.Add(accesorio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(accesorio);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var accesorio = await _context.Accesorios.FindAsync(id);
            if (accesorio == null) return NotFound();
            return View(accesorio);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Accesorio accesorio)
        {
            if (id != accesorio.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(accesorio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(accesorio);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var accesorio = await _context.Accesorios.FirstOrDefaultAsync(m => m.Id == id);
            if (accesorio == null) return NotFound();
            return View(accesorio);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var accesorio = await _context.Accesorios.FindAsync(id);
            if (accesorio != null)
            {
                _context.Accesorios.Remove(accesorio);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}