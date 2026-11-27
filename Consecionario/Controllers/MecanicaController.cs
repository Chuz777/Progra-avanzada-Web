using System.Security.Claims;
using ConcesionarioN.DTOs;
using ConcesionarioN.Services;
using ConcesionarioN.Validations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ConcesionarioN.Controllers
{
    // Módulo 4: Mecánica (recepciones de taller y citas de ingreso)
    [Authorize]
    public class MecanicaController : Controller
    {
        private readonly IMecanicaService _mecanica;

        public MecanicaController(IMecanicaService mecanica)
        {
            _mecanica = mecanica;
        }

        private int IdActual() =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        private bool EsPersonal() => User.IsInRole(RolesSistema.Admin) || User.IsInRole(RolesSistema.Dependiente);

        private bool EsCliente() => User.IsInRole(RolesSistema.Cliente);

        // ---------- HU-21: recepciones. El Cliente solo ve sus vehículos (HU-21/CA-02) ----------

        public async Task<IActionResult> Index(FiltroRecepcionesDto filtro)
        {
            var soloCliente = EsCliente() ? IdActual() : (int?)null;

            ViewBag.Filtro = filtro;
            ViewBag.EsPersonal = EsPersonal();
            return View(await _mecanica.ListarRecepcionesAsync(filtro, soloCliente));
        }

        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null) return NotFound();
            var recepcion = await _mecanica.ObtenerRecepcionAsync(id.Value);
            if (recepcion == null) return NotFound();

            // Un Cliente no puede ver vehículos de otros clientes
            if (EsCliente() && recepcion.UsuarioId != IdActual()) return NotFound();

            ViewBag.EsPersonal = EsPersonal();
            return View(recepcion);
        }

        // ---------- HU-20: recibir vehículo ----------

        private async Task CargarListasRecepcionAsync()
        {
            var clientes = await _mecanica.ClientesActivosAsync();
            ViewBag.Clientes = clientes.Select(c => new SelectListItem($"{c.Nombre} ({c.Email})", c.Id.ToString())).ToList();
            ViewBag.Placas = await _mecanica.PlacasPorClienteAsync();
        }

        [Authorize(Roles = "Admin,Dependiente")]
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarListasRecepcionAsync();
            return View(new RecepcionCrearDto());
        }

        [Authorize(Roles = "Admin,Dependiente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(RecepcionCrearDto dto)
        {
            ModelState.Clear();

            var resultado = await _mecanica.RegistrarRecepcionAsync(dto);
            if (!resultado.Exito)
            {
                foreach (var e in resultado.Errores) ModelState.AddModelError(e.Campo, e.Mensaje);
                await CargarListasRecepcionAsync();
                return View(dto);
            }

            TempData["Exito"] = "Recepción registrada con estado Recibido.";
            return RedirectToAction(nameof(Detalles), new { id = resultado.Id });
        }

        [Authorize(Roles = "Admin,Dependiente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
        {
            var resultado = await _mecanica.CambiarEstadoAsync(id, nuevoEstado);
            if (resultado.Exito) TempData["Exito"] = resultado.Mensaje;
            else TempData["Error"] = resultado.Mensaje;
            return RedirectToAction(nameof(Detalles), new { id });
        }

        // ---------- HU-22: solo Admin ----------

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var resultado = await _mecanica.EliminarRecepcionAsync(id);
            if (!resultado.Exito)
            {
                TempData["Error"] = resultado.Mensaje;
                return RedirectToAction(nameof(Detalles), new { id });
            }

            TempData["Exito"] = resultado.Mensaje;
            return RedirectToAction(nameof(Index));
        }

        // ---------- HU-23: agendar cita de ingreso ----------

        private async Task CargarListasCitaAsync(DateTime dia, int? excluirCitaId = null)
        {
            ViewBag.Dia = dia;
            ViewBag.Horarios = await _mecanica.HorariosAsync(dia, excluirCitaId);
            ViewBag.EsPersonal = EsPersonal();

            if (EsPersonal())
            {
                var clientes = await _mecanica.ClientesActivosAsync();
                ViewBag.Clientes = clientes.Select(c => new SelectListItem($"{c.Nombre} ({c.Email})", c.Id.ToString())).ToList();
                ViewBag.Placas = await _mecanica.PlacasPorClienteAsync();
            }
            else
            {
                ViewBag.Placas = await _mecanica.PlacasPorClienteAsync(IdActual());
            }
        }

        // Primer día hábil desde mañana
        private static DateTime ProximoDiaHabil()
        {
            var dia = DateTime.Today.AddDays(1);
            while (!HorarioTaller.EsDiaHabil(dia)) dia = dia.AddDays(1);
            return dia;
        }

        [HttpGet]
        public async Task<IActionResult> Agendar(DateTime? fecha, int? clienteId, string? placa)
        {
            var dia = (fecha ?? ProximoDiaHabil()).Date;
            var dto = new CitaDto
            {
                Fecha = dia,
                ClienteId = EsCliente() ? IdActual() : clienteId,
                PlacaVehiculo = placa ?? string.Empty
            };

            await CargarListasCitaAsync(dia);
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agendar(CitaDto dto)
        {
            ModelState.Clear();

            // El Cliente solo agenda a su nombre
            if (EsCliente()) dto.ClienteId = IdActual();

            var resultado = await _mecanica.AgendarCitaAsync(dto);
            if (!resultado.Exito)
            {
                foreach (var e in resultado.Errores) ModelState.AddModelError(e.Campo, e.Mensaje);
                await CargarListasCitaAsync((dto.Fecha ?? ProximoDiaHabil()).Date);
                return View(dto);
            }

            TempData["Exito"] = "Cita agendada. Quedó en estado Programada.";
            return RedirectToAction(nameof(Agenda), new { fecha = dto.Fecha!.Value.ToString("yyyy-MM-dd") });
        }

        // ---------- HU-24: agenda, reprogramar y cancelar ----------

        public async Task<IActionResult> Agenda(string? vista, DateTime? fecha)
        {
            var soloCliente = EsCliente() ? IdActual() : (int?)null;
            ViewBag.EsPersonal = EsPersonal();
            return View(await _mecanica.AgendaAsync(vista, fecha, soloCliente));
        }

        [HttpGet]
        public async Task<IActionResult> Reprogramar(int? id, DateTime? fecha)
        {
            if (id == null) return NotFound();
            var cita = await _mecanica.ObtenerCitaAsync(id.Value);
            if (cita == null) return NotFound();

            // HU-24/CA-02: el Cliente solo gestiona sus citas
            if (EsCliente() && cita.ClienteId != IdActual()) return NotFound();

            if (cita.Estado != EstadosCita.Programada)
            {
                TempData["Error"] = "Solo se pueden reprogramar citas programadas.";
                return RedirectToAction(nameof(Agenda), new { fecha = cita.FechaHora.ToString("yyyy-MM-dd") });
            }

            var dia = (fecha ?? cita.FechaHora).Date;
            ViewBag.Cita = cita;
            await CargarListasCitaAsync(dia, cita.Id);
            return View(new CitaDto { Fecha = dia, Hora = dia == cita.FechaHora.Date ? cita.FechaHora.Hour : null });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reprogramar(int id, CitaDto dto)
        {
            ModelState.Clear();

            var cita = await _mecanica.ObtenerCitaAsync(id);
            if (cita == null) return NotFound();
            if (EsCliente() && cita.ClienteId != IdActual()) return NotFound();

            var resultado = await _mecanica.ReprogramarCitaAsync(id, dto.Fecha, dto.Hora);
            if (!resultado.Exito)
            {
                foreach (var e in resultado.Errores) ModelState.AddModelError(e.Campo, e.Mensaje);
                var dia = (dto.Fecha ?? cita.FechaHora).Date;
                ViewBag.Cita = cita;
                await CargarListasCitaAsync(dia, cita.Id);
                return View(dto);
            }

            TempData["Exito"] = "Cita reprogramada. El horario anterior quedó libre.";
            return RedirectToAction(nameof(Agenda), new { fecha = dto.Fecha!.Value.ToString("yyyy-MM-dd") });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var cita = await _mecanica.ObtenerCitaAsync(id);
            if (cita == null) return NotFound();
            if (EsCliente() && cita.ClienteId != IdActual()) return NotFound();

            var resultado = await _mecanica.CancelarCitaAsync(id);
            if (resultado.Exito) TempData["Exito"] = resultado.Mensaje;
            else TempData["Error"] = resultado.Mensaje;
            return RedirectToAction(nameof(Agenda), new { fecha = cita.FechaHora.ToString("yyyy-MM-dd") });
        }
    }
}
