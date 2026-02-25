using Microsoft.EntityFrameworkCore;
using Clynic.Domain.Models;
using Clynic.Domain.Models.Enums;

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
        public DbSet<Clinica> Clinicas { get; set; } = null!;
        public DbSet<Sucursal> Sucursales { get; set; } = null!;
        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Paciente> Pacientes { get; set; } = null!;
        public DbSet<Servicio> Servicios { get; set; } = null!;
        public DbSet<HorarioSucursal> HorariosSucursal { get; set; } = null!;
        public DbSet<Cita> Citas { get; set; } = null!;
        public DbSet<CitaServicio> CitasServicio { get; set; } = null!;
        public DbSet<CodigoVerificacion> CodigosVerificacion { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== Configuración de entidades ==========
            ConfigureClinicas(modelBuilder);
            ConfigureSucursales(modelBuilder);
            ConfigureUsuarios(modelBuilder);
            ConfigurePacientes(modelBuilder);
            ConfigureServicios(modelBuilder);
            ConfigureHorariosSucursal(modelBuilder);
            ConfigureCitas(modelBuilder);
            ConfigureCitasServicio(modelBuilder);
            ConfigureCodigosVerificacion(modelBuilder);
        }

        private void ConfigureClinicas(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Clinica>(entity =>
            {
                // Nombre de tabla en SQL Server
                entity.ToTable("Clinica");

                // Clave primaria
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
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

                entity.HasMany(e => e.Sucursales)
                    .WithOne(e => e.Clinica)
                    .HasForeignKey(e => e.IdClinica);

                entity.HasMany(e => e.Usuarios)
                    .WithOne(e => e.Clinica)
                    .HasForeignKey(e => e.IdClinica);

                entity.HasMany(e => e.Servicios)
                    .WithOne(e => e.Clinica)
                    .HasForeignKey(e => e.IdClinica);

                entity.HasMany(e => e.Citas)
                    .WithOne(e => e.Clinica)
                    .HasForeignKey(e => e.IdClinica);
            });
        }

        private void ConfigureSucursales(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Sucursal>(entity =>
            {
                entity.ToTable("Sucursal");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.IdClinica)
                    .IsRequired();

                entity.Property(e => e.Nombre)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(e => e.Direccion)
                    .HasMaxLength(250)
                    .IsRequired();

                entity.Property(e => e.Activa)
                    .HasDefaultValue(true);

                entity.HasOne(e => e.Clinica)
                    .WithMany(c => c.Sucursales)
                    .HasForeignKey(e => e.IdClinica);

                entity.HasMany(e => e.Horarios)
                    .WithOne(e => e.Sucursal)
                    .HasForeignKey(e => e.IdSucursal);

                entity.HasMany(e => e.Citas)
                    .WithOne(e => e.Sucursal)
                    .HasForeignKey(e => e.IdSucursal);

                entity.HasMany(e => e.Usuarios)
                    .WithOne(e => e.Sucursal)
                    .HasForeignKey(e => e.IdSucursal)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }

        private void ConfigureUsuarios(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuario");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.IdClinica)
                    .IsRequired();

                entity.Property(e => e.IdSucursal)
                    .IsRequired(false);

                entity.Property(e => e.NombreCompleto)
                    .HasMaxLength(150);

                entity.Property(e => e.Correo)
                    .HasMaxLength(150);

                entity.Property(e => e.ClaveHash)
                    .HasMaxLength(300);

                entity.Property(e => e.Rol)
                    .HasConversion(
                        v => v.ToString(),
                        v => (UsuarioRol)Enum.Parse(typeof(UsuarioRol), v))
                    .HasMaxLength(50);

                entity.Property(e => e.Activo)
                    .HasDefaultValue(true);

                entity.Property(e => e.DebeCambiarClave)
                    .HasDefaultValue(false);

                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasMany(e => e.CitasComoDoctor)
                    .WithOne(e => e.Doctor)
                    .HasForeignKey(e => e.IdDoctor);
            });
        }

        private void ConfigurePacientes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Paciente>(entity =>
            {
                entity.ToTable("Paciente");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.NombreCompleto)
                    .HasMaxLength(150);

                entity.Property(e => e.Telefono)
                    .HasMaxLength(50);

                entity.Property(e => e.FechaRegistro)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasMany(e => e.Citas)
                    .WithOne(e => e.Paciente)
                    .HasForeignKey(e => e.IdPaciente);
            });
        }

        private void ConfigureServicios(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Servicio>(entity =>
            {
                entity.ToTable("Servicio");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.IdClinica)
                    .IsRequired();

                entity.Property(e => e.NombreServicio)
                    .HasMaxLength(150);

                entity.Property(e => e.DuracionMin)
                    .IsRequired();

                entity.Property(e => e.PrecioBase)
                    .HasColumnType("decimal(10,2)");

                entity.Property(e => e.Activo)
                    .HasDefaultValue(true);

                entity.HasMany(e => e.CitaServicios)
                    .WithOne(e => e.Servicio)
                    .HasForeignKey(e => e.IdServicio);
            });
        }

        private void ConfigureHorariosSucursal(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HorarioSucursal>(entity =>
            {
                entity.ToTable("HorarioSucursal");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.IdSucursal)
                    .IsRequired();

                entity.Property(e => e.DiaSemana);
                entity.Property(e => e.HoraInicio);
                entity.Property(e => e.HoraFin);
            });
        }

        private void ConfigureCitas(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cita>(entity =>
            {
                entity.ToTable("Cita");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.IdClinica)
                    .IsRequired();

                entity.Property(e => e.IdSucursal)
                    .IsRequired();

                entity.Property(e => e.IdPaciente)
                    .IsRequired();

                entity.Property(e => e.IdDoctor);

                entity.Property(e => e.FechaHoraInicioPlan)
                    .IsRequired();

                entity.Property(e => e.FechaHoraFinPlan)
                    .IsRequired();

                entity.Property(e => e.Estado)
                    .HasConversion(
                        v => v.ToString(),
                        v => (EstadoCita)Enum.Parse(typeof(EstadoCita), v))
                    .HasMaxLength(50)
                    .HasDefaultValue(EstadoCita.Pendiente);

                entity.Property(e => e.Notas)
                    .HasMaxLength(250);

                entity.Property(e => e.SubTotal)
                    .HasColumnType("decimal(10,2)")
                    .HasDefaultValue(0);

                entity.Property(e => e.TotalFinal)
                    .HasColumnType("decimal(10,2)")
                    .HasDefaultValue(0);

                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasMany(e => e.CitaServicios)
                    .WithOne(e => e.Cita)
                    .HasForeignKey(e => e.IdCita);
            });
        }

        private void ConfigureCitasServicio(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CitaServicio>(entity =>
            {
                entity.ToTable("CitaServicio");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.IdCita)
                    .IsRequired();

                entity.Property(e => e.IdServicio)
                    .IsRequired();

                entity.Property(e => e.DuracionMin);

                entity.Property(e => e.Precio)
                    .HasColumnType("decimal(10,2)");
            });
        }

        private void ConfigureCodigosVerificacion(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CodigoVerificacion>(entity =>
            {
                entity.ToTable("CodigoVerificacion");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.IdUsuario)
                    .IsRequired();

                entity.Property(e => e.Codigo)
                    .HasMaxLength(12)
                    .IsRequired();

                entity.Property(e => e.Tipo)
                    .HasConversion(
                        v => v.ToString(),
                        v => (TipoCodigo)Enum.Parse(typeof(TipoCodigo), v))
                    .HasMaxLength(50);

                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.FechaExpiracion)
                    .IsRequired();

                entity.Property(e => e.Usado)
                    .HasDefaultValue(false);

                entity.HasOne(e => e.Usuario)
                    .WithMany()
                    .HasForeignKey(e => e.IdUsuario)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
