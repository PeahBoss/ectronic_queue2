using ElectronicQueue.Core.Enums;

namespace ElectronicQueue.Core.Requests;

public record AddOrUpdatePatientRequest(
    Guid? Id,
    string Name,
    DateTime Birthday,
    Gender Gender,
    string PhoneNumber,
    string InsuranceNumber
) : IRequest;