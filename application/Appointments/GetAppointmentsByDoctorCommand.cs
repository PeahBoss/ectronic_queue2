
namespace ElectronicQueue.Application.Appointments;

public class GetAppointmentsByDoctorCommand(
    IRepository<Doctor> doctorRepository,
    IConverter<Appointment, AppointmentResponse> converter,
    ILogger<GetAppointmentsByDoctorCommand>? logger = null
) : ICommand<ByIdRequest, AppointmentsArrayResponse>
{
    public async Task<AppointmentsArrayResponse> ExecuteAsync(
        ByIdRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger?.LogInformation("RQST: Getting appointments for doctor {DoctorId}", request.Id);

            var doctor = await doctorRepository.GetOneAsync(
                d => d.Id.Value == request.Id,
                cancellationToken);

            if (doctor is null)
                return new AppointmentsArrayResponse(404, "Doctor not found", []);

            var appointments = doctor.Appointments.Select(converter.ToResponse);

            return new AppointmentsArrayResponse(200, "OK", appointments);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "ERR: Failed to get doctor's appointments");
            return new AppointmentsArrayResponse(500, "Internal server error", []);
        }
    }
}
