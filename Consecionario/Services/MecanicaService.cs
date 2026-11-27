using ConcesionarioN.Data.Context;
using ConcesionarioN.DTOs;
using ConcesionarioN.Models;
using ConcesionarioN.Validations;
using Microsoft.EntityFrameworkCore;

namespace ConcesionarioN.Services
{
    public class MecanicaService : IMecanicaService
    {
        private readonly ConcesionarioNDbContext _context;

        public MecanicaService(ConcesionarioNDbContext context)
        {
            _context = context;
        }

        private static string NormalizarPlaca(string? placa) => (placa ?? string.Empty).Trim().ToUpperInvariant();

        // ---------- Datos para los formularios ----------

        public Task<List<Usuario>> ClientesActivosAsync() =>
            _context.Usuarios.AsNoTracking()
                .Where(u => u.Rol == RolesSistema.Cliente && u.Activo)
                .OrderBy(u => u.Nombre)
                .ToListAsync();

        public async Task<Dictionary<int, List<string>>> PlacasPorClienteAsync(int? soloClienteId = null)
        {
            var ventas = _context.Ventas.AsNoTracking().Where(v => !v.Anulada && v.ClienteId != null);
            var recepciones = _context.TallerCitas.AsNoTracking().Where(t => t.UsuarioId != null);
            if (soloClienteId.HasValue)
            {
                ventas = ventas.Where(v => v.ClienteId == soloClienteId.Value);
                recepciones = recepciones.Where(t => t.UsuarioId == soloClienteId.Value);
            }

            var deVehiculos = await ventas.Where(v => v.Vehiculo != null && v.Vehiculo.Placa != null)
                .Select(v => new { ClienteId = v.ClienteId!.Value, Placa = v.Vehiculo!.Placa! }).ToListAsync();
            var deMotos = await ventas.Where(v => v.Moto != null && v.Moto.Placa != null)
                .Select(v => new { ClienteId = v.ClienteId!.Value, Placa = v.Moto!.Placa! }).ToListAsync();
            var deTaller = await recepciones
                .Select(t => new { ClienteId = t.UsuarioId!.Value, Placa = t.PlacaVehiculo }).ToListAsync();

            return deVehiculos.Concat(deMotos).Concat(deTaller)
                .GroupBy(x => x.ClienteId)
                .ToDictionary(g => g.Key, g => g.Select(x => NormalizarPlaca(x.Placa)).Distinct().OrderBy(p => p).ToList());
        }

        // ---------- HU-20: recibir vehículo ----------

        public async Task<ResultadoMecanica> RegistrarRecepcionAsync(RecepcionCrearDto dto)
        {
            var errores = new List<ErrorCampo>();

            var cliente = dto.ClienteId.HasValue
                ? await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == dto.ClienteId && u.Rol == RolesSistema.Cliente && u.Activo)
                : null;
            if (cliente == null) errores.Add(new ErrorCampo(nameof(dto.ClienteId), "Seleccione un cliente registrado."));

            // HU-20/CA-02: cliente, placa y problema de al menos 10 caracteres
            var placa = NormalizarPlaca(dto.PlacaVehiculo);
            if (placa.Length == 0)
                errores.Add(new ErrorCampo(nameof(dto.PlacaVehiculo), "La placa del vehículo es obligatoria."));
            else if (placa.Length > 10)
                errores.Add(new ErrorCampo(nameof(dto.PlacaVehiculo), "La placa no puede superar 10 caracteres."));

            var problema = (dto.DescripcionProblema ?? string.Empty).Trim();
            if (problema.Length < 10)
                errores.Add(new ErrorCampo(nameof(dto.DescripcionProblema), "Describa el problema con al menos 10 caracteres."));

            var estadoVehiculo = string.IsNullOrWhiteSpace(dto.EstadoVehiculo) ? null : dto.EstadoVehiculo.Trim();
            if (estadoVehiculo != null && estadoVehiculo.Length > 300)
                errores.Add(new ErrorCampo(nameof(dto.EstadoVehiculo), "El estado del vehículo no puede superar 300 caracteres."));

            if (errores.Count > 0) return new ResultadoMecanica { Errores = errores };

            // HU-20/CA-01: se guarda con estado "Recibido" y fecha de ingreso
            var recepcion = new TallerCita
            {
                UsuarioId = cliente!.Id,
                ClienteNombre = cliente.Nombre,
                ClienteTelefono = cliente.Telefono,
                ClienteEmail = cliente.Email,
                PlacaVehiculo = placa,
                EstadoVehiculo = estadoVehiculo,
                DescripcionProblema = problema,
                EstadoTaller = EstadosTaller.Recibido,
                FechaIngreso = DateTime.Now
            };

            _context.TallerCitas.Add(recepcion);
            await _context.SaveChangesAsync();
            return new ResultadoMecanica { Id = recepcion.Id };
        }

        // ---------- HU-21: consultar y actualizar estado ----------

        public async Task<List<TallerCita>> ListarRecepcionesAsync(FiltroRecepcionesDto filtro, int? soloClienteId)
        {
            var q = _context.TallerCitas.AsNoTracking();

            if (soloClienteId.HasValue) q = q.Where(t => t.UsuarioId == soloClienteId.Value);
            if (!string.IsNullOrWhiteSpace(filtro.Estado)) q = q.Where(t => t.EstadoTaller == filtro.Estado);
            if (!string.IsNullOrWhiteSpace(filtro.Placa))
            {
                var p = NormalizarPlaca(filtro.Placa);
                q = q.Where(t => t.PlacaVehiculo.Contains(p));
            }

            return await q.OrderByDescending(t => t.FechaIngreso).ThenByDescending(t => t.Id).ToListAsync();
        }

        public Task<TallerCita?> ObtenerRecepcionAsync(int id) =>
            _context.TallerCitas.AsNoTracking().Include(t => t.Usuario).FirstOrDefaultAsync(t => t.Id == id);

        public async Task<ResultadoOperacion> CambiarEstadoAsync(int id, string nuevoEstado)
        {
            if (!EstadosTaller.Todos.Contains(nuevoEstado))
                return ResultadoOperacion.Error("Seleccione un estado válido.", "nuevoEstado");

            var recepcion = await _context.TallerCitas.FirstOrDefaultAsync(t => t.Id == id);
            if (recepcion == null) return ResultadoOperacion.Error("La recepción no existe.");
            if (recepcion.EstadoTaller == nuevoEstado)
                return ResultadoOperacion.Error($"La recepción ya está en estado {nuevoEstado}.");

            // HU-21/CA-01: el cambio de estado queda con fecha
            recepcion.EstadoTaller = nuevoEstado;
            recepcion.FechaCambioEstado = DateTime.Now;
            await _context.SaveChangesAsync();
            return ResultadoOperacion.Ok($"Estado actualizado a {nuevoEstado}.");
        }

        // ---------- HU-22: eliminar recepción ----------

        public async Task<ResultadoOperacion> EliminarRecepcionAsync(int id)
        {
            var recepcion = await _context.TallerCitas.FirstOrDefaultAsync(t => t.Id == id);
            if (recepcion == null) return ResultadoOperacion.Error("La recepción no existe.");

            // HU-22/CA-01: solo se eliminan recepciones en estado "Recibido"
            if (recepcion.EstadoTaller != EstadosTaller.Recibido)
                return ResultadoOperacion.Error($"No se puede eliminar la recepción porque está en estado \"{recepcion.EstadoTaller}\". Solo se eliminan recepciones en estado \"Recibido\".");

            _context.TallerCitas.Remove(recepcion);
            await _context.SaveChangesAsync();
            return ResultadoOperacion.Ok("Recepción eliminada.");
        }

        // ---------- HU-23: agendar cita ----------

        public async Task<List<HorarioItem>> HorariosAsync(DateTime dia, int? excluirCitaId = null)
        {
            var inicio = dia.Date;
            var fin = inicio.AddDays(1);
            var ocupados = await _context.CitasIngreso.AsNoTracking()
                .Where(c => c.Estado == EstadosCita.Programada && c.FechaHora >= inicio && c.FechaHora < fin
                            && (!excluirCitaId.HasValue || c.Id != excluirCitaId.Value))
                .Select(c => c.FechaHora)
                .ToListAsync();

            var ahora = DateTime.Now;
            return HorarioTaller.HorariosDelDia(inicio)
                .Select(h => new HorarioItem(h, h > ahora && !ocupados.Contains(h)))
                .ToList();
        }

        // Valida fecha y hora (HU-23/CA-02); devuelve la fecha y hora de la cita o null con errores
        private async Task<DateTime?> ValidarHorarioAsync(DateTime? fecha, int? hora, List<ErrorCampo> errores, int? excluirCitaId)
        {
            if (!fecha.HasValue)
            {
                errores.Add(new ErrorCampo(nameof(CitaDto.Fecha), "Seleccione la fecha de la cita."));
                return null;
            }
            if (!hora.HasValue || !HorarioTaller.EsHoraValida(hora.Value))
            {
                errores.Add(new ErrorCampo(nameof(CitaDto.Hora), "Seleccione un horario disponible."));
                return null;
            }

            var fechaHora = fecha.Value.Date.AddHours(hora.Value);

            if (fechaHora <= DateTime.Now)
            {
                errores.Add(new ErrorCampo(nameof(CitaDto.Fecha), "No se puede agendar una cita en una fecha u hora pasada."));
                return null;
            }
            if (!HorarioTaller.EsDiaHabil(fechaHora))
            {
                errores.Add(new ErrorCampo(nameof(CitaDto.Fecha), "El taller atiende de lunes a viernes."));
                return null;
            }

            var ocupado = await _context.CitasIngreso.AnyAsync(c => c.Estado == EstadosCita.Programada && c.FechaHora == fechaHora
                                                                   && (!excluirCitaId.HasValue || c.Id != excluirCitaId.Value));
            if (ocupado)
            {
                errores.Add(new ErrorCampo(nameof(CitaDto.Hora), "Ese horario ya está ocupado. Elija otro."));
                return null;
            }

            return fechaHora;
        }

        public async Task<ResultadoMecanica> AgendarCitaAsync(CitaDto dto)
        {
            var errores = new List<ErrorCampo>();

            var cliente = dto.ClienteId.HasValue
                ? await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == dto.ClienteId && u.Rol == RolesSistema.Cliente && u.Activo)
                : null;
            if (cliente == null) errores.Add(new ErrorCampo(nameof(dto.ClienteId), "Seleccione un cliente registrado."));

            var placa = NormalizarPlaca(dto.PlacaVehiculo);
            if (placa.Length == 0)
                errores.Add(new ErrorCampo(nameof(dto.PlacaVehiculo), "Indique la placa del vehículo."));
            else if (placa.Length > 10)
                errores.Add(new ErrorCampo(nameof(dto.PlacaVehiculo), "La placa no puede superar 10 caracteres."));

            var motivo = string.IsNullOrWhiteSpace(dto.Motivo) ? null : dto.Motivo.Trim();
            if (motivo != null && motivo.Length > 300)
                errores.Add(new ErrorCampo(nameof(dto.Motivo), "El motivo no puede superar 300 caracteres."));

            var fechaHora = await ValidarHorarioAsync(dto.Fecha, dto.Hora, errores, null);
            if (errores.Count > 0) return new ResultadoMecanica { Errores = errores };

            // HU-23/CA-01: al confirmar, la cita queda "Programada"
            var cita = new CitaIngreso
            {
                ClienteId = cliente!.Id,
                ClienteNombre = cliente.Nombre,
                PlacaVehiculo = placa,
                Motivo = motivo,
                FechaHora = fechaHora!.Value,
                Estado = EstadosCita.Programada,
                FechaCreacion = DateTime.Now
            };

            _context.CitasIngreso.Add(cita);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Otro usuario tomó el mismo horario al mismo tiempo (índice único)
                return ResultadoMecanica.Fallo(nameof(dto.Hora), "Ese horario ya está ocupado. Elija otro.");
            }
            return new ResultadoMecanica { Id = cita.Id };
        }

        public Task<CitaIngreso?> ObtenerCitaAsync(int id) =>
            _context.CitasIngreso.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

        // ---------- HU-24: reprogramar y cancelar ----------

        public async Task<ResultadoMecanica> ReprogramarCitaAsync(int id, DateTime? fecha, int? hora)
        {
            var cita = await _context.CitasIngreso.FirstOrDefaultAsync(c => c.Id == id);
            if (cita == null) return ResultadoMecanica.Fallo(string.Empty, "La cita no existe.");
            if (cita.Estado != EstadosCita.Programada)
                return ResultadoMecanica.Fallo(string.Empty, "Solo se pueden reprogramar citas programadas.");

            var errores = new List<ErrorCampo>();
            var fechaHora = await ValidarHorarioAsync(fecha, hora, errores, cita.Id);
            if (errores.Count > 0) return new ResultadoMecanica { Errores = errores };

            // HU-24/CA-02: la cita toma el nuevo horario y el anterior queda libre
            cita.FechaHora = fechaHora!.Value;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return ResultadoMecanica.Fallo(nameof(CitaDto.Hora), "Ese horario ya está ocupado. Elija otro.");
            }
            return new ResultadoMecanica { Id = cita.Id };
        }

        public async Task<ResultadoOperacion> CancelarCitaAsync(int id)
        {
            var cita = await _context.CitasIngreso.FirstOrDefaultAsync(c => c.Id == id);
            if (cita == null) return ResultadoOperacion.Error("La cita no existe.");
            if (cita.Estado != EstadosCita.Programada) return ResultadoOperacion.Error("La cita ya está cancelada.");

            // HU-24/CA-02: al cancelar, el horario queda libre
            cita.Estado = EstadosCita.Cancelada;
            cita.FechaCancelacion = DateTime.Now;
            await _context.SaveChangesAsync();
            return ResultadoOperacion.Ok("Cita cancelada. El horario quedó libre.");
        }

        // HU-24/CA-01: agenda por día o por semana (lunes a domingo)
        public async Task<AgendaDto> AgendaAsync(string? vista, DateTime? fecha, int? soloClienteId)
        {
            var dia = (fecha ?? DateTime.Today).Date;
            var dto = new AgendaDto { Vista = vista == "semana" ? "semana" : "dia", Fecha = dia };

            if (dto.Vista == "semana")
            {
                dto.Desde = dia.AddDays(-(((int)dia.DayOfWeek + 6) % 7));
                dto.Hasta = dto.Desde.AddDays(6);
            }
            else
            {
                dto.Desde = dia;
                dto.Hasta = dia;
            }

            var q = _context.CitasIngreso.AsNoTracking()
                .Where(c => c.FechaHora >= dto.Desde && c.FechaHora < dto.Hasta.AddDays(1));
            if (soloClienteId.HasValue) q = q.Where(c => c.ClienteId == soloClienteId.Value);

            dto.Citas = await q.OrderBy(c => c.FechaHora).ThenBy(c => c.Id).ToListAsync();
            return dto;
        }
    }
}
