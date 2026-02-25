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
using Clynic.Application.DTOs.Sucursales;
using Clynic.Application.DTOs.Usuarios;
using Clynic.Application.Validators;
using Clynic.Api.Middlewares;

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
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ICodigoVerificacionRepository, CodigoVerificacionRepository>();

// DI - Services
builder.Services.AddScoped<IClinicaService, ClinicaService>();
builder.Services.AddScoped<ISucursalService, SucursalService>();
builder.Services.AddScoped<IHorarioSucursalService, HorarioSucursalService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IVerificationCodeService, VerificationCodeService>();

// DI - Business Rules
builder.Services.AddScoped<ClinicaRules>();
builder.Services.AddScoped<SucursalRules>();
builder.Services.AddScoped<HorarioSucursalRules>();
builder.Services.AddScoped<UsuarioRules>();

// DI - Validators
builder.Services.AddScoped<IValidator<CreateClinicaDto>, CreateClinicaDtoValidator>();
builder.Services.AddScoped<IValidator<CreateSucursalDto>, CreateSucursalDtoValidator>();
builder.Services.AddScoped<IValidator<CreateHorarioSucursalDto>, CreateHorarioSucursalDtoValidator>();
builder.Services.AddScoped<IValidator<RegisterDto>, RegisterDtoValidator>();
builder.Services.AddScoped<IValidator<CreateUsuarioAdminDto>, CreateUsuarioAdminDtoValidator>();
builder.Services.AddScoped<IValidator<LoginDto>, LoginDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateUsuarioDto>, UpdateUsuarioDtoValidator>();
builder.Services.AddScoped<IValidator<ChangePasswordDto>, ChangePasswordDtoValidator>();
builder.Services.AddScoped<IValidator<ForgotPasswordDto>, ForgotPasswordDtoValidator>();
builder.Services.AddScoped<IValidator<ResetPasswordDto>, ResetPasswordDtoValidator>();

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
    options.AddPolicy("AdminOrDoctor", policy => policy.RequireRole("Admin", "Doctor"));
});

// Controllers y API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Clynic API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingresa el token JWT con el prefijo 'Bearer '"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
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

// Log de inicio
app.Logger.LogInformation("🚀 API iniciada en {Environment}", app.Environment.EnvironmentName);
app.Logger.LogInformation("📊 Swagger disponible en http://localhost:8080/swagger");
app.Logger.LogInformation("🗄️ Base de datos: {Database}", connectionString?.Split(";").FirstOrDefault(s => s.Contains("Database")));

app.Run();

public partial class Program;
