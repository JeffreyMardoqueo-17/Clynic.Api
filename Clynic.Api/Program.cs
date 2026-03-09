using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using Clynic.Infrastructure.Data;
using Clynic.Infrastructure.Repositories;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Application.Interfaces.Services;
using Clynic.Application.Services;
using Clynic.Application.Rules;
using Clynic.Application.DTOs.Clinicas;
using Clynic.Application.DTOs.HorariosSucursal;
using Clynic.Application.DTOs.Servicios;
using Clynic.Application.DTOs.Sucursales;
using Clynic.Application.DTOs.Usuarios;
using Clynic.Application.DTOs.Citas;
using Clynic.Application.DTOs.CitaServicios;
using Clynic.Application.DTOs.HistorialesClinicos;
using Clynic.Application.DTOs.Pacientes;
using Clynic.Application.Validators;
using Clynic.Api.Middlewares;
using Clynic.Api.Hubs;
using Clynic.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ========== Configurar servicios ==========

// Entity Framework Core 8.0 con SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found. Set it via environment variable " +
        "ConnectionStrings__DefaultConnection or user secrets.");
}
builder.Services.AddDbContext<ClynicDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// DI - Repositories
builder.Services.AddScoped<IClinicaRepository, ClinicaRepository>();
builder.Services.AddScoped<ISucursalRepository, SucursalRepository>();
builder.Services.AddScoped<IHorarioSucursalRepository, HorarioSucursalRepository>();
builder.Services.AddScoped<IAsuetoSucursalRepository, AsuetoSucursalRepository>();
builder.Services.AddScoped<IServicioRepository, ServicioRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IRolRepository, RolRepository>();
builder.Services.AddScoped<IRolEspecialidadRepository, RolEspecialidadRepository>();
builder.Services.AddScoped<IEspecialidadRepository, EspecialidadRepository>();
builder.Services.AddScoped<ISucursalEspecialidadRepository, SucursalEspecialidadRepository>();
builder.Services.AddScoped<ICodigoVerificacionRepository, CodigoVerificacionRepository>();
builder.Services.AddScoped<IPacienteRepository, PacienteRepository>();
builder.Services.AddScoped<ICitaRepository, CitaRepository>();
builder.Services.AddScoped<ICitaServicioRepository, CitaServicioRepository>();
builder.Services.AddScoped<IHistorialClinicoRepository, HistorialClinicoRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// DI - Services
builder.Services.AddScoped<IClinicaService, ClinicaService>();
builder.Services.AddScoped<ISucursalService, SucursalService>();
builder.Services.AddScoped<IHorarioSucursalService, HorarioSucursalService>();
builder.Services.AddScoped<IServicioService, ServicioService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ICatalogoPersonalService, CatalogoPersonalService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IVerificationCodeService, VerificationCodeService>();
builder.Services.AddScoped<IPacienteService, PacienteService>();
builder.Services.AddScoped<ICitaService, CitaService>();
builder.Services.AddScoped<ICitaServicioService, CitaServicioService>();
builder.Services.AddScoped<IHistorialClinicoService, HistorialClinicoService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IDoctorNotificationService, DoctorNotificationService>();

// DI - Business Rules
builder.Services.AddScoped<ClinicaRules>();
builder.Services.AddScoped<SucursalRules>();
builder.Services.AddScoped<HorarioSucursalRules>();
builder.Services.AddScoped<ServicioRules>();
builder.Services.AddScoped<UsuarioRules>();
builder.Services.AddScoped<PacienteRules>();
builder.Services.AddScoped<CitaRules>();

// DI - Validators
builder.Services.AddScoped<IValidator<CreateClinicaDto>, CreateClinicaDtoValidator>();
builder.Services.AddScoped<IValidator<CreateSucursalDto>, CreateSucursalDtoValidator>();
builder.Services.AddScoped<IValidator<CreateHorarioSucursalDto>, CreateHorarioSucursalDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateHorarioSucursalDto>, UpdateHorarioSucursalDtoValidator>();
builder.Services.AddScoped<IValidator<CreateAsuetoSucursalDto>, CreateAsuetoSucursalDtoValidator>();
builder.Services.AddScoped<IValidator<CreateServicioDto>, CreateServicioDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateServicioDto>, UpdateServicioDtoValidator>();
builder.Services.AddScoped<IValidator<RegisterDto>, RegisterDtoValidator>();
builder.Services.AddScoped<IValidator<RegisterClinicDto>, RegisterClinicDtoValidator>();
builder.Services.AddScoped<IValidator<CreateUsuarioAdminDto>, CreateUsuarioAdminDtoValidator>();
builder.Services.AddScoped<IValidator<LoginDto>, LoginDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateUsuarioDto>, UpdateUsuarioDtoValidator>();
builder.Services.AddScoped<IValidator<ChangePasswordDto>, ChangePasswordDtoValidator>();
builder.Services.AddScoped<IValidator<ForgotPasswordDto>, ForgotPasswordDtoValidator>();
builder.Services.AddScoped<IValidator<ResetPasswordDto>, ResetPasswordDtoValidator>();
builder.Services.AddScoped<IValidator<CreateCitaPublicaDto>, CreateCitaPublicaDtoValidator>();
builder.Services.AddScoped<IValidator<CreateCitaInternaDto>, CreateCitaInternaDtoValidator>();
builder.Services.AddScoped<IValidator<RegistrarConsultaMedicaDto>, RegistrarConsultaMedicaDtoValidator>();
builder.Services.AddScoped<IValidator<CreateCitaServicioDto>, CreateCitaServicioDtoValidator>();
builder.Services.AddScoped<IValidator<UpdatePacienteDto>, UpdatePacienteDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateHistorialClinicoDto>, UpdateHistorialClinicoDtoValidator>();
builder.Services.AddScoped<IValidator<UpsertHistorialClinicoDto>, UpsertHistorialClinicoDtoValidator>();

// JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] 
    ?? throw new InvalidOperationException("Jwt:SecretKey no está configurado");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "ClynicAPI",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "ClynicClients",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrWhiteSpace(accessToken) && path.StartsWithSegments("/hubs/doctor-queue"))
            {
                context.Token = accessToken;
                return Task.CompletedTask;
            }

            if (string.IsNullOrWhiteSpace(context.Token) &&
                context.Request.Cookies.TryGetValue("clynic_access_token", out var cookieToken))
            {
                context.Token = cookieToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("AdminOrDoctor", policy => policy.RequireRole("Admin", "Doctor", "Nutricionista", "Fisioterapeuta"));
});

// Controllers y API
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Clynic API", Version = "v1" });
    // Evita colisiones cuando existen DTOs con el mismo nombre en namespaces distintos.
    c.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);
    
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingresa el token JWT con el prefijo 'Bearer '"
    });

    c.OperationFilter<AuthorizeOperationFilter>();
});

// CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000", "https://clynic-sys.vercel.app" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ========== Construir aplicación ==========
var app = builder.Build();

// ========== Aplicar migraciones automáticamente (solo Development) ==========
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ClynicDbContext>();
    
    try
    {
        dbContext.Database.EnsureCreated();
        dbContext.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'AsuetoSucursal', N'U') IS NULL
BEGIN
    CREATE TABLE AsuetoSucursal (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IdSucursal INT NOT NULL,
        Fecha DATE NOT NULL,
        Motivo NVARCHAR(200) NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_AsuetoSucursal_Sucursal FOREIGN KEY (IdSucursal) REFERENCES Sucursal(Id)
    );

    CREATE UNIQUE INDEX UX_AsuetoSucursal_Sucursal_Fecha ON AsuetoSucursal(IdSucursal, Fecha);
END

IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_Cita_Clinica_Sucursal_FechaHoraInicio_Activa'
      AND object_id = OBJECT_ID('Cita')
)
BEGIN
    DROP INDEX UX_Cita_Clinica_Sucursal_FechaHoraInicio_Activa ON Cita;
END

IF OBJECT_ID(N'Rol', N'U') IS NULL
BEGIN
    CREATE TABLE Rol (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IdClinica INT NULL,
        IdSucursal INT NULL,
        Nombre NVARCHAR(80) NOT NULL,
        Descripcion NVARCHAR(250) NULL,
        Activo BIT NOT NULL DEFAULT 1
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Rol_Nombre' AND object_id = OBJECT_ID('Rol'))
BEGIN
    CREATE UNIQUE INDEX UX_Rol_Nombre ON Rol(Nombre);
END

IF OBJECT_ID(N'Especialidad', N'U') IS NULL
BEGIN
    CREATE TABLE Especialidad (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IdClinica INT NULL,
        Nombre NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(400) NULL,
        Activa BIT NOT NULL DEFAULT 1
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Especialidad_Nombre' AND object_id = OBJECT_ID('Especialidad'))
BEGIN
    CREATE UNIQUE INDEX UX_Especialidad_Nombre ON Especialidad(Nombre);
END

IF OBJECT_ID(N'RolEspecialidad', N'U') IS NULL
BEGIN
    CREATE TABLE RolEspecialidad (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IdRol INT NOT NULL,
        IdEspecialidad INT NOT NULL,
        Activa BIT NOT NULL DEFAULT 1,
        CONSTRAINT UX_RolEspecialidad_Rol_Especialidad UNIQUE (IdRol, IdEspecialidad),
        CONSTRAINT FK_RolEspecialidad_Rol FOREIGN KEY (IdRol) REFERENCES Rol(Id) ON DELETE CASCADE,
        CONSTRAINT FK_RolEspecialidad_Especialidad FOREIGN KEY (IdEspecialidad) REFERENCES Especialidad(Id) ON DELETE CASCADE
    );
END

IF OBJECT_ID(N'Usuario', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('Usuario', 'IdRol') IS NULL
    BEGIN
        ALTER TABLE Usuario ADD IdRol INT NULL;
    END

    IF COL_LENGTH('Usuario', 'IdEspecialidad') IS NULL
    BEGIN
        ALTER TABLE Usuario ADD IdEspecialidad INT NULL;
    END
END

IF NOT EXISTS (SELECT 1 FROM Rol WHERE LOWER(Nombre) = 'admin')
BEGIN
    INSERT INTO Rol (IdClinica, IdSucursal, Nombre, Descripcion, Activo)
    VALUES (NULL, NULL, 'Admin', 'Usuario global administrador', 1);
END

IF NOT EXISTS (SELECT 1 FROM Especialidad WHERE LOWER(Nombre) = 'encargado global')
BEGIN
    INSERT INTO Especialidad (IdClinica, Nombre, Descripcion, Activa)
    VALUES (NULL, 'Encargado Global', 'Especialidad global para administradores', 1);
END

DECLARE @AdminRolId INT;
DECLARE @EspecialidadAdminId INT;

SELECT TOP 1 @AdminRolId = Id FROM Rol WHERE LOWER(Nombre) = 'admin';
SELECT TOP 1 @EspecialidadAdminId = Id FROM Especialidad WHERE LOWER(Nombre) = 'encargado global';

IF @AdminRolId IS NOT NULL AND @EspecialidadAdminId IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
       FROM RolEspecialidad
       WHERE IdRol = @AdminRolId
         AND IdEspecialidad = @EspecialidadAdminId
   )
BEGIN
    INSERT INTO RolEspecialidad (IdRol, IdEspecialidad, Activa)
    VALUES (@AdminRolId, @EspecialidadAdminId, 1);
END

IF OBJECT_ID(N'Usuario', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
       FROM sys.indexes
       WHERE name = 'UX_Usuario_Clinica_Correo'
         AND object_id = OBJECT_ID('Usuario')
   )
   AND NOT EXISTS (
       SELECT 1
       FROM Usuario
       GROUP BY IdClinica, Correo
       HAVING COUNT(*) > 1
   )
BEGIN
    CREATE UNIQUE INDEX UX_Usuario_Clinica_Correo ON Usuario(IdClinica, Correo);
END
");
        app.Logger.LogInformation("✅ Base de datos lista");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error con base de datos");
    }
}

// ========== Configurar pipeline HTTP ==========
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Clynic API v1"));
}

app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<DoctorQueueHub>("/hubs/doctor-queue");

// Log de inicio
app.Logger.LogInformation("🚀 API iniciada en {Environment}", app.Environment.EnvironmentName);
app.Logger.LogInformation("📊 Swagger disponible en http://localhost:8080/swagger");
app.Logger.LogInformation("🗄️ Base de datos: {Database}", connectionString?.Split(";").FirstOrDefault(s => s.Contains("Database")));

app.Run();

public partial class Program;
