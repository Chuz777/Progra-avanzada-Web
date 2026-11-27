using System.Text.RegularExpressions;
using ConcesionarioN.Data.Context;
using ConcesionarioN.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ConcesionarioN.Services
{
    public class CatalogoService : ICatalogoService
    {
        private static readonly Regex PlacaRegex = new(@"^[A-Z0-9-]{4,10}$", RegexOptions.Compiled);
        private static readonly Regex VinRegex = new(@"^[A-Z0-9]{11,17}$", RegexOptions.Compiled);

        private readonly ConcesionarioNDbContext _context;

        public CatalogoService(ConcesionarioNDbContext context)
        {
            _context = context;
        }

        public string? Normalizar(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? null : valor.Trim().ToUpperInvariant();

        public async Task<List<ErrorCampo>> ValidarIdentificadoresAsync(string? placa, string? vin, string tipo, int? idExcluir)
        {
            var errores = new List<ErrorCampo>();
            var p = Normalizar(placa);
            var v = Normalizar(vin);

            if (p == null) errores.Add(new ErrorCampo("Placa", "La placa es obligatoria."));
            else if (!PlacaRegex.IsMatch(p)) errores.Add(new ErrorCampo("Placa", "La placa debe tener de 4 a 10 letras o números."));

            if (v == null) errores.Add(new ErrorCampo("Vin", "El VIN es obligatorio."));
            else if (!VinRegex.IsMatch(v)) errores.Add(new ErrorCampo("Vin", "El VIN debe tener de 11 a 17 letras o números."));

            // Id 0 nunca existe, así que no excluye nada al crear
            var excluirVehiculo = tipo == "Vehiculo" ? idExcluir ?? 0 : 0;
            var excluirMoto = tipo == "Moto" ? idExcluir ?? 0 : 0;

            if (p != null && PlacaRegex.IsMatch(p))
            {
                var repetida = await _context.Vehiculos.AnyAsync(x => x.Placa == p && x.Id != excluirVehiculo)
                            || await _context.Motos.AnyAsync(x => x.Placa == p && x.Id != excluirMoto);
                if (repetida) errores.Add(new ErrorCampo("Placa", "Ya existe una unidad registrada con esta placa."));
            }

            if (v != null && VinRegex.IsMatch(v))
            {
                var repetido = await _context.Vehiculos.AnyAsync(x => x.Vin == v && x.Id != excluirVehiculo)
                            || await _context.Motos.AnyAsync(x => x.Vin == v && x.Id != excluirMoto);
                if (repetido) errores.Add(new ErrorCampo("Vin", "Ya existe una unidad registrada con este VIN."));
            }

            return errores;
        }

        // Reserva "activa" = Activa o En proceso
        public async Task<bool> VehiculoVendidoOReservadoAsync(int id) =>
            await _context.Ventas.AnyAsync(v => v.VehiculoId == id && !v.Anulada)
            || await _context.Reservas.AnyAsync(r => r.VehiculoId == id
                && (r.EstadoReserva == "Activa" || r.EstadoReserva == "En proceso"));

        // Las reservas del modelo actual son de vehículos; las motos solo se bloquean por ventas vigentes.
        public Task<bool> MotoVendidaAsync(int id) =>
            _context.Ventas.AnyAsync(v => v.MotoId == id && !v.Anulada);

        public async Task<bool> VehiculoTieneMovimientosAsync(int id) =>
            await _context.Ventas.AnyAsync(v => v.VehiculoId == id)
            || await _context.Reservas.AnyAsync(r => r.VehiculoId == id);

        public Task<bool> MotoTieneMovimientosAsync(int id) =>
            _context.Ventas.AnyAsync(v => v.MotoId == id);

        public Task<bool> AccesorioTieneVentasAsync(int id) =>
            _context.VentaLineas.AnyAsync(l => l.AccesorioId == id);
    }
}
