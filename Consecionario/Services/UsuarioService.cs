using ConcesionarioN.Data.Context;
using ConcesionarioN.DTOs;
using ConcesionarioN.Models;
using ConcesionarioN.Validations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ConcesionarioN.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly ConcesionarioNDbContext _context;
        private readonly PasswordHasher<Usuario> _hasher = new();

        public UsuarioService(ConcesionarioNDbContext context)
        {
            _context = context;
        }

        private static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();

        private Task<bool> EmailExisteAsync(string emailNormalizado, int? excluirId = null) =>
            _context.Usuarios.AnyAsync(u => u.Email == emailNormalizado && (excluirId == null || u.Id != excluirId));

        // HU-01: devuelve el usuario solo si existe, está activo y la contraseña coincide.
        public async Task<Usuario?> AutenticarAsync(string email, string password)
        {
            var correo = NormalizarEmail(email);
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == correo);
            if (usuario == null || !usuario.Activo) return null;

            var resultado = _hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, password);
            return resultado == PasswordVerificationResult.Failed ? null : usuario;
        }

        // HU-02
        public async Task<ResultadoOperacion> RegistrarClienteAsync(RegistroDto dto)
        {
            var correo = NormalizarEmail(dto.Email);
            if (await EmailExisteAsync(correo))
                return ResultadoOperacion.Error("Ya existe una cuenta registrada con este correo.", nameof(RegistroDto.Email));

            var usuario = new Usuario
            {
                Nombre = dto.Nombre.Trim(),
                Cedula = dto.Cedula.Trim(),
                Telefono = dto.Telefono.Trim(),
                Email = correo,
                Rol = RolesSistema.Cliente,
                Activo = true,
                FechaCreacion = DateTime.Now
            };
            usuario.PasswordHash = _hasher.HashPassword(usuario, dto.Password);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return ResultadoOperacion.Ok("Cuenta creada correctamente. Ya puede iniciar sesión.");
        }

        // HU-03
        public async Task<ResultadoOperacion> CrearAsync(UsuarioCrearDto dto)
        {
            if (!RolesSistema.Personal.Contains(dto.Rol))
                return ResultadoOperacion.Error("El rol debe ser Admin o Dependiente.", nameof(UsuarioCrearDto.Rol));

            var correo = NormalizarEmail(dto.Email);
            if (await EmailExisteAsync(correo))
                return ResultadoOperacion.Error("Ya existe un usuario con este correo.", nameof(UsuarioCrearDto.Email));

            var usuario = new Usuario
            {
                Nombre = dto.Nombre.Trim(),
                Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim(),
                Email = correo,
                Rol = dto.Rol,
                Activo = true,
                FechaCreacion = DateTime.Now
            };
            usuario.PasswordHash = _hasher.HashPassword(usuario, dto.Password);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return ResultadoOperacion.Ok("Usuario creado correctamente.");
        }

        // HU-04: listado paginado con filtro por texto (nombre o correo) y por rol.
        public async Task<PaginaResultadoDto<UsuarioListaDto>> ListarAsync(string? texto, string? rol, int pagina, int tamanoPagina)
        {
            var consulta = _context.Usuarios.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(texto))
            {
                var t = texto.Trim();
                consulta = consulta.Where(u => u.Nombre.Contains(t) || u.Email.Contains(t));
            }
            if (!string.IsNullOrWhiteSpace(rol))
            {
                consulta = consulta.Where(u => u.Rol == rol);
            }

            var total = await consulta.CountAsync();
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanoPagina));
            var paginaActual = Math.Clamp(pagina, 1, totalPaginas);

            var items = await consulta
                .OrderBy(u => u.Nombre)
                .ThenBy(u => u.Id)
                .Skip((paginaActual - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .Select(u => new UsuarioListaDto
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Email = u.Email,
                    Rol = u.Rol,
                    Activo = u.Activo
                })
                .ToListAsync();

            return new PaginaResultadoDto<UsuarioListaDto>
            {
                Items = items,
                PaginaActual = paginaActual,
                TotalPaginas = totalPaginas,
                TotalRegistros = total
            };
        }

        public Task<Usuario?> ObtenerAsync(int id) =>
            _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

        private Task<int> OtrosAdminsActivosAsync(int idExcluido) =>
            _context.Usuarios.CountAsync(u => u.Id != idExcluido && u.Rol == RolesSistema.Admin && u.Activo);

        // HU-05
        public async Task<ResultadoOperacion> ActualizarAsync(UsuarioEditarDto dto)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == dto.Id);
            if (usuario == null)
                return ResultadoOperacion.Error("El usuario no existe.");

            if (!RolesSistema.Todos.Contains(dto.Rol))
                return ResultadoOperacion.Error("Rol no válido.", nameof(UsuarioEditarDto.Rol));

            // HU-05/CA-02: no se puede quitar el rol Admin al último Admin activo
            if (usuario.Rol == RolesSistema.Admin && usuario.Activo && dto.Rol != RolesSistema.Admin
                && await OtrosAdminsActivosAsync(usuario.Id) == 0)
            {
                return ResultadoOperacion.Error(
                    "No se puede quitar el rol Admin al último Admin activo del sistema.",
                    nameof(UsuarioEditarDto.Rol));
            }

            usuario.Nombre = dto.Nombre.Trim();
            usuario.Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim();
            usuario.Rol = dto.Rol;

            await _context.SaveChangesAsync();
            return ResultadoOperacion.Ok("Usuario actualizado correctamente.");
        }

      
        public async Task<bool> TieneMovimientosAsync(int id)
        {
            var usuario = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null) return false;

            var nombre = usuario.Nombre;
            return await _context.Ventas.AnyAsync(v => v.ClienteNombre == nombre)
                || await _context.Reservas.AnyAsync(r => r.ClienteNombre == nombre)
                || await _context.TallerCitas.AnyAsync(c => c.UsuarioId == id);
        }

        // HU-06
        public async Task<ResultadoEliminacion> EliminarAsync(int id, int adminActualId)
        {
            // HU-06/CA-02
            if (id == adminActualId)
                return new ResultadoEliminacion { Exito = false, Mensaje = "No puede eliminar su propia cuenta." };

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null)
                return new ResultadoEliminacion { Exito = false, Mensaje = "El usuario no existe." };

            if (usuario.Rol == RolesSistema.Admin && usuario.Activo && await OtrosAdminsActivosAsync(usuario.Id) == 0)
                return new ResultadoEliminacion { Exito = false, Mensaje = "No se puede eliminar al último Admin activo del sistema." };

            // HU-06/CA-01
            if (await TieneMovimientosAsync(id))
            {
                usuario.Activo = false;
                await _context.SaveChangesAsync();
                return new ResultadoEliminacion
                {
                    Exito = true,
                    Desactivado = true,
                    Mensaje = "El usuario tiene movimientos (ventas, reservas o citas), por lo que se desactivó y ya no puede iniciar sesión."
                };
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return new ResultadoEliminacion { Exito = true, Desactivado = false, Mensaje = "Usuario eliminado correctamente." };
        }

        // Cuentas de prueba
        public async Task SembrarDatosInicialesAsync()
        {
            if (await _context.Usuarios.AnyAsync()) return;

            var semillas = new[]
            {
                ("Administrador", "admin@concesionaria.com", RolesSistema.Admin),
                ("Dependiente Demo", "dependiente@concesionaria.com", RolesSistema.Dependiente),
                ("Cliente Demo", "cliente@concesionaria.com", RolesSistema.Cliente)
            };

            foreach (var (nombre, correo, rol) in semillas)
            {
                var u = new Usuario { Nombre = nombre, Email = correo, Rol = rol, Activo = true, FechaCreacion = DateTime.Now };
                u.PasswordHash = _hasher.HashPassword(u, "Prueba123");
                _context.Usuarios.Add(u);
            }
            await _context.SaveChangesAsync();
        }
    }
}
