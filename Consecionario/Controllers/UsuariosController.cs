using System.Security.Claims;
using ConcesionarioN.DTOs;
using ConcesionarioN.Services;
using ConcesionarioN.Validations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConcesionarioN.Controllers
{
    // HU-03/CA-02: solo Admin entra al módulo; Dependiente y Cliente reciben "Acceso denegado".
    [Authorize(Roles = RolesSistema.Admin)]
    public class UsuariosController : Controller
    {
        private const int TamanoPagina = 5;
        private readonly IUsuarioService _usuarios;

        public UsuariosController(IUsuarioService usuarios)
        {
            _usuarios = usuarios;
        }

        private int IdUsuarioActual() =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        // HU-04
        public async Task<IActionResult> Index(string? texto, string? rol, int pagina = 1)
        {
            var resultado = await _usuarios.ListarAsync(texto, rol, pagina, TamanoPagina);
            ViewBag.Texto = texto;
            ViewBag.Rol = rol;
            ViewBag.IdActual = IdUsuarioActual();
            return View(resultado);
        }

        public async Task<IActionResult> Details(int id)
        {
            var usuario = await _usuarios.ObtenerAsync(id);
            if (usuario == null) return NotFound();
            ViewBag.TieneMovimientos = await _usuarios.TieneMovimientosAsync(id);
            return View(usuario);
        }

        // HU-03
        [HttpGet]
        public IActionResult Create() => View(new UsuarioCrearDto());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioCrearDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var resultado = await _usuarios.CrearAsync(dto);
            if (!resultado.Exito)
            {
                ModelState.AddModelError(resultado.Campo ?? string.Empty, resultado.Mensaje ?? "No se pudo crear el usuario.");
                return View(dto);
            }

            TempData["Exito"] = resultado.Mensaje;
            return RedirectToAction(nameof(Index));
        }

        // HU-05
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var u = await _usuarios.ObtenerAsync(id);
            if (u == null) return NotFound();

            return View(new UsuarioEditarDto
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Email = u.Email,
                Telefono = u.Telefono,
                Rol = u.Rol
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UsuarioEditarDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var resultado = await _usuarios.ActualizarAsync(dto);
            if (!resultado.Exito)
            {
                ModelState.AddModelError(resultado.Campo ?? string.Empty, resultado.Mensaje ?? "No se pudo actualizar el usuario.");
                return View(dto);
            }

            TempData["Exito"] = resultado.Mensaje;
            return RedirectToAction(nameof(Index));
        }

        // HU-06: pantalla de confirmación
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var usuario = await _usuarios.ObtenerAsync(id);
            if (usuario == null) return NotFound();

            ViewBag.TieneMovimientos = await _usuarios.TieneMovimientosAsync(id);
            ViewBag.EsCuentaPropia = id == IdUsuarioActual();
            return View(usuario);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var resultado = await _usuarios.EliminarAsync(id, IdUsuarioActual());
            if (!resultado.Exito)
            {
                TempData["Error"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            TempData["Exito"] = resultado.Mensaje;
            return RedirectToAction(nameof(Index));
        }
    }
}
