using Microsoft.Extensions.Logging;

namespace ElectronicQueue.Application.Appointments;

public class AddOrUpdateAppointmentCommand(
    IRepository<Appointment> appointmentsRepository,
    IRepository<Doctor> doctorsRepository,
    IRepository<Patient> patientsRepository,
    IValidator<Appointment>? appointmentValidator = null,
    ILogger<AddOrUpdateAppointmentCommand>? logger = null
) : ICommand<AddOrUpdateAppointmentRequest, BaseResponse>
{
    public async Task<BaseResponse> ExecuteAsync(AddOrUpdateAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            bool isAdding = request.Id is null;
            logger?.LogInformation("RQST: {Operation} appointment for DoctorId={DoctorId}, PatientId={PatientId} at {Date}",
                                   isAdding ? "creation" : "update",
                                   request.DoctorId,
                                   request.PatientId,
                                   request.AppointmentDate);

            var allAppointments = await appointmentsRepository.GetAllAsync(cancellationToken);
            Appointment appointment;

            if (isAdding)
            {
                appointment = new Appointment();
            }
            else
            {
                appointment = allAppointments.FirstOrDefault(a => a.Id.Value == request.Id!.Value)!;
                if (appointment is null)
                {
                    logger?.LogWarning("RQST: failed: Appointment ID {AppointmentId} not found", request.Id);
                    return new(404, "Appointment not found.");
                }
            }

            var allDoctors = await doctorsRepository.GetAllAsync(cancellationToken);
            var doctor = allDoctors.FirstOrDefault(d => d.Id.Value == request.DoctorId);
            if (doctor is null)
            {
                logger?.LogWarning("RQST: failed: Doctor ID {DoctorId} not found", request.DoctorId);
                return new(404, "Doctor not found.");
            }

            var allPatients = await patientsRepository.GetAllAsync(cancellationToken);
            var patient = allPatients.FirstOrDefault(p => p.Id.Value == request.PatientId);
            if (patient is null)
            {
                logger?.LogWarning("RQST: failed: Patient ID {PatientId} not found", request.PatientId);
                return new(404, "Patient not found.");
            }

            var oldData = (appointment.AppointmentDate, appointment.ClinicalRecords, appointment.Doctor, appointment.Patient);

            appointment.AppointmentDate = request.AppointmentDate;
            appointment.ClinicalRecords = request.ClinicalRecords;
            appointment.Doctor = doctor;
            appointment.Patient = patient;

            if (appointmentValidator is not null && !await appointmentValidator.ValidateAsync(appointment, cancellationToken))
            {
                logger?.LogWarning("RQST: failed: validation failed for appointment");
                appointment.AppointmentDate = oldData.AppointmentDate;
                appointment.ClinicalRecords = oldData.ClinicalRecords;
                appointment.Doctor = oldData.Doctor;
                appointment.Patient = oldData.Patient;
                return new(400, "Appointment validation failed");
            }

            await appointmentsRepository.AddOrUpdateAsync(appointment, cancellationToken);
            await appointmentsRepository.SaveChanges(cancellationToken);

            return new(200, "OK");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "RQST: failed: unexpected error during {Operation} for appointment",
                             request.Id is null ? "creation" : "update");
            return new(500, "An error occurred while processing your request");
        }
    }
}
