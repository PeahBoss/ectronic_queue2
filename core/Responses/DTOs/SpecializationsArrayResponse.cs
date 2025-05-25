
namespace ElectronicQueue.Core.Responses.DTOs;

public record SpecializationsArrayResponse(
    int StatusCode,
    string Message,
    IEnumerable<SpecializationResponse> Specializations
) : BaseResponse(StatusCode, Message);
