using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Clynic.Infrastructure.Data;
using Clynic.Infrastructure.Repositories;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Application.Interfaces.Services;
using Clynic.Application.Services;
using Clynic.Application.Rules;
using Clynic.Application.DTOs.Clinicas;
using Clynic.Application.DTOs.Sucursales;
using Clynic.Application.Validators;

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

    // Solo en Development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// DI - Repositories
builder.Services.AddScoped<IClinicaRepository, ClinicaRepository>();
builder.Services.AddScoped<ISucursalRepository, SucursalRepository>();

// DI - Services
builder.Services.AddScoped<IClinicaService, ClinicaService>();
builder.Services.AddScoped<ISucursalService, SucursalService>();

// DI - Business Rules
builder.Services.AddScoped<ClinicaRules>();
builder.Services.AddScoped<SucursalRules>();

// DI - Validators
builder.Services.AddScoped<IValidator<CreateClinicaDto>, CreateClinicaDtoValidator>();
builder.Services.AddScoped<IValidator<CreateSucursalDto>, CreateSucursalDtoValidator>();

// Controllers y API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Clynic API", Version = "v1" });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
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
        // Crear BD si no existe
        dbContext.Database.EnsureCreated();
        app.Logger.LogInformation("✅ Base de datos lista");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error con base de datos");
    }
}

// ========== Configurar pipeline HTTP ==========
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Clynic API v1"));
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Log de inicio
app.Logger.LogInformation("🚀 API iniciada en {Environment}", app.Environment.EnvironmentName);
app.Logger.LogInformation("📊 Swagger disponible en http://localhost:8080/swagger");
app.Logger.LogInformation("🗄️ Base de datos: {Database}", connectionString?.Split(";").FirstOrDefault(s => s.Contains("Database")));

app.Run();
