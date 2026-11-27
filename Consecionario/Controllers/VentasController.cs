using System.Security.Claims;
using ConcesionarioN.DTOs;
using ConcesionarioN.Services;
using ConcesionarioN.Validations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ConcesionarioN.Controllers
{
    [Authorize]
    public class VentasController : Controller
    {
        private readonly IVentaService _ventas;

        public VentasController(IVentaService ventas)
        {
            _ventas = ventas;
        }

        private int IdActual() =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        private bool EsPersonal() => User.IsInRole(RolesSistema.Admin) || User.IsInRole(RolesSistema.Dependiente);

        // HU-15: historial. Un Cliente solo ve sus propias compras (HU-15/CA-02)
        public async Task<IActionResult> Index(FiltroVentasDto filtro)
        {
            var soloCliente = User.IsInRole(RolesSistema.Cliente) ? IdActual() : (int?)null;
            if (soloCliente.HasValue) filtro.Cliente = null;

            ViewBag.Filtro = filtro;
            ViewBag.EsPersonal = EsPersonal();
            return View(await _ventas.ListarAsync(filtro, soloCliente));
        }

        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null) return NotFound();
            var venta = await _ventas.ObtenerAsync(id.Value);
            if (venta == null) return NotFound();

            // Un Cliente no puede ver compras de otros
            if (User.IsInRole(RolesSistema.Cliente) && venta.ClienteId != IdActual()) return NotFound();

            return View(venta);
        }

        // ---------- HU-13: vender vehículo o moto ----------

        private async Task CargarListasUnidadAsync()
        {
            var clientes = await _ventas.ClientesActivosAsync();
            ViewBag.Clientes = clientes.Select(c => new SelectListItem($"{c.Nombre} ({c.Email})", c.Id.ToString())).ToList();
            ViewBag.Unidades = await _ventas.UnidadesAsync();
            ViewBag.Metodos = MetodosPago.Todos;
        }

        [Authorize(Roles = "Admin,Dependiente")]
        [HttpGet]
        public async Task<IActionResult> CrearUnidad()
        {
            await CargarListasUnidadAsync();
            return View(new VentaUnidadDto());
        }

        [Authorize(Roles = "Admin,Dependiente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearUnidad(VentaUnidadDto dto)
        {
            // Los errores de enlace (por ejemplo, texto en el precio) se reemplazan por los del servicio
            ModelState.Clear();

            var resultado = await _ventas.RegistrarUnidadAsync(dto, IdActual());
            if (!resultado.Exito)
            {
                foreach (var e in resultado.Errores) ModelState.AddModelError(e.Campo, e.Mensaje);
                await CargarListasUnidadAsync();
                return View(dto);
            }

            TempData["Exito"] = "Venta registrada. La unidad quedó como Vendida.";
            return RedirectToAction(nameof(Detalles), new { id = resultado.VentaId });
        }

        // ---------- HU-14: vender accesorios ----------

        private async Task CargarListasAccesoriosAsync()
        {
            var clientes = await _ventas.ClientesActivosAsync();
            ViewBag.Clientes = clientes.Select(c => new SelectListItem($"{c.Nombre} ({c.Email})", c.Id.ToString())).ToList();
            ViewBag.Accesorios = await _ventas.AccesoriosAsync();
            ViewBag.Metodos = MetodosPago.Todos;
        }

        [Authorize(Roles = "Admin,Dependiente")]
        [HttpGet]
        public async Task<IActionResult> CrearAccesorios()
        {
            await CargarListasAccesoriosAsync();
            return View(new VentaAccesoriosDto { Lineas = new List<LineaAccesorioDto> { new() } });
        }

        [Authorize(Roles = "Admin,Dependiente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearAccesorios(VentaAccesoriosDto dto)
        {
            ModelState.Clear();

            var resultado = await _ventas.RegistrarAccesoriosAsync(dto, IdActual());
            if (!resultado.Exito)
            {
                foreach (var e in resultado.Errores) ModelState.AddModelError(e.Campo, e.Mensaje);
                if (dto.Lineas.Count == 0) dto.Lineas.Add(new LineaAccesorioDto());
                await CargarListasAccesoriosAsync();
                return View(dto);
            }

            TempData["Exito"] = "Venta registrada. El stock se descontó.";
            return RedirectToAction(nameof(Detalles), new { id = resultado.VentaId });
        }

        // ---------- HU-16: solo Admin ----------

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();
            var venta = await _ventas.ObtenerAsync(id.Value);
            if (venta == null) return NotFound();
            if (venta.Anulada)
            {
                TempData["Error"] = "No se puede editar una venta anulada.";
                return RedirectToAction(nameof(Detalles), new { id });
            }
            ViewBag.Metodos = MetodosPago.Todos;
            return View(venta);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, string metodoPago, string? notas)
        {
            var resultado = await _ventas.EditarAsync(id, metodoPago, notas);
            if (!resultado.Exito)
            {
                TempData["Error"] = resultado.Mensaje;
                return RedirectToAction(nameof(Editar), new { id });
            }

            TempData["Exito"] = resultado.Mensaje;
            return RedirectToAction(nameof(Detalles), new { id });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Anular(int id)
        {
            var resultado = await _ventas.AnularAsync(id);
            if (resultado.Exito) TempData["Exito"] = resultado.Mensaje;
            else TempData["Error"] = resultado.Mensaje;
            return RedirectToAction(nameof(Detalles), new { id });
        }

        // ---------- HU-17, HU-18, HU-19: solo Admin ----------

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Dashboard(FiltroDashboardDto filtro)
        {
            return View(await _ventas.DashboardAsync(filtro));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportarCsv(FiltroDashboardDto filtro)
        {
            var contenido = await _ventas.ExportarCsvAsync(filtro);
            return File(contenido, "text/csv; charset=utf-8", $"ventas_{DateTime.Now:yyyyMMdd_HHmm}.csv");
        }
    }
}
