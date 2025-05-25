using Microsoft.Extensions.Logging;

namespace ElectronicQueue.Application.Doctors;

public class DeleteDoctorCommand(
    IRepository<Doctor> doctorsRepository,
    IValidator<Doctor>? doctorValidator = null,
    ILogger<DeleteDoctorCommand>? logger = null
) : ICommand<ByIdRequest, BaseResponse>
{
    public async Task<BaseResponse> ExecuteAsync(ByIdRequest request,
                                                 CancellationToken cancellationToken = default)
    {
        try
        {
            logger?.LogInformation("RQST: Delete doctor ID {DoctorId}", request.Id);

            var doctor = await doctorsRepository.GetOneAsync(d => d.Id.Value == request.Id, cancellationToken);
            if (doctor is null)
            {
                logger?.LogWarning("RQST: Doctor ID {DoctorId} not found", request.Id);
                return new BaseResponse(404, "Doctor not found");
            }

            if (doctorValidator is not null)
            {
                var valid = await doctorValidator.ValidateAsync(doctor, cancellationToken);
                if (!valid)
                {
                    logger?.LogWarning("RQST: Validation failed for doctor ID {DoctorId}", request.Id);
                    return new BaseResponse(409, "Doctor cannot be deleted due to dependencies");
                }
            }

            await doctorsRepository.RemoveAsync(doctor, cancellationToken);
            logger?.LogInformation("RQST: Doctor ID {DoctorId} deleted", request.Id);

            return new BaseResponse(200, "OK");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "ERR: Delete doctor ID {DoctorId} failed: {Message}",
                request.Id, ex.Message);
            return new BaseResponse(500, "Internal server error");
        }
    }
}
