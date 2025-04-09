namespace ectronic_queue.Core.Responses;

public record BaseResponse(
    int Code,
    string Description
) : IResponse;