using ElectronicQueue.Application;
using ElectronicQueue.Application.Specializations;

namespace ElectronicQueue.Tests.Application.Specializations;

public class AddOrUpdateSpecializationCommandTests
{
    private const string specializationId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";

    [TestCase(null, "Cardiology")]
    [TestCase(null, "Neurology")]
    public async Task ExecuteAsync_WhenAddingSpecializationThatAlreadyExists_Returns409(Guid? Id, string Name)
    {
        // Arrange
        FakeRepository<Specialization> specializationsRepository = new(new[]
        {
            new Specialization
            {
                Id = new(Id ?? Guid.NewGuid()),
                Name = Name,
                Doctors = Array.Empty<Doctor>()
            }
        });

        AddOrUpdateSpecializationCommand command = new(specializationsRepository);
        var request = new AddOrUpdateSpecializationRequest(Id, Name);

        // Act
        var response = await command.ExecuteAsync(request);

        // Assert
        Assert.That(response.Code, Is.EqualTo(409));
    }

    [TestCase(specializationId, "Updated Specialty")]
    public async Task ExecuteAsync_WhenUpdatingNonExistentSpecialization_Returns404(Guid Id, string Name)
    {
        // Arrange
        FakeRepository<Specialization> specializationsRepository = new(Array.Empty<Specialization>());

        AddOrUpdateSpecializationCommand command = new(specializationsRepository);
        var request = new AddOrUpdateSpecializationRequest(Id, Name);

        // Act
        var response = await command.ExecuteAsync(request);

        // Assert
        Assert.That(response.Code, Is.EqualTo(404));
    }

    [TestCase("Dermatology")]
    [TestCase("Pediatrics")]
    public async Task ExecuteAsync_WhenAddingValidSpecialization_Returns200_AndAddsSpecialization(string Name)
    {
        // Arrange
        FakeRepository<Specialization> specializationsRepository = new();

        AddOrUpdateSpecializationCommand command = new(specializationsRepository);
        var request = new AddOrUpdateSpecializationRequest(null, Name);

        // Act
        var response = await command.ExecuteAsync(request);
        var added = specializationsRepository.Db.Last();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(200));
            Assert.That(specializationsRepository.Db, Has.Count.EqualTo(1));
            Assert.That(added.Name, Is.EqualTo(Name));
        });
    }

    [TestCase(specializationId, "Updated Specialty")]
    public async Task ExecuteAsync_WhenUpdatingExistingSpecialization_Returns200_AndUpdatesSpecialization(Guid Id, string Name)
    {
        // Arrange
        Specialization existing = new()
        {
            Id = new(Id),
            Name = "Old Specialty",
            Doctors = Array.Empty<Doctor>()
        };

        FakeRepository<Specialization> specializationsRepository = new(new[] { existing });

        AddOrUpdateSpecializationCommand command = new(specializationsRepository);
        var request = new AddOrUpdateSpecializationRequest(Id, Name);

        // Act
        var response = await command.ExecuteAsync(request);
        var updated = specializationsRepository.Db.First();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(200));
            Assert.That(updated.Name, Is.EqualTo(Name));
        });
    }

    [TestCase(specializationId, "Invalid Specialty")]
    public async Task ExecuteAsync_WhenValidationFails_Returns400_AndDoesNotUpdateSpecialization(Guid Id, string Name)
    {
        // Arrange
        Specialization existing = new()
        {
            Id = new(Id),
            Name = "Valid Specialty",
            Doctors = Array.Empty<Doctor>()
        };

        FakeRepository<Specialization> specializationsRepository = new(new[] { existing });

        var validator = new AlwaysInvalidSpecializationValidator();

        AddOrUpdateSpecializationCommand command = new(specializationsRepository, validator);
        var request = new AddOrUpdateSpecializationRequest(Id, Name);

        // Act
        var response = await command.ExecuteAsync(request);
        var updated = specializationsRepository.Db.First();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(400));
            Assert.That(updated.Name, Is.Not.EqualTo(Name));
        });
    }

    // Валидатор, который всегда возвращает false
    private class AlwaysInvalidSpecializationValidator : IValidator<Specialization>
    {
        public Task<bool> ValidateAsync(Specialization entity, CancellationToken cancellationToken = default)
            => Task.FromResult(false);
    }
}
