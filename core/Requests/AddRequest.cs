namespace ElectronicQueue.Core.Requests;

public record AddPatientRequest(
    string Name,
    DateTime Birthday,
    string PhoneNumber,
   string InsuranceNumber
) : IRequest;