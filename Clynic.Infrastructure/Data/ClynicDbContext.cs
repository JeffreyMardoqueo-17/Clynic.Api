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
        public DbSet<Rol> Roles { get; set; } = null!;
        public DbSet<Especialidad> Especialidades { get; set; } = null!;
        public DbSet<RolEspecialidad> RolesEspecialidad { get; set; } = null!;
        public DbSet<SucursalEspecialidad> SucursalesEspecialidad { get; set; } = null!;
        public DbSet<Paciente> Pacientes { get; set; } = null!;
        public DbSet<HistorialClinico> HistorialesClinicos { get; set; } = null!;
        public DbSet<Servicio> Servicios { get; set; } = null!;
        public DbSet<HorarioSucursal> HorariosSucursal { get; set; } = null!;
        public DbSet<AsuetoSucursal> AsuetosSucursal { get; set; } = null!;
        public DbSet<Cita> Citas { get; set; } = null!;
        public DbSet<CitaServicio> CitasServicio { get; set; } = null!;
        public DbSet<ConsultaMedica> ConsultasMedicas { get; set; } = null!;
        public DbSet<CodigoVerificacion> CodigosVerificacion { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== Configuración de entidades ==========
            ConfigureClinicas(modelBuilder);
            ConfigureSucursales(modelBuilder);
            ConfigureRoles(modelBuilder);
            ConfigureEspecialidades(modelBuilder);
            ConfigureRolesEspecialidad(modelBuilder);
            ConfigureSucursalesEspecialidad(modelBuilder);
            ConfigureUsuarios(modelBuilder);
            ConfigurePacientes(modelBuilder);
            ConfigureHistorialesClinicos(modelBuilder);
            ConfigureServicios(modelBuilder);
            ConfigureHorariosSucursal(modelBuilder);
            ConfigureAsuetosSucursal(modelBuilder);
            ConfigureCitas(modelBuilder);
            ConfigureCitasServicio(modelBuilder);
            ConfigureConsultasMedicas(modelBuilder);
            ConfigureCodigosVerificacion(modelBuilder);
        }

        private void ConfigureRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rol>(entity =>
            {
                entity.ToTable("Rol");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.IdClinica)
                    .IsRequired(false);

                entity.Property(e => e.IdSucursal)
                    .IsRequired(false);

                entity.Property(e => e.Nombre)
                    .HasMaxLength(80)
                    .IsRequired();

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(250);

                entity.Property(e => e.Activo)
                    .HasDefaultValue(true);

                entity.HasIndex(e => e.Nombre)
                    .IsUnique();

                entity.HasOne(e => e.Clinica)
                    .WithMany(c => c.Roles)
                    .HasForeignKey(e => e.IdClinica)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Sucursal)
                    .WithMany(s => s.Roles)
                    .HasForeignKey(e => e.IdSucursal)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureEspecialidades(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Especialidad>(entity =>
            {
                entity.ToTable("Especialidad");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.IdClinica)
                    .IsRequired(false);

                entity.Property(e => e.Nombre)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(400);

                entity.Property(e => e.Activa)
                    .HasDefaultValue(true);

                entity.HasIndex(e => e.Nombre)
                    .IsUnique();

                entity.HasOne(e => e.Clinica)
                    .WithMany(c => c.Especialidades)
                    .HasForeignKey(e => e.IdClinica)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureRolesEspecialidad(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RolEspecialidad>(entity =>
            {
                entity.ToTable("RolEspecialidad");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.IdRol)
                    .IsRequired();

                entity.Property(e => e.IdEspecialidad)
                    .IsRequired();

                entity.Property(e => e.Activa)
                    .HasDefaultValue(true);

                entity.HasIndex(e => new { e.IdRol, e.IdEspecialidad })
                    .IsUnique();

                entity.HasOne(e => e.Rol)
                    .WithMany(r => r.RolesEspecialidad)
                    .HasForeignKey(e => e.IdRol)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Especialidad)
                    .WithMany(es => es.RolesEspecialidad)
                    .HasForeignKey(e => e.IdEspecialidad)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureSucursalesEspecialidad(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SucursalEspecialidad>(entity =>
            {
                entity.ToTable("SucursalEspecialidad");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.IdSucursal)
                    .IsRequired();

                entity.Property(e => e.IdEspecialidad)
                    .IsRequired();

                entity.Property(e => e.Activa)
                    .HasDefaultValue(true);

                entity.HasIndex(e => new { e.IdSucursal, e.IdEspecialidad })
                    .IsUnique();

                entity.HasOne(e => e.Sucursal)
                    .WithMany(s => s.Especialidades)
                    .HasForeignKey(e => e.IdSucursal)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Especialidad)
                    .WithMany(e => e.SucursalesEspecialidad)
                    .HasForeignKey(e => e.IdEspecialidad)
                    .OnDelete(DeleteBehavior.Restrict);
            });
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

                entity.HasMany(e => e.Roles)
                    .WithOne(e => e.Clinica)
                    .HasForeignKey(e => e.IdClinica);

                entity.HasMany(e => e.Especialidades)
                    .WithOne(e => e.Clinica)
                    .HasForeignKey(e => e.IdClinica);

                entity.HasMany(e => e.Servicios)
                    .WithOne(e => e.Clinica)
                    .HasForeignKey(e => e.IdClinica);

                entity.HasMany(e => e.Citas)
                    .WithOne(e => e.Clinica)
                    .HasForeignKey(e => e.IdClinica);

                entity.HasMany(e => e.Pacientes)
                    .WithOne(e => e.Clinica)
                    .HasForeignKey(e => e.IdClinica)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(e => e.ConsultasMedicas)
                    .WithOne(e => e.Clinica)
                    .HasForeignKey(e => e.IdClinica)
                    .OnDelete(DeleteBehavior.NoAction);
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

                entity.HasMany(e => e.Asuetos)
                    .WithOne(e => e.Sucursal)
                    .HasForeignKey(e => e.IdSucursal);

                entity.HasMany(e => e.Citas)
                    .WithOne(e => e.Sucursal)
                    .HasForeignKey(e => e.IdSucursal);

                entity.HasMany(e => e.ConsultasMedicas)
                    .WithOne(e => e.Sucursal)
                    .HasForeignKey(e => e.IdSucursal)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(e => e.Usuarios)
                    .WithOne(e => e.Sucursal)
                    .HasForeignKey(e => e.IdSucursal)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(e => e.Especialidades)
                    .WithOne(e => e.Sucursal)
                    .HasForeignKey(e => e.IdSucursal)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Roles)
                    .WithOne(e => e.Sucursal)
                    .HasForeignKey(e => e.IdSucursal)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureAsuetosSucursal(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AsuetoSucursal>(entity =>
            {
                entity.ToTable("AsuetoSucursal");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.IdSucursal)
                    .IsRequired();

                entity.Property(e => e.Fecha)
                    .IsRequired();

                entity.Property(e => e.Motivo)
                    .HasMaxLength(200)
                    .IsRequired(false);

                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => new { e.IdSucursal, e.Fecha })
                    .IsUnique();
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

                entity.HasIndex(e => new { e.IdClinica, e.Correo })
                    .IsUnique();

                entity.Property(e => e.ClaveHash)
                    .HasMaxLength(300);

                entity.Property(e => e.IdRol)
                    .IsRequired();

                entity.Property(e => e.IdEspecialidad)
                    .IsRequired(false);

                entity.Property(e => e.Activo)
                    .HasDefaultValue(true);

                entity.Property(e => e.DebeCambiarClave)
                    .HasDefaultValue(false);

                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasMany(e => e.CitasComoDoctor)
                    .WithOne(e => e.Doctor)
                    .HasForeignKey(e => e.IdDoctor);

                entity.HasMany(e => e.ConsultasMedicasRealizadas)
                    .WithOne(e => e.Doctor)
                    .HasForeignKey(e => e.IdDoctor)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Rol)
                    .WithMany(r => r.Usuarios)
                    .HasForeignKey(e => e.IdRol)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Especialidad)
                    .WithMany(es => es.Usuarios)
                    .HasForeignKey(e => e.IdEspecialidad)
                    .OnDelete(DeleteBehavior.SetNull);
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

                entity.Property(e => e.IdClinica)
                    .IsRequired();

                entity.Property(e => e.Nombres)
                    .HasMaxLength(150);

                entity.Property(e => e.Apellidos)
                    .HasMaxLength(150);

                entity.Property(e => e.Telefono)
                    .HasMaxLength(50);

                entity.Property(e => e.Correo)
                    .HasMaxLength(150);

                entity.Property(e => e.IdEspecialidad)
                    .IsRequired(false);

                entity.Property(e => e.FechaNacimiento)
                    .IsRequired(false);

                entity.Property(e => e.FechaRegistro)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => new { e.IdClinica, e.Correo });

                entity.HasOne(e => e.HistorialClinico)
                    .WithOne(e => e.Paciente)
                    .HasForeignKey<HistorialClinico>(e => e.IdPaciente)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Citas)
                    .WithOne(e => e.Paciente)
                    .HasForeignKey(e => e.IdPaciente);

                entity.HasMany(e => e.ConsultasMedicas)
                    .WithOne(e => e.Paciente)
                    .HasForeignKey(e => e.IdPaciente)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Especialidad)
                    .WithMany(es => es.Pacientes)
                    .HasForeignKey(e => e.IdEspecialidad)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }

        private void ConfigureHistorialesClinicos(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HistorialClinico>(entity =>
            {
                entity.ToTable("HistorialClinico");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.IdPaciente)
                    .IsRequired();

                entity.Property(e => e.EnfermedadesPrevias)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.MedicamentosActuales)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.Alergias)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.AntecedentesFamiliares)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.Observaciones)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.FechaActualizacion)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.IdPaciente)
                    .IsUnique();
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

                entity.HasOne(e => e.ConsultaMedica)
                    .WithOne(e => e.Cita)
                    .HasForeignKey<ConsultaMedica>(e => e.IdCita)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.CitaServicios)
                    .WithOne(e => e.Cita)
                    .HasForeignKey(e => e.IdCita);

                entity.HasOne(e => e.Especialidad)
                    .WithMany(e => e.Citas)
                    .HasForeignKey(e => e.IdEspecialidad)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.IdClinica, e.IdSucursal, e.FechaHoraInicioPlan, e.Estado })
                    .HasDatabaseName("IX_Cita_BusquedaSolape");

                entity.HasIndex(e => new { e.IdClinica, e.IdSucursal, e.IdEspecialidad, e.FechaHoraInicioPlan })
                    .HasDatabaseName("IX_Cita_Especialidad_Fecha");
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

        private void ConfigureConsultasMedicas(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConsultaMedica>(entity =>
            {
                entity.ToTable("ConsultaMedica");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.IdCita)
                    .IsRequired();

                entity.Property(e => e.IdClinica)
                    .IsRequired();

                entity.Property(e => e.IdSucursal)
                    .IsRequired();

                entity.Property(e => e.IdPaciente)
                    .IsRequired();

                entity.Property(e => e.Diagnostico)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.Tratamiento)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.Receta)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.ExamenesSolicitados)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.NotasMedicas)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.FechaConsulta)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.IdCita)
                    .IsUnique();

                entity.HasIndex(e => new { e.IdClinica, e.IdPaciente, e.FechaConsulta });
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
