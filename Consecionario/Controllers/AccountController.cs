using System.Security.Claims;
using ConcesionarioN.DTOs;
using ConcesionarioN.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConcesionarioN.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IUsuarioService _usuarios;

        public AccountController(IUsuarioService usuarios)
        {
            _usuarios = usuarios;
        }

        // HU-01
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(dto);

            var usuario = await _usuarios.AutenticarAsync(dto.Email, dto.Password);
            if (usuario == null)
            {
                // Mensaje exacto de HU-01/CA-02 (también para cuentas desactivadas, sin revelar el motivo)
                ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos");
                dto.Password = string.Empty;
                return View(dto);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new(ClaimTypes.Name, usuario.Nombre),
                new(ClaimTypes.Email, usuario.Email),
                new(ClaimTypes.Role, usuario.Rol)
            };
            var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidad));

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // HU-02
        [HttpGet]
        public IActionResult Registro()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View(new RegistroDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var resultado = await _usuarios.RegistrarClienteAsync(dto);
            if (!resultado.Exito)
            {
                ModelState.AddModelError(resultado.Campo ?? string.Empty, resultado.Mensaje ?? "No se pudo crear la cuenta.");
                return View(dto);
            }

            TempData["Exito"] = resultado.Mensaje;
            return RedirectToAction(nameof(Login));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        // HU-03/CA-02: mensaje "Acceso denegado"
        [HttpGet]
        public IActionResult AccessDenied()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }
    }
}
