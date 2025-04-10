namespace ElectronicQueue.Core.Responses;

public record BaseResponse(
    int Code,
    string Description
) : IResponse;