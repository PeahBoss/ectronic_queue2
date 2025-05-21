using ElectronicQueue.Application;
using ElectronicQueue.Application.Appointments;
using ElectronicQueue.Application.Doctors;
using ElectronicQueue.Application.Patients;

namespace ElectronicQueue.Tests.Application.Appointments;

public class AddOrUpdateAppointmentCommandTests
{
    private static readonly Guid appointmentId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid doctorId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid patientId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    [Test]
    public async Task ExecuteAsync_WhenAddingAppointment_Returns200_AndAddsAppointment()
    {
        var doctor = new Doctor { Id = new(doctorId), Name = "Dr. Test" };
        var patient = new Patient { Id = new(patientId), Name = "Patient Test" };

        var doctorsRepo = new FakeRepository<Doctor>(new[] { doctor });
        var patientsRepo = new FakeRepository<Patient>(new[] { patient });
        var appointmentsRepo = new FakeRepository<Appointment>();

        var command = new AddOrUpdateAppointmentCommand(appointmentsRepo, doctorsRepo, patientsRepo);

        var request = new AddOrUpdateAppointmentRequest(
            null,
            DateTime.UtcNow.AddDays(1),
            "Some clinical notes",
            doctorId,
            patientId
        );

        var response = await command.ExecuteAsync(request);
        var added = appointmentsRepo.Db.Last();

        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(200));
            Assert.That(appointmentsRepo.Db.Count, Is.EqualTo(1));
            Assert.That(added.Doctor.Id.Value, Is.EqualTo(doctorId));
            Assert.That(added.Patient.Id.Value, Is.EqualTo(patientId));
            Assert.That(added.ClinicalRecords, Is.EqualTo("Some clinical notes"));
        });
    }

    [Test]
    public async Task ExecuteAsync_WhenUpdatingAppointment_Returns200_AndUpdatesAppointment()
    {
        var doctorOld = new Doctor { Id = new(doctorId), Name = "Dr. Old" };
        var doctorNew = new Doctor { Id = new(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd")), Name = "Dr. New" };
        var patientOld = new Patient { Id = new(patientId), Name = "Patient Old" };
        var patientNew = new Patient { Id = new(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee")), Name = "Patient New" };

        var existingAppointment = new Appointment
        {
            Id = new(appointmentId),
            Doctor = doctorOld,
            Patient = patientOld,
            AppointmentDate = DateTime.UtcNow,
            ClinicalRecords = "Old notes"
        };

        var doctorsRepo = new FakeRepository<Doctor>(new[] { doctorOld, doctorNew });
        var patientsRepo = new FakeRepository<Patient>(new[] { patientOld, patientNew });
        var appointmentsRepo = new FakeRepository<Appointment>(new[] { existingAppointment });

        var command = new AddOrUpdateAppointmentCommand(appointmentsRepo, doctorsRepo, patientsRepo);

        var request = new AddOrUpdateAppointmentRequest(
            appointmentId,
            DateTime.UtcNow.AddDays(5),
            "Updated clinical notes",
            doctorNew.Id.Value,
            patientNew.Id.Value
        );

        var response = await command.ExecuteAsync(request);
        var updated = appointmentsRepo.Db.First();

        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(200));
            Assert.That(updated.Doctor.Id.Value, Is.EqualTo(doctorNew.Id.Value));
            Assert.That(updated.Patient.Id.Value, Is.EqualTo(patientNew.Id.Value));
            Assert.That(updated.ClinicalRecords, Is.EqualTo("Updated clinical notes"));
        });
    }

    [Test]
    public async Task ExecuteAsync_WhenAppointmentNotFound_Returns404()
    {
        var doctorsRepo = new FakeRepository<Doctor>(new[] { new Doctor { Id = new(doctorId) } });
        var patientsRepo = new FakeRepository<Patient>(new[] { new Patient { Id = new(patientId) } });
        var appointmentsRepo = new FakeRepository<Appointment>();

        var command = new AddOrUpdateAppointmentCommand(appointmentsRepo, doctorsRepo, patientsRepo);

        var request = new AddOrUpdateAppointmentRequest(
            appointmentId,
            DateTime.UtcNow,
            "Notes",
            doctorId,
            patientId
        );

        var response = await command.ExecuteAsync(request);

        Assert.That(response.Code, Is.EqualTo(404));
    }

    [Test]
    public async Task ExecuteAsync_WhenDoctorNotFound_Returns404()
    {
        var patientsRepo = new FakeRepository<Patient>(new[] { new Patient { Id = new(patientId) } });
        var appointmentsRepo = new FakeRepository<Appointment>();
        var doctorsRepo = new FakeRepository<Doctor>();

        var command = new AddOrUpdateAppointmentCommand(appointmentsRepo, doctorsRepo, patientsRepo);

        var request = new AddOrUpdateAppointmentRequest(
            null,
            DateTime.UtcNow,
            "Notes",
            doctorId,
            patientId
        );

        var response = await command.ExecuteAsync(request);

        Assert.That(response.Code, Is.EqualTo(404));
    }

    [Test]
    public async Task ExecuteAsync_WhenPatientNotFound_Returns404()
    {
        var doctorsRepo = new FakeRepository<Doctor>(new[] { new Doctor { Id = new(doctorId) } });
        var appointmentsRepo = new FakeRepository<Appointment>();
        var patientsRepo = new FakeRepository<Patient>();

        var command = new AddOrUpdateAppointmentCommand(appointmentsRepo, doctorsRepo, patientsRepo);

        var request = new AddOrUpdateAppointmentRequest(
            null,
            DateTime.UtcNow,
            "Notes",
            doctorId,
            patientId
        );

        var response = await command.ExecuteAsync(request);

        Assert.That(response.Code, Is.EqualTo(404));
    }

    [Test]
    public async Task ExecuteAsync_WhenValidationFails_Returns400_AndDoesNotSave()
    {
        var doctor = new Doctor { Id = new(doctorId) };
        var patient = new Patient { Id = new(patientId) };
        var appointmentsRepo = new FakeRepository<Appointment>();
        var doctorsRepo = new FakeRepository<Doctor>(new[] { doctor });
        var patientsRepo = new FakeRepository<Patient>(new[] { patient });

        var invalidValidator = new AlwaysInvalidAppointmentValidator();

        var command = new AddOrUpdateAppointmentCommand(appointmentsRepo, doctorsRepo, patientsRepo, invalidValidator);

        var request = new AddOrUpdateAppointmentRequest(
            null,
            DateTime.UtcNow,
            "Invalid data",
            doctorId,
            patientId
        );

        var response = await command.ExecuteAsync(request);

        Assert.That(response.Code, Is.EqualTo(400));
        Assert.That(appointmentsRepo.Db.Count, Is.EqualTo(0));
    }

    private class AlwaysInvalidAppointmentValidator : IValidator<Appointment>
    {
        public Task<bool> ValidateAsync(Appointment entity, CancellationToken cancellationToken = default)
            => Task.FromResult(false);
    }
}
