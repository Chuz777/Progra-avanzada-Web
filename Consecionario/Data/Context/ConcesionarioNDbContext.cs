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
        public DbSet<VentaLinea> VentaLineas { get; set; }
        public DbSet<TallerCita> TallerCitas { get; set; }
        public DbSet<CitaIngreso> CitasIngreso { get; set; }
        public DbSet<Reserva> Reservas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Venta>().HasOne(v => v.Cliente).WithMany().HasForeignKey(v => v.ClienteId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Venta>().HasOne(v => v.Vendedor).WithMany().HasForeignKey(v => v.VendedorId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Venta>().HasOne(v => v.Vehiculo).WithMany().HasForeignKey(v => v.VehiculoId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Venta>().HasOne(v => v.Moto).WithMany().HasForeignKey(v => v.MotoId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<VentaLinea>().HasOne(l => l.Venta).WithMany(v => v.Lineas).HasForeignKey(l => l.VentaId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<VentaLinea>().HasOne(l => l.Accesorio).WithMany().HasForeignKey(l => l.AccesorioId).OnDelete(DeleteBehavior.Restrict);

            // Módulo 4: Mecánica
            modelBuilder.Entity<TallerCita>().HasOne(t => t.Usuario).WithMany().HasForeignKey(t => t.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CitaIngreso>().HasOne(c => c.Cliente).WithMany().HasForeignKey(c => c.ClienteId).OnDelete(DeleteBehavior.Restrict);
            // HU-23/CA-02: no puede haber dos citas programadas en el mismo horario
            modelBuilder.Entity<CitaIngreso>().HasIndex(c => c.FechaHora).IsUnique().HasFilter("[Estado] = N'Programada'");
        }
    }
}