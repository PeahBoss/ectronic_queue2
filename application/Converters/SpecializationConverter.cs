

namespace ElectronicQueue.Application.Specializations;

public class SpecializationConverter : IConverter<Specialization, SpecializationResponse>
{
    public SpecializationResponse ToResponse(Specialization specialization)
    {
        return new SpecializationResponse
        {
            Id = specialization.Id.Value,
            Name = specialization.Name
        };
    }
}
