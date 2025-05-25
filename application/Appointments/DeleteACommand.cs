using Microsoft.Extensions.Logging;
using ElectronicQueue.Core.Entities;
using ElectronicQueue.Application;

namespace ElectronicQueue.Application.Appointments;

public class DeleteAppointmentCommand(
    IRepository<Appointment> appointmentsRepository,
    IValidator<Appointment>? appointmentValidator = null,
    ILogger<DeleteAppointmentCommand>? logger = null
) : ICommand<ByIdRequest, BaseResponse>
{
    public async Task<BaseResponse> ExecuteAsync(ByIdRequest request,
                                                 CancellationToken cancellationToken = default)
    {
        try
        {
            logger?.LogInformation("RQST: Delete appointment ID {AppointmentId}", request.Id);

            var appointment = await appointmentsRepository.GetOneAsync(a => a.Id.Value == request.Id, cancellationToken);
            if (appointment is null)
            {
                logger?.LogWarning("RQST: Appointment ID {AppointmentId} not found", request.Id);
                return new BaseResponse(404, "Appointment not found");
            }

            if (appointmentValidator is not null)
            {
                var valid = await appointmentValidator.ValidateAsync(appointment, cancellationToken);
                if (!valid)
                {
                    logger?.LogWarning("RQST: Validation failed for appointment ID {AppointmentId}", request.Id);
                    return new BaseResponse(409, "Appointment cannot be deleted due to dependencies");
                }
            }

            await appointmentsRepository.RemoveAsync(appointment, cancellationToken);
            logger?.LogInformation("RQST: Appointment ID {AppointmentId} deleted", request.Id);

            return new BaseResponse(200, "OK");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "ERR: Delete appointment ID {AppointmentId} failed: {Message}",
                request.Id, ex.Message);
            return new BaseResponse(500, "Internal server error");
        }
    }
}

