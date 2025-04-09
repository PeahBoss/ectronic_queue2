namespace ectronic_queue.Core.Responces;

public record BaseResponse(
    int Code,
    string Description
) : IResponse;