using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConcesionarioN.Data.Context;
using ConcesionarioN.Models;

namespace ConcesionarioN.Controllers
{
    public class VehiculosController : Controller
    {
        private readonly ConcesionarioNDbContext _context;

        public VehiculosController(ConcesionarioNDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index() => View(await _context.Vehiculos.ToListAsync());

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vehiculo vehiculo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vehiculo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vehiculo);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null) return NotFound();
            return View(vehiculo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Vehiculo vehiculo)
        {
            if (id != vehiculo.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(vehiculo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vehiculo);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var vehiculo = await _context.Vehiculos.FirstOrDefaultAsync(m => m.Id == id);
            if (vehiculo == null) return NotFound();
            return View(vehiculo);
        }
    }
}