using Microsoft.Extensions.Logging;

namespace ElectronicQueue.Application.Doctors;

public class AddOrUpdateDoctorCommand(
    IRepository<Doctor> doctorsRepository,
    IRepository<Specialization> specializationsRepository,
    IValidator<Doctor>? doctorValidator = null,
    ILogger<AddOrUpdateDoctorCommand>? logger = null
) : ICommand<AddOrUpdateDoctorRequest, BaseResponse>
{
    public async Task<BaseResponse> ExecuteAsync(AddOrUpdateDoctorRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            bool isAdding = request.Id is null;
            logger?.LogInformation("RQST: {Operation} doctor {DoctorName} ({Phone})",
                                   isAdding ? "creation" : "update",
                                   request.Name,
                                   request.PhoneNumber);

            // Получаем всех врачей
            var allDoctors = await doctorsRepository.GetAllAsync(cancellationToken);
            Doctor doctor;

            if (isAdding)
            {
                var potentialDoctor = allDoctors.FirstOrDefault(d =>
                    d.Name == request.Name && d.PhoneNumber == request.PhoneNumber);

                if (potentialDoctor is not null)
                {
                    logger?.LogWarning("RQST: failed: Doctor with phone {Phone} already exists", request.PhoneNumber);
                    return new(409, $"Doctor with phone number {request.PhoneNumber} already exists.");
                }

                doctor = new Doctor();
            }
            else
            {
                doctor = allDoctors.FirstOrDefault(d => d.Id.Value == request.Id!.Value)!;

                if (doctor is null)
                {
                    logger?.LogWarning("RQST: failed: Doctor ID {DoctorId} not found", request.Id);
                    return new(404, "Doctor not found.");
                }
            }

            var oldData = (doctor.Name, doctor.PhoneNumber, doctor.OfficeNumber, doctor.WorkSchedule, doctor.Specializations);

            doctor.Name = request.Name;
            doctor.PhoneNumber = request.PhoneNumber;
            doctor.OfficeNumber = request.OfficeNumber;
            doctor.WorkSchedule = request.WorkSchedule;

            var allSpecializations = await specializationsRepository.GetAllAsync(cancellationToken);
            var matchedSpecializations = allSpecializations
                .Where(s => request.SpecializationIds.Contains(s.Id.Value))
                .ToList();

            if (matchedSpecializations.Count != request.SpecializationIds.Length)
            {
                logger?.LogWarning("RQST: failed: one or more specializations not found");
                return new(404, "One or more specializations not found.");
            }

            doctor.Specializations = matchedSpecializations;

            if (doctorValidator is not null && !await doctorValidator.ValidateAsync(doctor, cancellationToken))
            {
                logger?.LogWarning("RQST: failed: validation failed for doctor {DoctorName} ({Phone})", request.Name, request.PhoneNumber);
                doctor.Name = oldData.Name;
                doctor.PhoneNumber = oldData.PhoneNumber;
                doctor.OfficeNumber = oldData.OfficeNumber;
                doctor.WorkSchedule = oldData.WorkSchedule;
                doctor.Specializations = oldData.Specializations;
                return new(400, "Doctor validation failed");
            }

            await doctorsRepository.AddOrUpdateAsync(doctor, cancellationToken);
            await doctorsRepository.SaveChangesAsync(cancellationToken);

            return new(200, "OK");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "RQST: failed: unexpected error during {Operation} for doctor {DoctorName}", request.Id is null ? "creation" : "update", request.Name);
            return new(500, "An error occurred while processing your request");
        }
    }
}
