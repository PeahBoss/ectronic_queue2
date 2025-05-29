using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ElectronicQueue.Application.Appointments;
using ElectronicQueue.Core.Entities;
using ElectronicQueue.Application;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace ElectronicQueue.Tests.Application.Appointments;

public class DeleteAppointmentCommandTests
{
    private static readonly Guid appointmentId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private class FakeLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) => null!;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId,
            TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        { }
    }

    private class FakeAppointmentValidator : IValidator<Appointment>
    {
        private readonly bool _isValid;

        public FakeAppointmentValidator(bool isValid)
        {
            _isValid = isValid;
        }

        public Task<bool> ValidateAsync(Appointment entity, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_isValid);
        }
    }

    [Test]
    public async Task ExecuteAsync_WhenAppointmentExistsAndValidationPasses_DeletesAppointment_Returns200()
    {
        var appointment = new Appointment { Id = new(appointmentId) };
        var repo = new FakeRepository<Appointment>(new[] { appointment });
        var validator = new FakeAppointmentValidator(true);
        var logger = new FakeLogger<DeleteAppointmentCommand>();

        var command = new DeleteAppointmentCommand(repo, validator, logger);
        var request = new ByIdRequest(appointmentId);

        var response = await command.ExecuteAsync(request);

        Assert.That(response.Code, Is.EqualTo(200));
        Assert.That(repo.Db, Has.Count.EqualTo(0));
    }

    [Test]
    public async Task ExecuteAsync_WhenAppointmentNotFound_Returns404()
    {
        var repo = new FakeRepository<Appointment>(); // пустой репозиторий
        var command = new DeleteAppointmentCommand(repo, null, null);
        var request = new ByIdRequest(appointmentId);

        var response = await command.ExecuteAsync(request);

        Assert.That(response.Code, Is.EqualTo(404));
    }

    [Test]
    public async Task ExecuteAsync_WhenValidationFails_Returns409()
    {
        var appointment = new Appointment { Id = new(appointmentId) };
        var repo = new FakeRepository<Appointment>(new[] { appointment });
        var validator = new FakeAppointmentValidator(false); // Валидация не прошла
        var command = new DeleteAppointmentCommand(repo, validator, null);
        var request = new ByIdRequest(appointmentId);

        var response = await command.ExecuteAsync(request);

        Assert.That(response.Code, Is.EqualTo(409));
        Assert.That(repo.Db, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task ExecuteAsync_WhenExceptionThrown_Returns500()
    {
        var repo = new ThrowingRepo();
        var command = new DeleteAppointmentCommand(repo, null, null);
        var request = new ByIdRequest(appointmentId);

        var response = await command.ExecuteAsync(request);

        Assert.That(response.Code, Is.EqualTo(500));
    }

    private class ThrowingRepo : IRepository<Appointment>
    {
        public Task<Appointment?> GetOneAsync(Func<Appointment, bool> predicate, CancellationToken cancellationToken = default)
            => throw new Exception("Test exception");

        public Task AddAsync(Appointment entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddRangeAsync(IEnumerable<Appointment> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(Appointment entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveRangeAsync(IEnumerable<Appointment> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IEnumerable<Appointment>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<Appointment>>(Array.Empty<Appointment>());
        public Task<IEnumerable<Appointment>> GetAllAsync(Func<Appointment, bool> predicate, CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<Appointment>>(Array.Empty<Appointment>());
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddOrUpdateAsync(Appointment entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddOrUpdateRangeAsync(IEnumerable<Appointment> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public IQueryable<Appointment> Db => throw new NotImplementedException();
    }
}
