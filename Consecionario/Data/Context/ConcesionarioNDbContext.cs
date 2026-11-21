using Microsoft.EntityFrameworkCore;
using ConcesionarioN.Models;

namespace ConcesionarioN.Data.Context
{
    public class ConcesionarioNDbContext : DbContext
    {
        public ConcesionarioNDbContext(DbContextOptions<ConcesionarioNDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<Moto> Motos { get; set; } // Nuevo DbSet separado
        public DbSet<Accesorio> Accesorios { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<TallerCita> TallerCitas { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
    }
}