using Microsoft.EntityFrameworkCore;
using AutoGest.Models;

namespace AutoGest.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<OrdenReparacion> Ordenes { get; set; }
        public DbSet<DetalleReparacion> Detalles { get; set; }
        public DbSet<Repuesto> Repuestos { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // MySQL DateTime
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetColumnType("DATETIME");
                    }
                }
            }

            // Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Nombre).HasMaxLength(50).IsRequired();
                entity.Property(u => u.Apellido).HasMaxLength(50).IsRequired();
                entity.Property(u => u.Email).HasMaxLength(100).IsRequired();
                entity.Property(u => u.Password).HasMaxLength(255).IsRequired();
                entity.Property(u => u.Avatar).HasMaxLength(500);
                entity.Property(u => u.Rol).HasMaxLength(20).IsRequired();
                entity.Property(u => u.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(u => u.Activo).HasDefaultValue(true);

                entity.HasMany(u => u.OrdenesAsignadas)
                    .WithOne(o => o.Mecanico)
                    .HasForeignKey(o => o.MecanicoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.OrdenesCreadas)
                    .WithOne(o => o.UsuarioCreacion)
                    .HasForeignKey(o => o.UsuarioCreacionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.OrdenesModificadas)
                    .WithOne(o => o.UsuarioModificacion)
                    .HasForeignKey(o => o.UsuarioModificacionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.PagosCreados)
                    .WithOne(p => p.UsuarioCreacion)
                    .HasForeignKey(p => p.UsuarioCreacionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.PagosAnulados)
                    .WithOne(p => p.UsuarioAnulacion)
                    .HasForeignKey(p => p.UsuarioAnulacionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Cliente
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => c.Dni).IsUnique();
                entity.HasIndex(c => c.Email).IsUnique();
                entity.Property(c => c.Dni).HasMaxLength(10).IsRequired();
                entity.Property(c => c.Nombre).HasMaxLength(50).IsRequired();
                entity.Property(c => c.Apellido).HasMaxLength(50).IsRequired();
                entity.Property(c => c.Telefono).HasMaxLength(20).IsRequired();
                entity.Property(c => c.Email).HasMaxLength(100).IsRequired();
                entity.Property(c => c.Direccion).HasMaxLength(200);
                entity.Property(c => c.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(c => c.Activo).HasDefaultValue(true);

                entity.HasMany(c => c.Vehiculos)
                    .WithOne(v => v.Cliente)
                    .HasForeignKey(v => v.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Vehiculo
            modelBuilder.Entity<Vehiculo>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.HasIndex(v => v.Patente).IsUnique();
                entity.Property(v => v.Patente).HasMaxLength(10).IsRequired();
                entity.Property(v => v.Marca).HasMaxLength(50).IsRequired();
                entity.Property(v => v.Modelo).HasMaxLength(50).IsRequired();
                entity.Property(v => v.Color).HasMaxLength(30);
                entity.Property(v => v.Imagen).HasMaxLength(500);
                entity.Property(v => v.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(v => v.Activo).HasDefaultValue(true);

                entity.HasOne(v => v.Cliente)
                    .WithMany(c => c.Vehiculos)
                    .HasForeignKey(v => v.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // OrdenReparacion
            modelBuilder.Entity<OrdenReparacion>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.Descripcion).HasMaxLength(500).IsRequired();
                entity.Property(o => o.Estado).HasMaxLength(20).IsRequired();
                entity.Property(o => o.ManoDeObra).HasPrecision(18, 2);
                entity.Property(o => o.MontoTotal).HasPrecision(18, 2);
                entity.Property(o => o.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(o => o.Activo).HasDefaultValue(true);

                entity.HasOne(o => o.Vehiculo)
                    .WithMany(v => v.Ordenes)
                    .HasForeignKey(o => o.VehiculoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Mecanico)
                    .WithMany(u => u.OrdenesAsignadas)
                    .HasForeignKey(o => o.MecanicoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Repuesto
            modelBuilder.Entity<Repuesto>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.HasIndex(r => r.Codigo).IsUnique();
                entity.Property(r => r.Codigo).HasMaxLength(30).IsRequired();
                entity.Property(r => r.Descripcion).HasMaxLength(200).IsRequired();
                entity.Property(r => r.Proveedor).HasMaxLength(100);
                entity.Property(r => r.Precio).HasPrecision(18, 2);
                entity.Property(r => r.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(r => r.Activo).HasDefaultValue(true);
            });

            // DetalleReparacion
            modelBuilder.Entity<DetalleReparacion>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.PrecioUnitario).HasPrecision(18, 2);
                entity.Property(d => d.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(d => d.Orden)
                    .WithMany(o => o.Detalles)
                    .HasForeignKey(d => d.OrdenId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Repuesto)
                    .WithMany()
                    .HasForeignKey(d => d.RepuestoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Pago
            modelBuilder.Entity<Pago>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Concepto).HasMaxLength(150).IsRequired();
                entity.Property(p => p.Importe).HasPrecision(18, 2);
                entity.Property(p => p.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(p => p.Anulado).HasDefaultValue(false);

                entity.HasOne(p => p.Orden)
                    .WithMany(o => o.Pagos)
                    .HasForeignKey(p => p.OrdenId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}