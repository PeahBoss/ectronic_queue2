namespace ElectronicQueue.Core.Responses.DTOs;

public record AppointmentsArrayResponse(
    int StatusCode,
    string Message,
    IEnumerable<AppointmentResponse> Appointments
) : BaseResponse(StatusCode, Message);
