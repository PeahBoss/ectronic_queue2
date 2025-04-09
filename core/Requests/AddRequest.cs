namespace ectronic_queue.Core.Requests;

public record AddRequest(
    string Name,
    DateTime DateB,
    string PhoneNumber,
   string InsuranceNumber
) : IRequest;