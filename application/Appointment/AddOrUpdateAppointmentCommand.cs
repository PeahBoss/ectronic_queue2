namespace ElectronicQueue.Application.Appointments;

public class AddOrUpdateAppointmentCommand(
    IRepository<Appointment> appointmentsRepository,
    IRepository<Doctor> doctorsRepository,
    IRepository<Patient> patientsRepository
) : ICommand<AddOrUpdateAppointmentRequest, BaseResponse>
{
    public async Task<BaseResponse> ExecuteAsync(AddOrUpdateAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        bool isAdding = request.Id is null;

        var allAppointments = await appointmentsRepository.GetAllAsync(cancellationToken);
        Appointment? appointment = null;

        if (!isAdding)
        {
            appointment = allAppointments.FirstOrDefault(a => a.Id.Value == request.Id!.Value);
            if (appointment is null)
                return new(404, "Appointment not found.");

            appointment.AppointmentDate = request.AppointmentDate;
            appointment.ClinicalRecords = request.ClinicalRecords;
        }

        var allDoctors = await doctorsRepository.GetAllAsync(cancellationToken);
        var doctor = allDoctors.FirstOrDefault(d => d.Id.Value == request.DoctorId);
        if (doctor is null)
            return new(404, "Doctor not found.");

        var allPatients = await patientsRepository.GetAllAsync(cancellationToken);
        var patient = allPatients.FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null)
            return new(404, "Patient not found.");

        if (isAdding)
        {
            appointment = new Appointment
            {
                AppointmentDate = request.AppointmentDate,
                ClinicalRecords = request.ClinicalRecords,
                Doctor = doctor,
                Patient = patient
            };

            await appointmentsRepository.AddAsync(appointment, cancellationToken);
        }
        else
        {
            appointment.Doctor = doctor;
            appointment.Patient = patient;
        }

        await appointmentsRepository.SaveChanges(cancellationToken);
        return new(200, "OK");
    }
}
