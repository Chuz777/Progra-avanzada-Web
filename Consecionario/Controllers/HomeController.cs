using System.Diagnostics;
using Consecionario.Models;
using ConcesionarioN.Data.Context;
using ConcesionarioN.DTOs;
using ConcesionarioN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConcesionarioN.Controllers
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

        // GET: /Home/CrearItem?tipo=Vehiculo
        [HttpGet]
        public IActionResult CrearItem(string tipo = "Vehiculo")
        {
            var model = new CrearProductoViewModel { TipoProducto = tipo };
            return View(model);
        }

        // POST: /Home/CrearItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearItem(CrearProductoViewModel model)
        {
            if (model.TipoProducto == "Vehiculo")
            {
                if (string.IsNullOrWhiteSpace(model.Marca) || string.IsNullOrWhiteSpace(model.Modelo) || !model.Anio.HasValue)
                {
                    ModelState.AddModelError("", "Marca, Modelo y Año son obligatorios para un vehículo.");
                    return View(model);
                }

                var vehiculo = new Vehiculo
                {
                    Marca = model.Marca.Trim(),
                    Modelo = model.Modelo.Trim(),
                    Anio = model.Anio.Value,
                    Precio = model.Precio,
                    Transmision = model.Transmision,
                    Estado = model.Estado,
                    ImagenUrl = model.ImagenUrl?.Trim() ?? string.Empty
                };

                _context.Vehiculos.Add(vehiculo);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Vehículo agregado al catálogo.";
                return RedirectToAction("Index", "Vehiculos");
            }

            if (model.TipoProducto == "Moto")
            {
                if (string.IsNullOrWhiteSpace(model.Marca) || string.IsNullOrWhiteSpace(model.Modelo) || !model.Anio.HasValue || string.IsNullOrWhiteSpace(model.CilindrajeCC))
                {
                    ModelState.AddModelError("", "Marca, Modelo, Año y Cilindraje son obligatorios para una moto.");
                    return View(model);
                }

                var moto = new Moto
                {
                    Marca = model.Marca.Trim(),
                    Modelo = model.Modelo.Trim(),
                    Anio = model.Anio.Value,
                    Precio = model.Precio,
                    CilindrajeCC = model.CilindrajeCC.Trim(),
                    Estado = model.Estado,
                    ImagenUrl = model.ImagenUrl?.Trim() ?? string.Empty
                };

                _context.Motos.Add(moto);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Moto agregada al catálogo.";
                return RedirectToAction("Index", "Motos");
            }

            if (model.TipoProducto == "Accesorio")
            {
                if (string.IsNullOrWhiteSpace(model.Nombre) || !model.Stock.HasValue)
                {
                    ModelState.AddModelError("", "El Nombre y el Stock son obligatorios para un accesorio.");
                    return View(model);
                }

                var accesorio = new Accesorio
                {
                    Nombre = model.Nombre.Trim(),
                    Precio = model.Precio,
                    Stock = model.Stock.Value,
                    ImagenUrl = model.ImagenUrl?.Trim() ?? string.Empty
                };

                _context.Accesorios.Add(accesorio);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Accesorio agregado al inventario.";
                return RedirectToAction("Index", "Accesorios");
            }

            ModelState.AddModelError("", "Tipo de producto no válido.");
            return View(model);
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