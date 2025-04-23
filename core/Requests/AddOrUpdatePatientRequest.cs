using ElectronicQueue.Core.Enums; // если Gender находится там

public record AddOrUpdatePatientRequest(
    Guid? Id,
    string Name,
    DateTime Birthday,
    Gender Gender,
    string PhoneNumber,
    string InsuranceNumber
) : IRequest;
