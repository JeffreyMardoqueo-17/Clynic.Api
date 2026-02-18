using Microsoft.EntityFrameworkCore;
using Clynic.Domain.Models;

namespace Clynic.Infrastructure.Data
{
    /// <summary>
    /// Contexto de Entity Framework para la aplicación Clynic
    /// </summary>
    public class ClynicDbContext : DbContext
    {
        public ClynicDbContext(DbContextOptions<ClynicDbContext> options)
            : base(options)
        {
        }

        // ========== DbSets (Tablas) ==========
        public DbSet<Clinicas> Clinicas { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== Configuración de entidades ==========
            ConfigureClinicas(modelBuilder);
        }

        private void ConfigureClinicas(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Clinicas>(entity =>
            {
                // Nombre de tabla en SQL Server
                entity.ToTable("Clinicas");

                // Clave primaria
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnName("IdClinica")
                    .ValueGeneratedOnAdd();

                // Propiedades
                entity.Property(e => e.Nombre)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(e => e.Telefono)
                    .HasMaxLength(50);

                entity.Property(e => e.Direccion)
                    .HasMaxLength(250);

                entity.Property(e => e.Activa)
                    .HasDefaultValue(true);

                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("GETDATE()");
            });
        }
    }
}
