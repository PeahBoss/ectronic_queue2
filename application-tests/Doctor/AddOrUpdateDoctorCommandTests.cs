using ElectronicQueue.Application.Doctors;


namespace ElectronicQueue.Tests.Application.Doctors;

public class AddOrUpdateDoctorCommandTests
{
    private const string specializationId1 = "11111111-1111-1111-1111-111111111111";
    private const string specializationId2 = "22222222-2222-2222-2222-222222222222";
    private const string doctorId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";

    [TestCase(null, "Dr. Smith", "12345", "101", "Mon-Fri")]
    [TestCase(null, "Dr. Adams", "98765", "202", "Sat-Sun")]
    public async Task ExecuteAsync_WhenAddingDoctorThatAlreadyExists_Returns409(Guid? Id, string Name, string PhoneNumber, string Office, string Schedule)
    {
        // Arrange
        FakeRepository<Doctor> doctorsRepository = new([new()
        {
            Id = new(Id ?? Guid.NewGuid()),
            Name = Name,
            PhoneNumber = PhoneNumber
        }]);
        FakeRepository<Specialization> specializationsRepository = new();
        AddOrUpdateDoctorCommand command = new(doctorsRepository, specializationsRepository);
        AddOrUpdateDoctorRequest request = new(Id, Name, PhoneNumber, Office, Schedule, []);

        // Act
        var response = await command.ExecuteAsync(request);

        // Assert
        Assert.That(response.Code, Is.EqualTo(409));
    }

    [TestCase(null, "Dr. Who", "00000", "A1", "MWF", new[] { specializationId1 })]
    [TestCase(doctorId, "Dr. Who", "00000", "A1", "MWF", new[] { specializationId1 })]
    public async Task ExecuteAsync_WhenSpecializationIsMissing_Returns404(Guid? Id, string Name, string Phone, string Office, string Schedule, string[] specializationIds)
    {
        // Arrange
        FakeRepository<Specialization> specializationRepo = new(); // empty
        FakeRepository<Doctor> doctorRepo = new(Id switch
        {
            null => [],
            _ => [new()
            {
                Id = new(Id.Value),
                Name = Name,
                PhoneNumber = Phone,
                OfficeNumber = Office,
                WorkSchedule = Schedule
            }]
        });

        AddOrUpdateDoctorCommand command = new(doctorRepo, specializationRepo);
        AddOrUpdateDoctorRequest request = new(Id, Name, Phone, Office, Schedule, specializationIds.Select(Guid.Parse).ToArray());

        // Act
        var response = await command.ExecuteAsync(request);

        // Assert
        Assert.That(response.Code, Is.EqualTo(404));
    }

    [TestCase("Dr. House", "99999", "102", "24/7", new[] { specializationId1, specializationId2 })]
    [TestCase("Dr. Chase", "88888", "103", "MWF", new[] { specializationId1 })]
    public async Task ExecuteAsync_WhenDoctorIsAdded_Returns200(string Name, string Phone, string Office, string Schedule, string[] specializationIds)
    {
        // Arrange
        var specs = specializationIds.Select(id => new Specialization
        {
            Id = new(Guid.Parse(id)),
            Name = new Faker().Name.JobTitle()
        }).ToList();

        FakeRepository<Specialization> specializationRepo = new([.. specs]); 
        var doctorFaker = new Faker<Doctor>()
            .RuleFor(d => d.Id, f => new(f.Random.Guid()))
            .RuleFor(d => d.Name, f => f.Name.FullName())
            .RuleFor(d => d.PhoneNumber, f => f.Phone.PhoneNumber());

        FakeRepository<Doctor> doctorRepo = new([.. doctorFaker.Generate(10)]);

        AddOrUpdateDoctorCommand command = new(doctorRepo, specializationRepo);
        AddOrUpdateDoctorRequest request = new(null, Name, Phone, Office, Schedule, specializationIds.Select(Guid.Parse).ToArray());

        // Act
        var response = await command.ExecuteAsync(request);
        var added = doctorRepo.Db.Last();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(200));
            Assert.That(doctorRepo.Db, Has.Count.EqualTo(11));
            Assert.That(added.Name, Is.EqualTo(Name));
            Assert.That(added.PhoneNumber, Is.EqualTo(Phone));
            Assert.That(added.OfficeNumber, Is.EqualTo(Office));
            Assert.That(added.WorkSchedule, Is.EqualTo(Schedule));
            Assert.That(added.Specializations.Select(s => s.Id.Value), Is.EquivalentTo(specializationIds.Select(Guid.Parse)));
        });
    }

    [TestCase(doctorId, "Dr. Updated", "55555", "301", "Weekends", new[] { specializationId1 })]
    public async Task ExecuteAsync_WhenDoctorIsUpdated_Returns200(Guid Id, string Name, string Phone, string Office, string Schedule, string[] specializationIds)
    {
        // Arrange
        var oldSpec = new Specialization { Id = new(Guid.NewGuid()), Name = "Old" };
        var newSpecs = specializationIds.Select(id => new Specialization
        {
            Id = new(Guid.Parse(id)),
            Name = new Faker().Name.JobTitle()
        }).ToList();

        Doctor existing = new()
        {
            Id = new(Id),
            Name = "Old Name",
            PhoneNumber = "0000",
            OfficeNumber = "100",
            WorkSchedule = "Old",
            Specializations = [oldSpec]
        };

        FakeRepository<Doctor> doctorRepo = new([existing]);
        FakeRepository<Specialization> specRepo = new(newSpecs.ToArray());

        AddOrUpdateDoctorCommand command = new(doctorRepo, specRepo);
        AddOrUpdateDoctorRequest request = new(Id, Name, Phone, Office, Schedule, specializationIds.Select(Guid.Parse).ToArray());

        // Act
        var response = await command.ExecuteAsync(request);
        var updated = doctorRepo.Db.First();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.Code, Is.EqualTo(200));
            Assert.That(updated.Name, Is.EqualTo(Name));
            Assert.That(updated.PhoneNumber, Is.EqualTo(Phone));
            Assert.That(updated.OfficeNumber, Is.EqualTo(Office));
            Assert.That(updated.WorkSchedule, Is.EqualTo(Schedule));
            Assert.That(updated.Specializations.Select(s => s.Id.Value), Is.EquivalentTo(specializationIds.Select(Guid.Parse)));
        });
    }

    [TestCase(doctorId, "Invalid", "0000", "A0", "None", new[] { specializationId1 })]
    public async Task ExecuteAsync_WhenUpdatingInvalidDoctor_Returns400(Guid Id, string Name, string Phone, string Office, string Schedule, string[] specializationIds)
    {
        // Arrange
        var user = new Faker().Person.FullName;
        var specs = specializationIds.Select(id => new Specialization { Id = new(Guid.Parse(id)), Name = "Spec" }).ToList();

        var doctor = new Doctor
        {
            Id = new(Id),
            Name = "Valid",
            PhoneNumber = "11111",
            OfficeNumber = "1",
            WorkSchedule = "MWF",
            Specializations = specs
        };

        var doctorRepo = new FakeRepository<Doctor>([doctor]);
        var specRepo = new FakeRepository<Specialization>(specs.ToArray());

        var command = new AddOrUpdateDoctorCommand(doctorRepo, specRepo, new Invalidator<Doctor>());
        var request = new AddOrUpdateDoctorRequest(Id, Name, Phone, Office, Schedule, specializationIds.Select(Guid.Parse).ToArray());

        // Act
        var result = await command.ExecuteAsync(request);
        var updated = doctorRepo.Db.First();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Code, Is.EqualTo(400));
            Assert.That(updated.Name, Is.Not.EqualTo(Name));
        });
    }

    [TestCase("Dr. Strange", "77777", "999", "All week", new[] { specializationId1 })]
    public async Task ExecuteAsync_WhenAddingInvalidDoctor_Returns400(string Name, string Phone, string Office, string Schedule, string[] specializationIds)
    {
        // Arrange
        var specs = specializationIds.Select(id => new Specialization { Id = new(Guid.Parse(id)), Name = "Spec" }).ToList();
        var doctorRepo = new FakeRepository<Doctor>();
        var specRepo = new FakeRepository<Specialization>(specs.ToArray());

        var command = new AddOrUpdateDoctorCommand(doctorRepo, specRepo, new Invalidator<Doctor>());
        var request = new AddOrUpdateDoctorRequest(null, Name, Phone, Office, Schedule, specializationIds.Select(Guid.Parse).ToArray());

        // Act
        var result = await command.ExecuteAsync(request);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Code, Is.EqualTo(400));
            Assert.That(doctorRepo.Db, Has.Count.EqualTo(0));
        });
    }
}
