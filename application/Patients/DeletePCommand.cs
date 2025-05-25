using Microsoft.Extensions.Logging;
using ElectronicQueue.Core.Entities;
using ElectronicQueue.Application;

namespace ElectronicQueue.Application.Patients;

public class DeletePatientCommand(
    IRepository<Patient> patientsRepository,
    IValidator<Patient>? patientValidator = null,
    ILogger<DeletePatientCommand>? logger = null
) : ICommand<ByIdRequest, BaseResponse>
{
    public async Task<BaseResponse> ExecuteAsync(ByIdRequest request,
                                                 CancellationToken cancellationToken = default)
    {
        try
        {
            logger?.LogInformation("RQST: Delete patient ID {PatientId}", request.Id);

            var patient = await patientsRepository.GetOneAsync(p => p.Id.Value == request.Id, cancellationToken);
            if (patient is null)
            {
                logger?.LogWarning("RQST: Patient ID {PatientId} not found", request.Id);
                return new BaseResponse(404, "Patient not found");
            }

            if (patientValidator is not null)
            {
                var valid = await patientValidator.ValidateAsync(patient, cancellationToken);
                if (!valid)
                {
                    logger?.LogWarning("RQST: Validation failed for patient ID {PatientId}", request.Id);
                    return new BaseResponse(409, "Patient cannot be deleted due to dependencies");
                }
            }

            await patientsRepository.RemoveAsync(patient, cancellationToken);
            logger?.LogInformation("RQST: Patient ID {PatientId} deleted", request.Id);

            return new BaseResponse(200, "OK");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "ERR: Delete patient ID {PatientId} failed: {Message}",
                request.Id, ex.Message);
            return new BaseResponse(500, "Internal server error");
        }
    }
}
