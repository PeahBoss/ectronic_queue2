using ElectronicQueue.Core.Interfaces;

namespace ElectronicQueue.Core.Requests;

public record AddOrUpdateDoctorRequest(
    Guid? Id,
    string Name,
    string PhoneNumber,
    string OfficeNumber,
    string WorkSchedule,
    Guid[] SpecializationIds
) : IRequest;