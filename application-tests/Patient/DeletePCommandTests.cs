using System;
using System.Threading;
using System.Threading.Tasks;
using ElectronicQueue.Application.Patients;
using ElectronicQueue.Core.Entities;
using ElectronicQueue.Application;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace ElectronicQueue.Tests.Application.Patients;

public class DeletePatientCommandTests
{
    private static readonly Guid patientId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private class FakeLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) => null!;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId,
            TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        { }
    }

    private class FakePatientValidator : IValidator<Patient>
    {
        private readonly bool _isValid;

        public FakePatientValidator(bool isValid)
        {
            _isValid = isValid;
        }

        public Task<bool> ValidateAsync(Patient entity, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_isValid);
        }
    }

    [Test]
    public async Task ExecuteAsync_WhenPatientExistsAndValidationPasses_DeletesPatient_Returns200()
    {
        // Arrange
        var patient = new Patient { Id = new(patientId), Name = "Test Patient" };
        var repo = new FakeRepository<Patient>(new[] { patient });
        var validator = new FakePatientValidator(true);
        var logger = new FakeLogger<DeletePatientCommand>();

        var command = new DeletePatientCommand(repo, validator, logger);
        var request = new ByIdRequest(patientId);

        // Act
        var response = await command.ExecuteAsync(request);

        // Assert
        Assert.That(response.Code, Is.EqualTo(200));
        Assert.That(repo.Db, Has.Count.EqualTo(0)); // Пациент удалён
    }

    [Test]
    public async Task ExecuteAsync_WhenPatientNotFound_Returns404()
    {
        // Arrange
        var repo = new FakeRepository<Patient>(); // пустой репозиторий
        var command = new DeletePatientCommand(repo, null, null);
        var request = new ByIdRequest(patientId);

        // Act
        var response = await command.ExecuteAsync(request);

        // Assert
        Assert.That(response.Code, Is.EqualTo(404));
    }

    [Test]
    public async Task ExecuteAsync_WhenValidationFails_Returns409()
    {
        // Arrange
        var patient = new Patient { Id = new(patientId), Name = "Test Patient" };
        var repo = new FakeRepository<Patient>(new[] { patient });
        var validator = new FakePatientValidator(false); // Валидация не прошла
        var command = new DeletePatientCommand(repo, validator, null);
        var request = new ByIdRequest(patientId);

        // Act
        var response = await command.ExecuteAsync(request);

        // Assert
        Assert.That(response.Code, Is.EqualTo(409));
        Assert.That(repo.Db, Has.Count.EqualTo(1)); // Пациент не удалён
    }

    [Test]
    public async Task ExecuteAsync_WhenExceptionThrown_Returns500()
    {
        // Arrange
        var repo = new ThrowingRepo();
        var command = new DeletePatientCommand(repo, null, null);
        var request = new ByIdRequest(patientId);

        // Act
        var response = await command.ExecuteAsync(request);

        // Assert
        Assert.That(response.Code, Is.EqualTo(500));
    }

    // Репозиторий, который выбрасывает исключение для теста обработки ошибок
    private class ThrowingRepo : IRepository<Patient>
    {
        public Task<Patient?> GetOneAsync(Func<Patient, bool> predicate, CancellationToken cancellationToken = default)
            => throw new Exception("Test exception");

        public Task AddAsync(Patient entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddRangeAsync(IEnumerable<Patient> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(Patient entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveRangeAsync(IEnumerable<Patient> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IEnumerable<Patient>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<Patient>>(Array.Empty<Patient>());
        public Task<IEnumerable<Patient>> GetAllAsync(Func<Patient, bool> predicate, CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<Patient>>(Array.Empty<Patient>());
        public Task SaveChanges(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddOrUpdateAsync(Patient entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddOrUpdateRangeAsync(IEnumerable<Patient> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public IQueryable<Patient> Db => throw new NotImplementedException();
    }
}
