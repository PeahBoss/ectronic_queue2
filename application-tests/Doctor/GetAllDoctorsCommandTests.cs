using Bogus;
using ElectronicQueue.Application.Doctors;
using ElectronicQueue.Core.Entities;
using ElectronicQueue.Core.Responses.DTOs;
using ElectronicQueue.Tests;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace ElectronicQueue.Tests.Application.Doctors;

public class GetAllDoctorsCommandTests
{
    [Test]
    public async Task ExecuteAsync_WhenDoctorsExist_ReturnsAllDoctors()
    {
        // Arrange: генерация 10 докторов
        var doctorGenerator = new Faker<Doctor>("ru")
     .RuleFor(d => d.Id, f => new Id(f.Random.Guid()))
     .RuleFor(d => d.Name, f => f.Name.FullName())
     .RuleFor(d => d.PhoneNumber, f => f.Phone.PhoneNumber())
     .RuleFor(d => d.OfficeNumber, f => f.Random.Number(100, 500).ToString())
     .RuleFor(d => d.WorkSchedule, f => "Пн-Пт 09:00–18:00")
     .RuleFor(d => d.Specializations, f => new List<Specialization>
     {
        new Specialization { Name = f.Random.Word() }
     });

        var fakeDoctors = doctorGenerator.Generate(10);
        var repo = new FakeRepository<Doctor>(fakeDoctors.ToArray());

        var converter = new DoctorConverter();
        var command = new GetAllDoctorsCommand(repo, converter);

        // Act
        var response = await command.ExecuteAsync(new EmptyRequest());
        var lastDoctor = response.Doctors.LastOrDefault();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(200));
            Assert.That(response.Doctors.Count(), Is.EqualTo(10));
            Assert.That(lastDoctor, Is.Not.Null);
            Assert.That(lastDoctor!.Name, Is.EqualTo(fakeDoctors[^1].Name));
            Assert.That(lastDoctor!.PhoneNumber, Is.EqualTo(fakeDoctors[^1].PhoneNumber));
            Assert.That(lastDoctor!.OfficeNumber, Is.EqualTo(fakeDoctors[^1].OfficeNumber));
        });
    }
}
