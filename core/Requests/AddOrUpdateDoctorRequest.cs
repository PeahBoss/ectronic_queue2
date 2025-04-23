using ElectronicQueue.Core.Interfaces;

public record AddOrUpdateDoctorRequest(
    Guid? Id,
    string Name,
    string PhoneNumber,
    string OfficeNumber,
    string WorkSchedule,
    List<Guid> SpecializationIds
) : IRequest;
