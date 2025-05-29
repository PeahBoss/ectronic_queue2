using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ElectronicQueue.Application.Specializations;
using ElectronicQueue.Core.Entities;
using ElectronicQueue.Application;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace ElectronicQueue.Tests.Application.Specializations;

public class DeleteSpecializationCommandTests
{
    private static readonly Guid specializationId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private class FakeLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) => null!;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId,
            TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        { }
    }

    private class FakeSpecializationValidator : IValidator<Specialization>
    {
        private readonly bool _isValid;

        public FakeSpecializationValidator(bool isValid)
        {
            _isValid = isValid;
        }

        public Task<bool> ValidateAsync(Specialization entity, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_isValid);
        }
    }

    [Test]
    public async Task ExecuteAsync_WhenSpecializationExistsAndValidationPasses_DeletesSpecialization_Returns200()
    {
        var specialization = new Specialization { Id = new(specializationId), Name = "Test Specialization" };
        var repo = new FakeRepository<Specialization>(new[] { specialization });
        var validator = new FakeSpecializationValidator(true);
        var logger = new FakeLogger<DeleteSpecializationCommand>();

        var command = new DeleteSpecializationCommand(repo, validator, logger);
        var request = new ByIdRequest(specializationId);

        var response = await command.ExecuteAsync(request);

        Assert.That(response.Code, Is.EqualTo(200));
        Assert.That(repo.Db, Has.Count.EqualTo(0));
    }

    [Test]
    public async Task ExecuteAsync_WhenSpecializationNotFound_Returns404()
    {
        var repo = new FakeRepository<Specialization>();
        var command = new DeleteSpecializationCommand(repo, null, null);
        var request = new ByIdRequest(specializationId);

        var response = await command.ExecuteAsync(request);

        Assert.That(response.Code, Is.EqualTo(404));
    }

    [Test]
    public async Task ExecuteAsync_WhenValidationFails_Returns409()
    {
        var specialization = new Specialization { Id = new(specializationId), Name = "Test Specialization" };
        var repo = new FakeRepository<Specialization>(new[] { specialization });
        var validator = new FakeSpecializationValidator(false);
        var command = new DeleteSpecializationCommand(repo, validator, null);
        var request = new ByIdRequest(specializationId);

        var response = await command.ExecuteAsync(request);

        Assert.That(response.Code, Is.EqualTo(409));
        Assert.That(repo.Db, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task ExecuteAsync_WhenExceptionThrown_Returns500()
    {
        var repo = new ThrowingRepo();
        var command = new DeleteSpecializationCommand(repo, null, null);
        var request = new ByIdRequest(specializationId);

        var response = await command.ExecuteAsync(request);

        Assert.That(response.Code, Is.EqualTo(500));
    }

    private class ThrowingRepo : IRepository<Specialization>
    {
        public Task<Specialization?> GetOneAsync(Func<Specialization, bool> predicate, CancellationToken cancellationToken = default)
            => throw new Exception("Test exception");

        public Task AddAsync(Specialization entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddRangeAsync(IEnumerable<Specialization> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(Specialization entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveRangeAsync(IEnumerable<Specialization> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IEnumerable<Specialization>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<Specialization>>(Array.Empty<Specialization>());
        public Task<IEnumerable<Specialization>> GetAllAsync(Func<Specialization, bool> predicate, CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<Specialization>>(Array.Empty<Specialization>());
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddOrUpdateAsync(Specialization entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddOrUpdateRangeAsync(IEnumerable<Specialization> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public IQueryable<Specialization> Db => throw new NotImplementedException();
    }
}
