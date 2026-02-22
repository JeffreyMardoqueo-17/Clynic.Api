using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Clynic.Application.DTOs.Clinicas;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Application.Rules;
using Clynic.Application.Services;
using Clynic.Application.Validators;
using Clynic.Domain.Models;
using Clynic.Infrastructure.Data;
using Clynic.Infrastructure.Repositories;

namespace Clynic.Application.Tests;

public class ClinicaServiceTests
{
    [Fact]
    public async Task CrearAsync_DtoValido_DebeCrearClinica()
    {
        var repository = new FakeClinicaRepository();
        var validator = new ValidatorExitoso();
        var service = new ClinicaService(repository, validator);

        var dto = new CreateClinicaDto
        {
            Nombre = "  Clynic Centro  ",
            Telefono = "555-101-2020",
            Direccion = "  Calle 10 #123  "
        };

        var resultado = await service.CrearAsync(dto);

        Assert.True(resultado.Id > 0);
        Assert.Equal("Clynic Centro", resultado.Nombre);
        Assert.Equal("555-101-2020", resultado.Telefono);
        Assert.Equal("Calle 10 #123", resultado.Direccion);
        Assert.True(resultado.Activa);
    }

    [Fact]
    public async Task CrearAsync_DtoInvalido_DebeLanzarValidationException()
    {
        var repository = new FakeClinicaRepository();
        var validator = new ValidatorConError();
        var service = new ClinicaService(repository, validator);

        var dto = new CreateClinicaDto
        {
            Nombre = "X",
            Telefono = "123",
            Direccion = "A"
        };

        await Assert.ThrowsAsync<ValidationException>(() => service.CrearAsync(dto));
    }

    [Fact]
    public async Task Integracion_ClinicaServiceConValidatorReal_DebePersistirEnInMemory()
    {
        var options = new DbContextOptionsBuilder<ClynicDbContext>()
            .UseInMemoryDatabase($"app-test-{Guid.NewGuid()}")
            .Options;

        await using var dbContext = new ClynicDbContext(options);
        IClinicaRepository repository = new ClinicaRepository(dbContext);
        var rules = new ClinicaRules(repository);
        var validator = new CreateClinicaDtoValidator(rules);
        var service = new ClinicaService(repository, validator);

        var creada = await service.CrearAsync(new CreateClinicaDto
        {
            Nombre = "Clinica Integracion",
            Telefono = "+52 55 2222 3333",
            Direccion = "Av. Integracion 300"
        });

        var todas = await service.ObtenerTodasAsync();

        Assert.True(creada.Id > 0);
        Assert.Single(todas);
        Assert.Equal("Clinica Integracion", todas.First().Nombre);
    }

    private sealed class FakeClinicaRepository : IClinicaRepository
    {
        private readonly List<Clinica> _clinicas = [];
        private int _nextId = 1;

        public Task<IEnumerable<Clinica>> ObtenerTodasAsync()
            => Task.FromResult(_clinicas.Where(c => c.Activa).AsEnumerable());

        public Task<Clinica?> ObtenerPorIdAsync(int id)
            => Task.FromResult(_clinicas.FirstOrDefault(c => c.Id == id && c.Activa));

        public Task<Clinica> CrearAsync(Clinica clinica)
        {
            clinica.Id = _nextId++;
            clinica.Activa = true;
            clinica.FechaCreacion = DateTime.UtcNow;
            _clinicas.Add(clinica);
            return Task.FromResult(clinica);
        }

        public Task<Clinica> ActualizarAsync(Clinica clinica)
        {
            var index = _clinicas.FindIndex(c => c.Id == clinica.Id);
            if (index >= 0)
            {
                _clinicas[index] = clinica;
            }

            return Task.FromResult(clinica);
        }

        public Task<bool> EliminarAsync(int id)
        {
            var clinica = _clinicas.FirstOrDefault(c => c.Id == id);
            if (clinica is null)
            {
                return Task.FromResult(false);
            }

            clinica.Activa = false;
            return Task.FromResult(true);
        }

        public Task<bool> ExisteNombreAsync(string nombre, int? idExcluir = null)
        {
            var existe = _clinicas.Any(c =>
                c.Activa
                && c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)
                && (!idExcluir.HasValue || c.Id != idExcluir.Value));

            return Task.FromResult(existe);
        }

        public Task<bool> ExisteAsync(int id)
            => Task.FromResult(_clinicas.Any(c => c.Id == id && c.Activa));
    }

    private sealed class ValidatorExitoso : IValidator<CreateClinicaDto>
    {
        public Task<ValidationResult> ValidateAsync(IValidationContext context, CancellationToken cancellation = default)
            => Task.FromResult(new ValidationResult());

        public Task<ValidationResult> ValidateAsync(CreateClinicaDto instance, CancellationToken cancellation = default)
            => Task.FromResult(new ValidationResult());

        public ValidationResult Validate(IValidationContext context) => new();

        public ValidationResult Validate(CreateClinicaDto instance) => new();

        public IValidatorDescriptor CreateDescriptor() => throw new NotSupportedException();

        public bool CanValidateInstancesOfType(Type type) => type == typeof(CreateClinicaDto);
    }

    private sealed class ValidatorConError : IValidator<CreateClinicaDto>
    {
        public Task<ValidationResult> ValidateAsync(IValidationContext context, CancellationToken cancellation = default)
            => Task.FromResult(new ValidationResult([new ValidationFailure("Nombre", "Nombre inválido") ]));

        public Task<ValidationResult> ValidateAsync(CreateClinicaDto instance, CancellationToken cancellation = default)
            => Task.FromResult(new ValidationResult([new ValidationFailure("Nombre", "Nombre inválido") ]));

        public ValidationResult Validate(IValidationContext context)
            => new([new ValidationFailure("Nombre", "Nombre inválido") ]);

        public ValidationResult Validate(CreateClinicaDto instance)
            => new([new ValidationFailure("Nombre", "Nombre inválido") ]);

        public IValidatorDescriptor CreateDescriptor() => throw new NotSupportedException();

        public bool CanValidateInstancesOfType(Type type) => type == typeof(CreateClinicaDto);
    }
}
