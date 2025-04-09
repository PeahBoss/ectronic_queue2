namespace ectronic_queue.Core.Requests;

public record AddDoctorRequest(
    string Name,
    string Specialization,
    string PhoneNumber,
    string OfficeNumber,
    string WorkSchedule
) : IRequest;