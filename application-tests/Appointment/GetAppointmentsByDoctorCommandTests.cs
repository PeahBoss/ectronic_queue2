
namespace ElectronicQueue.Tests.Application.Appointments;

public class GetAppointmentsByDoctorCommandTests
{
    [Test]
    public async Task ExecuteAsync_WhenDoctorExistsWithAppointments_ReturnsAppointments()
    {
        // Arrange: создаём доктора с приёмами
        var patientGenerator = new Faker<Patient>("ru")
            .RuleFor(p => p.Id, f => new Id(f.Random.Guid()))
            .RuleFor(p => p.Name, f => f.Name.FullName());

        var patients = patientGenerator.Generate(5);

        var appointmentGenerator = new Faker<Appointment>("ru")
            .RuleFor(a => a.Id, f => new Id(f.Random.Guid()))
            .RuleFor(a => a.AppointmentDate, f => f.Date.Future())
            .RuleFor(a => a.ClinicalRecords, f => f.Lorem.Sentence())
            .RuleFor(a => a.Patient, f => f.PickRandom(patients));

        var appointments = appointmentGenerator.Generate(5);

        var doctorId = Guid.NewGuid();
        var doctor = new Doctor
        {
            Id = new Id(doctorId),
            Name = "Тестовый Доктор",
            PhoneNumber = "123-456-7890",
            OfficeNumber = "101",
            WorkSchedule = "Пн-Пт",
            Appointments = appointments,
            Specializations = new List<Specialization>()
        };

        var repo = new FakeRepository<Doctor>(new[] { doctor });
        var converter = new AppointmentConverter();
        var command = new GetAppointmentsByDoctorCommand(repo, converter);

        var request = new ByIdRequest(doctorId); 


        // Act
        var response = await command.ExecuteAsync(request);
        var lastAppointment = response.Appointments.LastOrDefault();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(200));
            Assert.That(response.Appointments.Count(), Is.EqualTo(5));
            Assert.That(lastAppointment, Is.Not.Null);
            Assert.That(lastAppointment!.PatientName, Is.EqualTo(appointments[^1].Patient.Name));
        });
    }

    [Test]
    public async Task ExecuteAsync_WhenDoctorDoesNotExist_Returns404()
    {
        // Arrange: пустой репозиторий
        var repo = new FakeRepository<Doctor>(); // без докторов
        var converter = new AppointmentConverter();
        var command = new GetAppointmentsByDoctorCommand(repo, converter);

        var request = new ByIdRequest(Guid.NewGuid());


        // Act
        var response = await command.ExecuteAsync(request);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(404));
            Assert.That(response.Appointments, Is.Empty);
        });
    }
}
