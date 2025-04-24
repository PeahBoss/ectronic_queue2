namespace ElectronicQueue.Core.Requests;

public record AddOrUpdateSpecializationRequest(
    Guid? Id,
    string Name
) : IRequest;