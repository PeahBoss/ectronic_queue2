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

        // Получаем все приемы
        var allAppointments = await appointmentsRepository.GetAllAsync(cancellationToken);
        Appointment appointment = null!;

        if (isAdding)
        {
            appointment = new Appointment();
        }
        else
        {
            var potentialAppointment = allAppointments.FirstOrDefault(a => a.Id.Value == request.Id!.Value);
            if (potentialAppointment is null)
                return new(404, "Appointment not found.");
            else
                appointment = potentialAppointment;
        }

        // Получаем врача
        var allDoctors = await doctorsRepository.GetAllAsync(cancellationToken);
        var doctor = allDoctors.FirstOrDefault(d => d.Id.Value == request.DoctorId);
        if (doctor is null)
            return new(404, "Doctor not found.");

        // Получаем пациента
        var allPatients = await patientsRepository.GetAllAsync(cancellationToken);
        var patient = allPatients.FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null)
            return new(404, "Patient not found.");

        // Обновление данных
        appointment.AppointmentDate = request.AppointmentDate;
        appointment.ClinicalRecords = request.ClinicalRecords;
        appointment.Doctor = doctor;
        appointment.Patient = patient;

        if (isAdding)
        {
            await appointmentsRepository.AddAsync(appointment, cancellationToken);
        }

        await appointmentsRepository.SaveChanges(cancellationToken);
        return new(200, "OK");
    }
}

