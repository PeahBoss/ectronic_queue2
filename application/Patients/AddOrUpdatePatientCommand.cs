using Microsoft.Extensions.Logging;

namespace ElectronicQueue.Application.Patients;

public class AddOrUpdatePatientCommand(
    IRepository<Patient> patientsRepository,
    IValidator<Patient>? patientValidator = null,
    ILogger<AddOrUpdatePatientCommand>? logger = null
) : ICommand<AddOrUpdatePatientRequest, BaseResponse>
{
    public async Task<BaseResponse> ExecuteAsync(AddOrUpdatePatientRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            bool isAdding = request.Id is null;
            logger?.LogInformation("RQST: {Operation} patient {PatientName} ({Phone})",
                                   isAdding ? "creation" : "update",
                                   request.Name,
                                   request.PhoneNumber);

            var allPatients = await patientsRepository.GetAllAsync(cancellationToken);
            Patient patient;

            if (isAdding)
            {
                var potentialPatient = allPatients.FirstOrDefault(p =>
                    p.Name == request.Name && p.PhoneNumber == request.PhoneNumber);

                if (potentialPatient is not null)
                {
                    logger?.LogWarning("RQST: failed: Patient with phone {Phone} already exists", request.PhoneNumber);
                    return new(409, $"Patient with phone number {request.PhoneNumber} already exists.");
                }

                patient = new Patient { Appointments = [] };
            }
            else
            {
                patient = allPatients.FirstOrDefault(p => p.Id.Value == request.Id!.Value)!;

                if (patient is null)
                {
                    logger?.LogWarning("RQST: failed: Patient ID {PatientId} not found", request.Id);
                    return new(404, "Patient not found.");
                }
            }

            var oldData = (patient.Name, patient.Birthday, patient.Gender, patient.PhoneNumber, patient.InsuranceNumber);

            patient.Name = request.Name;
            patient.Birthday = request.Birthday;
            patient.Gender = request.Gender;
            patient.PhoneNumber = request.PhoneNumber;
            patient.InsuranceNumber = request.InsuranceNumber;

            if (patientValidator is not null && !await patientValidator.ValidateAsync(patient, cancellationToken))
            {
                logger?.LogWarning("RQST: failed: validation failed for patient {PatientName} ({Phone})", request.Name, request.PhoneNumber);

                patient.Name = oldData.Name;
                patient.Birthday = oldData.Birthday;
                patient.Gender = oldData.Gender;
                patient.PhoneNumber = oldData.PhoneNumber;
                patient.InsuranceNumber = oldData.InsuranceNumber;

                return new(400, "Patient validation failed");
            }

            await patientsRepository.AddOrUpdateAsync(patient, cancellationToken);
            await patientsRepository.SaveChangesAsync(cancellationToken);

            return new(200, "OK");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "RQST: failed: unexpected error during {Operation} for patient {PatientName}",
                             request.Id is null ? "creation" : "update", request.Name);
            return new(500, "An error occurred while processing your request");
        }
    }
}
