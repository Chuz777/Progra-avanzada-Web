using System.Diagnostics;
using Consecionario.Models;
using ConcesionarioN.Data.Context;
using ConcesionarioN.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Consecionario.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ConcesionarioNDbContext _context;

        public HomeController(ILogger<HomeController> logger, ConcesionarioNDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Página pública: banner y vehículos/motos disponibles más recientes
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var modelo = new InicioDto
            {
                Vehiculos = await _context.Vehiculos.AsNoTracking()
                    .Where(v => v.Estado == "Disponible")
                    .OrderByDescending(v => v.Id)
                    .Take(4)
                    .ToListAsync(),
                Motos = await _context.Motos.AsNoTracking()
                    .Where(m => m.Estado == "Disponible")
                    .OrderByDescending(m => m.Id)
                    .Take(4)
                    .ToListAsync()
            };
            return View(modelo);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
