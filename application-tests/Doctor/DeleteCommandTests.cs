using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ElectronicQueue.Application.Doctors;
using ElectronicQueue.Core.Entities;
using ElectronicQueue.Application;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace ElectronicQueue.Tests.Application.Doctors;

public class DeleteDoctorCommandTests
{
    private static readonly Guid doctorId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private class FakeLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) => null!;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId,
            TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        { }
    }

    private class FakeDoctorValidator : IValidator<Doctor>
    {
        private readonly bool _isValid;

        public FakeDoctorValidator(bool isValid)
        {
            _isValid = isValid;
        }

        public Task<bool> ValidateAsync(Doctor entity, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_isValid);
        }
    }

    [Test]
    public async Task ExecuteAsync_WhenDoctorExistsAndValidationPasses_DeletesDoctor_Returns200()
    {
        // Arrange
        var doctor = new Doctor { Id = new(doctorId), Name = "Test Doctor" };
        var repo = new FakeRepository<Doctor>(new[] { doctor });
        var validator = new FakeDoctorValidator(true);
        var logger = new FakeLogger<DeleteDoctorCommand>();

        var command = new DeleteDoctorCommand(repo, validator, logger);
        var request = new ByIdRequest(doctorId);

        // Act
        var response = await command.ExecuteAsync(request);

        // Assert
        Assert.That(response.Code, Is.EqualTo(200));
        Assert.That(repo.Db, Has.Count.EqualTo(0)); // Доктор удалён
    }

    [Test]
    public async Task ExecuteAsync_WhenDoctorNotFound_Returns404()
    {
        // Arrange
        var repo = new FakeRepository<Doctor>(); // пустой репозиторий
        var command = new DeleteDoctorCommand(repo, null, null);
        var request = new ByIdRequest(doctorId);

        // Act
        var response = await command.ExecuteAsync(request);

        // Assert
        Assert.That(response.Code, Is.EqualTo(404));
    }

    [Test]
    public async Task ExecuteAsync_WhenValidationFails_Returns409()
    {
        // Arrange
        var doctor = new Doctor { Id = new(doctorId), Name = "Test Doctor" };
        var repo = new FakeRepository<Doctor>(new[] { doctor });
        var validator = new FakeDoctorValidator(false); // Валидация не прошла
        var command = new DeleteDoctorCommand(repo, validator, null);
        var request = new ByIdRequest(doctorId);

        // Act
        var response = await command.ExecuteAsync(request);

        // Assert
        Assert.That(response.Code, Is.EqualTo(409));
        Assert.That(repo.Db, Has.Count.EqualTo(1)); // Доктор не удалён
    }

    [Test]
    public async Task ExecuteAsync_WhenExceptionThrown_Returns500()
    {
        // Arrange
        var repo = new ThrowingRepo();
        var command = new DeleteDoctorCommand(repo, null, null);
        var request = new ByIdRequest(doctorId);

        // Act
        var response = await command.ExecuteAsync(request);

        // Assert
        Assert.That(response.Code, Is.EqualTo(500));
    }

    // Репозиторий, который выбрасывает исключение для теста обработки ошибок
    private class ThrowingRepo : IRepository<Doctor>
    {
        public Task<Doctor?> GetOneAsync(Func<Doctor, bool> predicate, CancellationToken cancellationToken = default)
            => throw new Exception("Test exception");

        public Task AddAsync(Doctor entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddRangeAsync(IEnumerable<Doctor> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(Doctor entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveRangeAsync(IEnumerable<Doctor> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IEnumerable<Doctor>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<Doctor>>(Array.Empty<Doctor>());
        public Task<IEnumerable<Doctor>> GetAllAsync(Func<Doctor, bool> predicate, CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<Doctor>>(Array.Empty<Doctor>());
        public Task SaveChanges(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddOrUpdateAsync(Doctor entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddOrUpdateRangeAsync(IEnumerable<Doctor> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public IQueryable<Doctor> Db => throw new NotImplementedException();
    }
}
