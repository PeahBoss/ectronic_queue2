
using Microsoft.Extensions.Logging;

namespace ElectronicQueue.Application.Doctors;

public class GetAllDoctorsCommand(
    IRepository<Doctor> doctorsRepository,
    IConverter<Doctor,DoctorResponse> converter,
    ILogger<GetAllDoctorsCommand>? logger = null
) : ICommand<EmptyRequest, DoctorsArrayResponse>
{
    public async Task<DoctorsArrayResponse> ExecuteAsync(EmptyRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger?.LogInformation("RQST: Getting all doctors");

            var doctors = await doctorsRepository.GetAllAsync(cancellationToken);
            var convertedDoctors = doctors.Select(converter.ToResponse);

            return new DoctorsArrayResponse(200, "OK", convertedDoctors);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "ERR: Failed to get doctors. {ErrorMessage}", ex.Message);
            return new DoctorsArrayResponse(500, "Internal server error", []);
        }
    }
}
