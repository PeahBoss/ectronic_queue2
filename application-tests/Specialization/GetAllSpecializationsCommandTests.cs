
namespace ElectronicQueue.Tests.Application.Specializations;

public class GetAllSpecializationsCommandTests  
{
    [Test]
    public async Task ExecuteAsync_WhenSpecializationsExist_ReturnsAllSpecializations()
    {
        // Arrange: генерация 10 специализаций
        var specGenerator = new Faker<Specialization>("ru")
            .RuleFor(s => s.Id, f => new Id(f.Random.Guid()))
            .RuleFor(s => s.Name, f => f.Name.JobTitle())
            .RuleFor(s => s.Doctors, f => []);

        var fakeSpecializations = specGenerator.Generate(10);
        var repo = new FakeRepository<Specialization>(fakeSpecializations.ToArray());
        var converter = new SpecializationConverter();
        var command = new GetAllSpecializationsCommand(repo, converter);

        // Act
        var response = await command.ExecuteAsync(new EmptyRequest());
        var lastSpec = response.Specializations.LastOrDefault();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(200));
            Assert.That(response.Specializations.Count(), Is.EqualTo(10));
            Assert.That(lastSpec, Is.Not.Null);
            Assert.That(lastSpec!.Name, Is.EqualTo(fakeSpecializations[^1].Name));
            Assert.That(lastSpec!.Id, Is.EqualTo(fakeSpecializations[^1].Id.Value));
        });
    }
}
