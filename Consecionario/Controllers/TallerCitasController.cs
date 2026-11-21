using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConcesionarioN.Data.Context;
using ConcesionarioN.Models;

namespace ConcesionarioN.Controllers
{
    public class TallerCitasController : Controller
    {
        private readonly ConcesionarioNDbContext _context;

        public TallerCitasController(ConcesionarioNDbContext context)
        {
            _context = context;
        }

        // GET: TallerCitas/Create
        public async Task<IActionResult> Create()
        {
            // Simulamos la verificación de sesión mediante un ID guardado en Cookies/Session (ej: UsuarioId = 1)
            int? usuarioLogueadoId = HttpContext.Session.GetInt32("UsuarioId");

            var cita = new TallerCita();

            if (usuarioLogueadoId.HasValue)
            {
                // Si está registrado, buscamos sus datos para autorellenar
                var usuario = await _context.Usuarios.FindAsync(usuarioLogueadoId.Value);
                if (usuario != null)
                {
                    cita.UsuarioId = usuario.Id;
                    cita.ClienteNombre = usuario.Nombre;
                    cita.ClienteTelefono = usuario.Telefono;
                    cita.ClienteEmail = usuario.Email;
                }
            }
            // Si no está logueado, 'usuarioLogueadoId' es nulo y el formulario se renderiza vacío para invitado

            return View(cita);
        }

        // POST: TallerCitas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TallerCita tallerCita)
        {
            if (ModelState.IsValid)
            {
                tallerCita.FechaCita = DateTime.Now;
                tallerCita.EstadoTaller = "Recibido";

                _context.Add(tallerCita);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tallerCita);
        }
    }
}