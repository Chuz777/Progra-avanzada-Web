using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConcesionarioN.Data.Context;
using ConcesionarioN.Models;

namespace ConcesionarioN.Controllers
{
    public class MotosController : Controller
    {
        private readonly ConcesionarioNDbContext _context;

        public MotosController(ConcesionarioNDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index() => View(await _context.Motos.ToListAsync());

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Moto moto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(moto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(moto);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var moto = await _context.Motos.FindAsync(id);
            if (moto == null) return NotFound();
            return View(moto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Moto moto)
        {
            if (id != moto.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(moto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(moto);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var moto = await _context.Motos.FirstOrDefaultAsync(m => m.Id == id);
            if (moto == null) return NotFound();
            return View(moto);
        }
    }
}