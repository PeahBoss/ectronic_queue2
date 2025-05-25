namespace ElectronicQueue.Core.Responses.DTOs
{
    public record DoctorsArrayResponse(
        int StatusCode,
        string Message,
        IEnumerable<DoctorResponse> Doctors
    ) : BaseResponse(StatusCode, Message);
}