using System.Diagnostics;
using Consecionario.Models;
using ConcesionarioN.Data.Context;
using ConcesionarioN.DTOs;
using ConcesionarioN.Models;
using ConcesionarioN.Services;
using ConcesionarioN.Validations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConcesionarioN.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ConcesionarioNDbContext _context;
        private readonly ICatalogoService _catalogo;

        public HomeController(ILogger<HomeController> logger, ConcesionarioNDbContext context, ICatalogoService catalogo)
        {
            _logger = logger;
            _context = context;
            _catalogo = catalogo;
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
        [Authorize(Roles = "Admin,Dependiente")]
        [HttpGet]
        public IActionResult CrearItem(string tipo = "Vehiculo")
        {
            var model = new CrearProductoViewModel { TipoProducto = tipo };
            return View(model);
        }

        // POST: /Home/CrearItem  (HU-07 y HU-11)
        [Authorize(Roles = "Admin,Dependiente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearItem(CrearProductoViewModel model)
        {
            var imagen = model.ImagenUrl?.Trim() ?? string.Empty;

            if (model.TipoProducto == "Vehiculo" || model.TipoProducto == "Moto")
            {
                await ValidarUnidadAsync(model);
                if (!ModelState.IsValid) return View(model);

                var placa = _catalogo.Normalizar(model.Placa);
                var vin = _catalogo.Normalizar(model.Vin);

                if (model.TipoProducto == "Vehiculo")
                {
                    _context.Vehiculos.Add(new Vehiculo
                    {
                        Marca = model.Marca!.Trim(),
                        Modelo = model.Modelo!.Trim(),
                        Anio = model.Anio!.Value,
                        Precio = model.Precio,
                        Placa = placa,
                        Vin = vin,
                        Transmision = model.Transmision,
                        Estado = "Disponible", // HU-07/CA-01
                        ImagenUrl = imagen
                    });
                    await _context.SaveChangesAsync();
                    TempData["Exito"] = "Vehículo agregado al catálogo.";
                    return RedirectToAction("Index", "Vehiculos");
                }

                _context.Motos.Add(new Moto
                {
                    Marca = model.Marca!.Trim(),
                    Modelo = model.Modelo!.Trim(),
                    Anio = model.Anio!.Value,
                    Precio = model.Precio,
                    CilindrajeCC = model.CilindrajeCC!.Trim(),
                    Placa = placa,
                    Vin = vin,
                    Estado = "Disponible", // HU-07/CA-01
                    ImagenUrl = imagen
                });
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Moto agregada al catálogo.";
                return RedirectToAction("Index", "Motos");
            }

            if (model.TipoProducto == "Accesorio")
            {
                // HU-11/CA-02: nombre vacío, precio negativo y stock negativo se rechazan
                if (string.IsNullOrWhiteSpace(model.Nombre))
                    ModelState.AddModelError(nameof(model.Nombre), "El nombre es obligatorio.");
                else if (model.Nombre.Trim().Length > 100)
                    ModelState.AddModelError(nameof(model.Nombre), "El nombre no puede superar 100 caracteres.");

                if (string.IsNullOrWhiteSpace(model.Categoria))
                    ModelState.AddModelError(nameof(model.Categoria), "La categoría es obligatoria.");
                else if (model.Categoria.Trim().Length > 50)
                    ModelState.AddModelError(nameof(model.Categoria), "La categoría no puede superar 50 caracteres.");

                if (model.Precio < 0)
                    ModelState.AddModelError(nameof(model.Precio), "El precio no puede ser negativo.");

                if (!model.Stock.HasValue)
                    ModelState.AddModelError(nameof(model.Stock), "El stock es obligatorio.");
                else if (model.Stock.Value < 0)
                    ModelState.AddModelError(nameof(model.Stock), "El stock no puede ser negativo.");

                if (!ModelState.IsValid) return View(model);

                _context.Accesorios.Add(new Accesorio
                {
                    Nombre = model.Nombre!.Trim(),
                    Categoria = model.Categoria!.Trim(),
                    Precio = model.Precio,
                    Stock = model.Stock!.Value,
                    ImagenUrl = imagen
                });
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Accesorio agregado al inventario.";
                return RedirectToAction("Index", "Accesorios");
            }

            ModelState.AddModelError("", "Tipo de producto no válido.");
            return View(model);
        }

        // HU-07/CA-02: precio > 0, año 1990..actual+1, placa y VIN válidos y no repetidos
        private async Task ValidarUnidadAsync(CrearProductoViewModel model)
        {
            ModelState.Remove(nameof(model.Precio));
            ModelState.Remove(nameof(model.Anio));

            if (string.IsNullOrWhiteSpace(model.Marca))
                ModelState.AddModelError(nameof(model.Marca), "La marca es obligatoria.");
            if (string.IsNullOrWhiteSpace(model.Modelo))
                ModelState.AddModelError(nameof(model.Modelo), "El modelo es obligatorio.");

            if (!model.Anio.HasValue)
                ModelState.AddModelError(nameof(model.Anio), "El año es obligatorio.");
            else if (model.Anio.Value < AnioUnidadAttribute.Minimo || model.Anio.Value > AnioUnidadAttribute.Maximo)
                ModelState.AddModelError(nameof(model.Anio), AnioUnidadAttribute.Mensaje);

            if (model.Precio <= 0)
                ModelState.AddModelError(nameof(model.Precio), "El precio debe ser mayor que cero.");

            if (model.TipoProducto == "Moto" && string.IsNullOrWhiteSpace(model.CilindrajeCC))
                ModelState.AddModelError(nameof(model.CilindrajeCC), "El cilindraje es obligatorio.");

            foreach (var e in await _catalogo.ValidarIdentificadoresAsync(model.Placa, model.Vin, model.TipoProducto, null))
                ModelState.AddModelError(e.Campo, e.Mensaje);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Páginas de estado (404, etc.) usadas por UseStatusCodePagesWithReExecute
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Estado(int id)
        {
            Response.StatusCode = id;
            if (id == 404) return View("NoEncontrado");
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
