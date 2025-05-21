using ElectronicQueue.Application;
using ElectronicQueue.Application.Patients;

namespace ElectronicQueue.Tests.Application.Patients;

public class AddOrUpdatePatientCommandTests
{
    private const string patientId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";

    [TestCase(null, "John Doe", "1990-01-01", Gender.Male, "12345", "INS123")]
    [TestCase(null, "Jane Doe", "1985-05-05", Gender.Female, "54321", "INS456")]
    public async Task ExecuteAsync_WhenAddingPatientThatAlreadyExists_Returns409(
        Guid? Id, string Name, string Birthday, Gender Gender, string PhoneNumber, string InsuranceNumber)
    {
        // Arrange
        FakeRepository<Patient> patientsRepository = new(new[]
        {
            new Patient
            {
                Id = new(Id ?? Guid.NewGuid()),
                Name = Name,
                Birthday = DateTime.Parse(Birthday),
                Gender = Gender,
                PhoneNumber = PhoneNumber,
                InsuranceNumber = InsuranceNumber,
                Appointments = Array.Empty<Appointment>()
            }
        });

        AddOrUpdatePatientCommand command = new(patientsRepository);
        var request = new AddOrUpdatePatientRequest(
            Id,
            Name,
            DateTime.Parse(Birthday),
            Gender,
            PhoneNumber,
            InsuranceNumber);

        // Act
        var response = await command.ExecuteAsync(request);

        // Assert
        Assert.That(response.Code, Is.EqualTo(409));
    }

    [TestCase(patientId, "New Name", "2000-12-12", Gender.Female, "99999", "INS999")]
    public async Task ExecuteAsync_WhenUpdatingNonExistentPatient_Returns404(
        Guid Id, string Name, string Birthday, Gender Gender, string PhoneNumber, string InsuranceNumber)
    {
        // Arrange
        FakeRepository<Patient> patientsRepository = new(Array.Empty<Patient>()); // пустой репозиторий — пациент не найден

        AddOrUpdatePatientCommand command = new(patientsRepository);
        var request = new AddOrUpdatePatientRequest(
            Id,
            Name,
            DateTime.Parse(Birthday),
            Gender,
            PhoneNumber,
            InsuranceNumber);

        // Act
        var response = await command.ExecuteAsync(request);

        // Assert
        Assert.That(response.Code, Is.EqualTo(404));
    }

    [TestCase("Alice Smith", "1995-07-07", Gender.Female, "77777", "INS777")]
    public async Task ExecuteAsync_WhenAddingValidPatient_Returns200_AndAddsPatient(
        string Name, string Birthday, Gender Gender, string PhoneNumber, string InsuranceNumber)
    {
        // Arrange
        FakeRepository<Patient> patientsRepository = new(); // пустой репозиторий

        AddOrUpdatePatientCommand command = new(patientsRepository);
        var request = new AddOrUpdatePatientRequest(
            null,
            Name,
            DateTime.Parse(Birthday),
            Gender,
            PhoneNumber,
            InsuranceNumber);

        // Act
        var response = await command.ExecuteAsync(request);
        var added = patientsRepository.Db.Last();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(200));
            Assert.That(patientsRepository.Db, Has.Count.EqualTo(1));
            Assert.That(added.Name, Is.EqualTo(Name));
            Assert.That(added.Birthday.ToString("yyyy-MM-dd"), Is.EqualTo(Birthday));
            Assert.That(added.Gender, Is.EqualTo(Gender));
            Assert.That(added.PhoneNumber, Is.EqualTo(PhoneNumber));
            Assert.That(added.InsuranceNumber, Is.EqualTo(InsuranceNumber));
        });
    }

    [TestCase(patientId, "Updated Name", "1980-03-03", Gender.Male, "55555", "INS555")]
    public async Task ExecuteAsync_WhenUpdatingExistingPatient_Returns200_AndUpdatesPatient(
        Guid Id, string Name, string Birthday, Gender Gender, string PhoneNumber, string InsuranceNumber)
    {
        // Arrange
        Patient existing = new()
        {
            Id = new(Id),
            Name = "Old Name",
            Birthday = DateTime.Parse("1970-01-01"),
            Gender = Gender.Female,
            PhoneNumber = "00000",
            InsuranceNumber = "OLDINS",
            Appointments = Array.Empty<Appointment>()
        };

        FakeRepository<Patient> patientsRepository = new(new[] { existing });

        AddOrUpdatePatientCommand command = new(patientsRepository);
        var request = new AddOrUpdatePatientRequest(
            Id,
            Name,
            DateTime.Parse(Birthday),
            Gender,
            PhoneNumber,
            InsuranceNumber);

        // Act
        var response = await command.ExecuteAsync(request);
        var updated = patientsRepository.Db.First();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(200));
            Assert.That(updated.Name, Is.EqualTo(Name));
            Assert.That(updated.Birthday.ToString("yyyy-MM-dd"), Is.EqualTo(Birthday));
            Assert.That(updated.Gender, Is.EqualTo(Gender));
            Assert.That(updated.PhoneNumber, Is.EqualTo(PhoneNumber));
            Assert.That(updated.InsuranceNumber, Is.EqualTo(InsuranceNumber));
        });
    }

    [TestCase(patientId, "Invalid Patient", "1999-09-09", Gender.Female, "00000", "INS000")]
    public async Task ExecuteAsync_WhenValidationFails_Returns400_AndDoesNotUpdatePatient(
        Guid Id, string Name, string Birthday, Gender Gender, string PhoneNumber, string InsuranceNumber)
    {
        // Arrange
        Patient existing = new()
        {
            Id = new(Id),
            Name = "Valid Name",
            Birthday = DateTime.Parse("1970-01-01"),
            Gender = Gender.Male,
            PhoneNumber = "11111",
            InsuranceNumber = "INS1",
            Appointments = Array.Empty<Appointment>()
        };

        FakeRepository<Patient> patientsRepository = new(new[] { existing });

        // Создаем команду с валидатором, который всегда возвращает false
        var validator = new AlwaysInvalidPatientValidator();

        AddOrUpdatePatientCommand command = new(patientsRepository, validator);
        var request = new AddOrUpdatePatientRequest(
            Id,
            Name,
            DateTime.Parse(Birthday),
            Gender,
            PhoneNumber,
            InsuranceNumber);

        // Act
        var response = await command.ExecuteAsync(request);
        var updated = patientsRepository.Db.First();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(400));
            Assert.That(updated.Name, Is.Not.EqualTo(Name)); // данные не должны обновиться
        });
    }

    // Простой валидатор, всегда возвращающий false (для теста ошибки валидации)
    private class AlwaysInvalidPatientValidator : IValidator<Patient>
    {
        public Task<bool> ValidateAsync(Patient entity, CancellationToken cancellationToken = default)
            => Task.FromResult(false);
    }
}
